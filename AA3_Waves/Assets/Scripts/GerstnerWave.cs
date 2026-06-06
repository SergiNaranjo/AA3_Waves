using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class GerstnerWave : MonoBehaviour
{
    //  MODO DE SIMULACIÓN
    public enum WaveMode { Gerstner, Sinusoidal }

    [Header("Modo de simulación")]
    [Tooltip("Gerstner = desplazamiento XYZ completo | Sinusoidal = solo eje Y")]
    public WaveMode waveMode = WaveMode.Gerstner;

    //  PARÁMETROS DE LAS OLAS
    [System.Serializable]
    public struct WaveParams
    {
        [Tooltip("Amplitud (A)")]
        public float amplitud;

        [Tooltip("Longitud de onda (L)")]
        public float waveLength;

        [Tooltip("Dirección de propagación (se normaliza automáticamente)")]
        public Vector2 direction;

        [Range(0f, 1f)]
        public float steepness;

        [Tooltip("Fase inicial")]
        public float phase;

        [Tooltip("Frecuencia (f, Hz). Solo usado en modo Sinusoidal. " +
                 "En Gerstner se deriva de la relación de dispersión.")]
        public float frequency;
    }

    [Header("Olas")]
    public WaveParams[] waves = new WaveParams[]
    {
        new WaveParams
    {
        amplitud   = 1.5f,
        waveLength = 3f,
        direction  = new Vector2(1f, 0f),
        steepness  = 0.5f,
        phase      = 0f,
        frequency  = 0.4f
    },
    new WaveParams
    {
        amplitud   = 0.8f,
        waveLength = 2f,
        direction  = new Vector2(0.7f, 0.7f),
        steepness  = 0.4f,
        phase      = 1.2f,
        frequency  = 0.6f
    },
    new WaveParams
    {
        amplitud   = 0.5f,
        waveLength = 1.5f,
        direction  = new Vector2(-0.5f, 0.8f),
        steepness  = 0.3f,
        phase      = 2.5f,
        frequency  = 0.8f
    }
    };

    [Header("Física (Gerstner)")]
    [Tooltip("Gravedad usada en la relación de dispersión (m/s^2)")]
    public float gravity = 9.81f;

    [Header("Activity")]
    [Tooltip("Activa / desactiva la simulación en tiempo real")]
    public bool active = true;

    //  DATOS PRIVADOS
    private Mesh _mesh;
    private Vector3[] _baseVertices;
    private Vector3[] _modVertices;

    //  UNITY
    void Start()
    {
        _mesh = GetComponent<MeshFilter>().mesh;
        _baseVertices = _mesh.vertices;
        _modVertices = new Vector3[_baseVertices.Length];
    }

    void Update()
    {
        if (!active) return;

        float t = Time.time;

        for (int i = 0; i < _baseVertices.Length; i++)
        {
            Vector3 basePos = _baseVertices[i];
            Vector3 disp = Vector3.zero;

            foreach (WaveParams w in waves)
            {
                disp += waveMode == WaveMode.Sinusoidal
                    ? EvaluateSinusoidal(w, basePos, t)
                    : EvaluateGerstner(w, basePos, t);
            }

            _modVertices[i] = basePos + disp;
        }

        _mesh.vertices = _modVertices;
        _mesh.RecalculateNormals();
    }

    /// Devuelve la altura del agua en el punto (worldX, worldZ) en el instante actual.
    public float GetWaterHeight(float worldX, float worldZ)
    {
        Vector3 localPos = transform.InverseTransformPoint(new Vector3(worldX, 0f, worldZ));
        float t = Time.time;
        float y = 0f;

        foreach (WaveParams w in waves)
        {
            y += waveMode == WaveMode.Sinusoidal
                ? EvaluateSinusoidal(w, localPos, t).y
                : EvaluateGerstner(w, localPos, t).y;
        }

        return transform.position.y + y;
    }


    //  HELPERS PRIVADOS


    /// <summary>
    /// Modo Gerstner
    /// </summary>
    /// La maya se desplaza en xyz
    private Vector3 EvaluateGerstner(WaveParams w, Vector3 pos, float t)
    {
        if (w.waveLength <= 0f || w.amplitud <= 0f) return Vector3.zero;

        Vector2 dir = w.direction.normalized;
        float k = 2f * Mathf.PI / w.waveLength;
        float wi = Mathf.Sqrt(gravity * k);

        float proj = dir.x * pos.x + dir.y * pos.z;
        float theta = k * proj - wi * t + w.phase;

        float sinT = Mathf.Sin(theta);
        float cosT = Mathf.Cos(theta);

        float amp_k = w.steepness * w.amplitud / k;
        float dx = (amp_k) * dir.x * cosT;
        float dz = (amp_k) * dir.y * cosT;
        float dy = w.amplitud * sinT;

        return new Vector3(dx, dy, dz);
    }

    /// Modo Sinusoidal puro
    /// La maya solo sube y baja
    private Vector3 EvaluateSinusoidal(WaveParams w, Vector3 pos, float t)
    {
        if (w.waveLength <= 0f || w.amplitud <= 0f) return Vector3.zero;

        Vector2 dir = w.direction.normalized;

        float L = w.waveLength;
        float v = w.frequency * L;
        float x = dir.x * pos.x + dir.y * pos.z;

        float dy = w.amplitud * Mathf.Sin((2f * Mathf.PI / L) * (x - v * t) + w.phase);

        return new Vector3(0f, dy, 0f);
    }
}