using UnityEngine;

/// <summary>
/// Física simplificada de Arquímedes:
///   F_boyancy = ? · g · V_desplazado  (hacia arriba)
///   V_desplazado = volumen del objeto sumergido (aprox. proporcional a la profundidad)
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Buoy : MonoBehaviour
{
    // REFERENCIAS A LOS SIMULADORES DE OLAS
    [Header("Simuladores de olas (asignar en Inspector)")]
    public GerstnerWave gerstnerWave;

    // PARÁMETROS FÍSICO
    [Header("Física de flotabilidad")]

    [Tooltip("Densidad del fluido (agua ? 1000 kg/m³)")]
    public float fluidDensity = 1000f;                  // ?

    [Tooltip("Volumen total del objeto (m³) cuando está completamente sumergido")]
    public float objectVolume = 0.1f;                   // V_total

    [Tooltip("Altura del objeto (para calcular fracción sumergida)")]
    public float objectHeight = 1f;

    [Tooltip("Amortiguación del movimiento vertical en el agua")]
    public float dampingFactor = 2f;

    [Tooltip("Gravedad local (debe coincidir con Physics.gravity.magnitude)")]
    public float gravity = 9.81f;                       // g

    // CONTROL DE QUE OLA USAR
    [Header("Modo activo")]
    public WaveMode activeMode = WaveMode.Gerstner;

    public enum WaveMode { Sinusoidal, Gerstner }

    // DATOS INTERNOS
    private Rigidbody _rb;

    // UNITY
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = true;
    }

    void FixedUpdate()
    {
        float waterHeight = GetWaterHeightAtPosition();
        ApplyBuoyancy(waterHeight);
        ApplyDamping();
    }

    // FÍSICA

    /// <summary>
    /// Calcula y aplica la fuerza de Arquímedes según la fracción sumergida.
    /// </summary>
    private void ApplyBuoyancy(float waterHeight)
    {
        // Posición del centro de la boya
        float objectBottom = transform.position.y - objectHeight * 0.5f;
        float objectTop = transform.position.y + objectHeight * 0.5f;

        // ¿Cuánto está por debajo de la superficie?
        float submergedDepth = Mathf.Clamp(waterHeight - objectBottom, 0f, objectHeight);

        if (submergedDepth <= 0f) return;   // fuera del agua ? solo gravedad

        // Fracción sumergida ? volumen desplazado
        float fractionSubmerged = submergedDepth / objectHeight;
        float volumeDisplaced = fractionSubmerged * objectVolume;   // V_desplazado

        // F = ? · g · V_desplazado  (Principio de Arquímedes)
        float buoyancyForce = fluidDensity * gravity * volumeDisplaced;

        _rb.AddForce(Vector3.up * buoyancyForce, ForceMode.Force);
    }

    /// <summary>
    /// Amortigua la velocidad vertical para evitar oscilación infinita.
    /// </summary>
    private void ApplyDamping()
    {
        Vector3 vel = _rb.linearVelocity;
        float dampForce = -dampingFactor * vel.y;
        _rb.AddForce(new Vector3(0f, dampForce, 0f), ForceMode.Force);
    }

    // ?? Consulta de altura ???????????????????????????????????????????????????

    /// <summary>
    /// Obtiene la altura del agua bajo la boya según el modo activo.
    /// </summary>
    private float GetWaterHeightAtPosition()
    {
        float x = transform.position.x;
        float z = transform.position.z;

        switch (activeMode)
        {
            //case WaveMode.Sinusoidal:
            //    if (sinusoidalWave != null && sinusoidalWave.active)
            //        return sinusoidalWave.GetWaterHeight(x, z);
            //    break;

            case WaveMode.Gerstner:
                if (gerstnerWave != null && gerstnerWave.active)
                    return gerstnerWave.GetWaterHeight(x, z);
                break;
        }

        return 0f;   // fallback: nivel del mar en y=0
    }

    // GIZMOS
    void OnDrawGizmosSelected()
    {
        // Muestra el volumen aproximado de la boya en el Editor
        Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
        Gizmos.DrawCube(transform.position,
                        new Vector3(
                            Mathf.Pow(objectVolume, 1f / 3f),
                            objectHeight,
                            Mathf.Pow(objectVolume, 1f / 3f)));
    }
}
