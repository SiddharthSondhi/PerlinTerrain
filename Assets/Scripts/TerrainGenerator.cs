using System.CodeDom.Compiler;
using UnityEngine;

[ExecuteAlways]
public class TerrainGenerator : MonoBehaviour {

    [SerializeField] private int gridSize = 64;
    [SerializeField] private float gridCellSize = 1f;
    [SerializeField] private float frequencyScale = 0.1f;
    [SerializeField] private float amplitudeScale = 10.0f;

    private Mesh mesh;


    private void Start() {
        GenerateTerrain();
    }

    private void OnValidate() {
        gridSize = Mathf.Max(1, gridSize);
        frequencyScale = Mathf.Max(0.0001f, frequencyScale);

        GenerateTerrain();
    }

    private void GenerateTerrain() {
        Vector3[] vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];

        // iterate through grid points, gridsquare + 1 vertices each direction. 
        int i = 0;
        for (int z = 0; z < gridSize + 1; z++) { 
            for (int x = 0; x < gridSize + 1; x++) {
                // get height value from perlin noise
                float perlinNoiseVal = Mathf.PerlinNoise(x * frequencyScale, z * frequencyScale);
                float y = perlinNoiseVal * amplitudeScale;

                vertices[i] = new Vector3((x - gridSize / 2f) * gridCellSize, y, (z - gridSize / 2f) * gridCellSize);
                i++;
            }

        }

        // each grid square has 2 triangles, each triange has 3 vertices, so overall gridsize * 3 * 2 triangles
        int[] triangles = new int[gridSize * gridSize * 6];
        int triangleIndex = 0;

        // iterate through gridpoints
        for (int z = 0; z < gridSize; z++) {
            for (int x = 0; x < gridSize; x++) {
                int bottomLeft = z * (gridSize + 1) + x;
                int bottomRight = bottomLeft + 1;
                int topLeft = bottomLeft + (gridSize + 1);
                int topRight = topLeft + 1;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomRight;
            }
        }

        mesh = new Mesh();

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}
