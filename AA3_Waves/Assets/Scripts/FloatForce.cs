using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatForce : MonoBehaviour
{
    [Header("Simulador olas")]
    public GerstnerWave gerstnerWave;

    [Header("Flotabilidad")]
    public float fluidDensity = 1000f;
    public float objectVolume = 0.1f;
    public float objectHeight = 1f;
    public float dampingForce = 200f;

    private Rigidbody rb;
    private const float gravity = 9.81f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (gerstnerWave == null)
            return;

        float waterHeight = gerstnerWave.GetWaterHeight(
            transform.position.x,
            transform.position.z);

        float bottom = transform.position.y - objectHeight * 0.5f;

        float submergedDepth = waterHeight - bottom;

        submergedDepth = Mathf.Clamp(submergedDepth, 0f, objectHeight);

        float submergedFraction = submergedDepth / objectHeight;

        float submergedVolume = objectVolume * submergedFraction;

        float buoyancyForce =
            fluidDensity *
            gravity *
            submergedVolume;

        float damping =
            -rb.linearVelocity.y *
            dampingForce;
        Debug.Log($"Depth={submergedDepth}");
        Debug.Log($"Volume={submergedVolume}");
        Debug.Log($"Buoyancy={buoyancyForce}");

        rb.AddForce(
            Vector3.up * (buoyancyForce + damping),
            ForceMode.Force);
    }
}