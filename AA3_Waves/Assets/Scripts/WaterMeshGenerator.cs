using UnityEngine;

/// <summary>
/// WaterMeshGenerator.cs
/// Genera una malla plana de agua por código con resolución configurable.
/// Coloca este script en un GameObject vacío — creará su propio MeshFilter y MeshRenderer.
/// 
/// La malla se genera en el plano XZ (Y=0), centrada en el origen local.
/// GerstnerWave y SinusoidalWave deformarán esta malla en runtime.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class WaterMeshGenerator : MonoBehaviour
{
    [Header("Dimensiones")]
    [Tooltip("Ancho total de la malla en metros (eje X)")]
    public float width = 20f;

    [Tooltip("Largo total de la malla en metros (eje Z)")]
    public float length = 20f;

    [Header("Resolución")]
    [Tooltip("Número de vértices por fila (eje X). Más = más detalle, más coste.")]
    [Range(2, 250)]
    public int resolutionX = 100;

    [Tooltip("Número de vértices por columna (eje Z). Más = más detalle, más coste.")]
    [Range(2, 250)]
    public int resolutionZ = 100;

    [Header("Material")]
    [Tooltip("Material de agua (asignar en Inspector). Si se deja vacío se usa uno por defecto.")]
    public Material waterMaterial;

    // ?? Unity ????????????????????????????????????????????????????????????????
    void Awake()
    {
        GenerateMesh();
    }

    // ?? Generación ???????????????????????????????????????????????????????????

    /// <summary>
    /// Genera y asigna la malla al MeshFilter del GameObject.
    /// Se puede llamar también desde el Editor si quieres previsualizar.
    /// </summary>
    public void GenerateMesh()
    {
        Mesh mesh = BuildMesh();
        mesh.name = "WaterMesh";

        GetComponent<MeshFilter>().mesh = mesh;

        // Asignar material si hay uno; si no, usar el Standard por defecto
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (waterMaterial != null)
            mr.material = waterMaterial;
        else if (mr.sharedMaterial == null)
            mr.material = new Material(Shader.Find("Standard"));
    }

    private Mesh BuildMesh()
    {
        // Número de quads = (resX-1) * (resZ-1)
        // Número de vértices = resX * resZ
        int vertCount = resolutionX * resolutionZ;

        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[(resolutionX - 1) * (resolutionZ - 1) * 6];

        float stepX = width / (resolutionX - 1);
        float stepZ = length / (resolutionZ - 1);

        // ?? Vértices y UVs ???????????????????????????????????????????????????
        int vi = 0;
        for (int z = 0; z < resolutionZ; z++)
        {
            for (int x = 0; x < resolutionX; x++)
            {
                float px = x * stepX - width * 0.5f;   // centrado en X
                float pz = z * stepZ - length * 0.5f;   // centrado en Z

                vertices[vi] = new Vector3(px, 0f, pz);
                uvs[vi] = new Vector2((float)x / (resolutionX - 1),
                                           (float)z / (resolutionZ - 1));
                vi++;
            }
        }

        // ?? Triángulos ???????????????????????????????????????????????????????
        // Cada quad se divide en 2 triángulos en sentido horario (winding Unity)
        //
        //  tl??tr
        //  ? ?  ?
        //  bl??br
        //
        int ti = 0;
        for (int z = 0; z < resolutionZ - 1; z++)
        {
            for (int x = 0; x < resolutionX - 1; x++)
            {
                int bl = z * resolutionX + x;
                int br = bl + 1;
                int tl = bl + resolutionX;
                int tr = tl + 1;

                // Triángulo 1: bl, tl, tr
                triangles[ti++] = bl;
                triangles[ti++] = tl;
                triangles[ti++] = tr;

                // Triángulo 2: bl, tr, br
                triangles[ti++] = bl;
                triangles[ti++] = tr;
                triangles[ti++] = br;
            }
        }

        // ?? Construir Mesh ???????????????????????????????????????????????????
        Mesh mesh = new Mesh();

        // Para mallas grandes (>65535 vértices) usar 32-bit index
        if (vertCount > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    // ?? Gizmo (previsualización en el Editor) ????????????????????????????????
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 0.6f, 1f, 0.25f);
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(Vector3.zero, new Vector3(width, 0.01f, length));

        Gizmos.color = new Color(0f, 0.6f, 1f, 0.6f);
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(width, 0.01f, length));
    }
}