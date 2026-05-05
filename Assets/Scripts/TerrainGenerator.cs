using System.CodeDom.Compiler;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour {
    int gridSize = 64;

    public float frequencyScale = 0.1f;
    public float amplitudeScale = 10.0f;

    private Mesh mesh;


    private void Start() {
        GenerateTerrain();
    }

    [ExecuteAlways]
    private void GenerateTerrain() {
        mesh = new Mesh();

        Vector3[] vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];

        // iterate through grid points, gridsquare + 1 vertices each direction. 
        int i = 0;
        for (int z = 0; z < gridSize + 1; z++) { 
            for (int x = 0; x < gridSize + 1; x++) {
                // get height value from perlin noise
                float perlinNoiseVal = Mathf.PerlinNoise(x * frequencyScale, z * frequencyScale);
                float y = perlinNoiseVal * amplitudeScale;

                vertices[i] = new Vector3(x, y, z);
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

        mesh.vertices = vertices;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
