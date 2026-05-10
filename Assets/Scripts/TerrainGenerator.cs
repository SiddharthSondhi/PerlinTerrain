using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
public class TerrainGenerator : MonoBehaviour {

    public enum NoiseType { Perlin, Gradient }

    [SerializeField] private int gridSize = 64;
    [SerializeField] private float gridCellSize = 1f;

    [SerializeField] private float initFrequency = 0.1f;
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float offset = 0f;

    [SerializeField] private int octaves = 4;
    [SerializeField] private float freqScaleFactor = 2f;
    [SerializeField] private float amplitudeScaleFactor = 0.5f;

    [SerializeField] private int seed = 0;
    [SerializeField] private NoiseType noiseType = NoiseType.Perlin;

    [SerializeField] private Transform waterPlane;
    [SerializeField] private float waterHeight = 0.3f;

    private Mesh mesh;
    private INoise2D[] noiseLayers;



    private void Start() {
        GenerateTerrain();
    }

    private void OnValidate() {
        gridSize = Mathf.Max(1, gridSize);
        gridCellSize = Mathf.Max(.0001f, gridCellSize);

        initFrequency = Mathf.Max(0.0001f, initFrequency);
        octaves = Mathf.Max(1, octaves);


        GenerateTerrain();
    }

    // build one independent noise field per octave, each seeded from the master RNG
    // so octaves sample fully decorrelated noise
    private void GenerateNoiseLayers() {
        System.Random rng = new System.Random(seed);
        noiseLayers = new INoise2D[octaves];
        for (int i = 0; i < octaves; i++) {
            int layerSeed = rng.Next();
            noiseLayers[i] = noiseType switch {
                NoiseType.Gradient => new GradientNoise2D(layerSeed),
                _                  => new PerlinNoise2D(layerSeed),
            };
        }
    }

    // gets multiple octaves of noise
    private float getOctaveNoise(float x, float z) {
        float totalNoise = 0f;
        float amplitude = 1f;
        float frequency = initFrequency;
        float amplitudeSum = 0f;


        for (int i = 0; i < octaves; i++) {
            float noise = noiseLayers[i].Sample(
                (x + offset) * frequency,
                (z + offset) * frequency
            );

            totalNoise += noise * amplitude;
            amplitudeSum += amplitude;

            // scale frequency up and amplitude down to get more detail
            amplitude *= amplitudeScaleFactor;
            frequency *= freqScaleFactor;
        }

        return totalNoise / amplitudeSum;
    }

    private void GenerateTerrain() {
        GenerateNoiseLayers();

        Vector3[] vertices = new Vector3[(gridSize + 1) * (gridSize + 1)];
        Color[] colors = new Color[vertices.Length];


        // iterate through grid points, gridsquare + 1 vertices each direction. 
        int i = 0;
        for (int z = 0; z < gridSize + 1; z++) { 
            for (int x = 0; x < gridSize + 1; x++) {
                // get height value from perlin noise
                float y = getOctaveNoise(x, z);
                float centeredY = y * 2f - 1f;
                vertices[i] = new Vector3((x - gridSize / 2f) * gridCellSize, centeredY * amplitude, (z - gridSize / 2f) * gridCellSize);

                // set vertex colors based on height
                if (y < waterHeight)
                    colors[i] = new Color(0.153f, 0.882f, 0.91f); // water
                else if (y < 0.4f)
                    colors[i] = new Color(0.8f, 0.7f, 0.5f); // sand
                else if (y < 0.6f)
                    colors[i] = new Color(0.1f, 0.7f, 0.2f); // grass
                else if (y < 0.7f)
                    colors[i] = new Color(0.5f, 0.5f, 0.5f); // rock
                else
                    colors[i] = new Color(1f, 1f, 1f); // snow

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


        // create mesh based on vertices and triangles 
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = mesh;

        //create water plan
        if (waterPlane != null) {
            float terrainSize = gridSize * gridCellSize;
            waterPlane.position = new Vector3(0f, (waterHeight * 2f - 1f) * amplitude, 0f);
            waterPlane.localScale = new Vector3(terrainSize / 10f, 1f, terrainSize / 10f);
        }
    }
}
