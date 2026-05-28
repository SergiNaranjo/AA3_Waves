using UnityEngine;

/// <summary>
/// Fórmulas (por ola i):
///   x' += (Qi·Ai / ki) · Di.x · cos(ki·(Di·P) - wi·t + ?i)
///   z' += (Qi·Ai / ki) · Di.z · cos(ki·(Di·P) - wi·t + ?i)
///   y' += Ai · sin(ki·(Di·P) - wi·t + ?i)
///
/// donde ki = 2?/Li  y  wi = sqrt(g·ki)  (dispersión de agua profunda)
///       Qi = parámetro de "picudez" (steepness), ? [0,1]
/// </summary>

[RequireComponent(typeof(MeshFilter))]
public class GerstnerWave : MonoBehaviour
{
    // PARAMETROS DE LAS OLAS
    [System.Serializable]
    public struct WaveParams
    {
        [Tooltip("Amplitud (A)")]
        public float amplitud;

        [Tooltip("Longitud de onda (L)")]
        public float waveLength;

        [Tooltip("Dirección de propagación (se normaliza autmáticamente)")]
        public Vector2 direction;

        [Tooltip("Stepness Qi ? [0,1]. 0 = sinusoidal pura; 1 = máxima picudez")]
        [Range(0f, 1f)]
        public float steepness;

        [Tooltip("Fase inicial (?, radianes)")]
        public float phase;
    }

    [Header("Olas de Gerstner")]
    public WaveParams[] waves = new WaveParams[]
    {
        new WaveParams
        {
            amplitud = 0.3f,
            waveLength = 4f,
            direction = new Vector2(1f, 0f),
            steepness = 0.5f,
            phase = 0f
        },
        new WaveParams
        {
            amplitud = 0.15f,
            waveLength = 2.5f,
            direction = new Vector2(0.7f, 0.7f),
            steepness = 0.4f,
            phase = 1.2f
        },
        new WaveParams
        {
            amplitud = 0.1f,
            waveLength = 1.5f,
            direction = new Vector2(-0.5f, 0.8f),
            steepness = 0.3f,
            phase = 2.5f
        }
    };

    [Header("Física")]
    [Tooltip("Gravedad usada en la relación de dispersión (m/s²)")]
    public float gravity = 9.81f;

    [Header("Activity")]
    [Tooltip("Activa / desactivo la simulación en tiempo real")]
    public bool active = true;

    // DATOS PRIVADOS

    private Mesh _mesh;
    private Vector3[] _baseVertices;
    private Vector3[] _modVertices;

    // UNITY
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
                disp += EvaluateGerstner(w, basePos, t);
            }

            _modVertices[i] = basePos + disp;
        }

        _mesh.vertices = _modVertices;
        _mesh.RecalculateNormals();
    }

    /// <summary>
    /// Devuelve la altura del agua en el punto (worldX, worldZ) en el instante actual.
    /// Nota: se usa la posición base XZ para evaluar la ola (aproximación estándar en RT).
    /// </summary>

    public float GetWaterHeight(float worldX, float worldZ)
    {
        Vector3 localPos = transform.InverseTransformPoint(new Vector3(worldX, 0f, worldZ));
        float t = Time.time;
        float y = 0f;

        foreach(WaveParams w in waves)
        {
            y += EvaluateGerstner(w, localPos, t).y;
        }

        return transform.position.y + y;
    }

    // HELPERS PRIVADOS

    /// <summary>
    /// Calcula el desplazamiento (dx, dy, dz) de Gerstner para un vértice dado.
    /// </summary>

    private Vector3 EvaluateGerstner(WaveParams w, Vector3 pos, float t)
    {
        if (w.waveLength <= 0f || w.amplitud <= 0f) return Vector3.zero;

        Vector2 dir = w.direction.normalized;

        float k = 2f * Mathf.PI / w.waveLength; // Numero de onda
        float wi = Mathf.Sqrt(gravity * k); // Frecuencia angular

        float proj = dir.x * pos.x + dir.y * pos.z; // D*P
        float theta = k * proj - wi * t + w.phase; // Argumento de fase

        float sinT = Mathf.Sin(theta);
        float cosT = Mathf.Cos(theta);

        // Desplazamiento horizontal
        float dx = (w.steepness * w.amplitud / k) * dir.x * cosT;
        float dz = (w.steepness * w.amplitud / k) * dir.y * cosT;

        // Desplazamiento vertical
        float dy = w.amplitud * sinT;

        return new Vector3(dx, dy, dz);
    }
}


