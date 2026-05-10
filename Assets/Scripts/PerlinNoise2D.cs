using UnityEngine;

public class PerlinNoise2D : INoise2D {
    // 8 unit gradient vectors at 45 degree intervals; (hash & 7) picks one
    private static readonly Vector2[] gradients = {
        new Vector2( 1f,  0f),
        new Vector2(-1f,  0f),
        new Vector2( 0f,  1f),
        new Vector2( 0f, -1f),
        new Vector2( 0.7071f,  0.7071f),
        new Vector2(-0.7071f,  0.7071f),
        new Vector2( 0.7071f, -0.7071f),
        new Vector2(-0.7071f, -0.7071f),
    };

    // 256-entry permutation, duplicated to 512 so perm[perm[xi] + zi] never overflows
    private readonly int[] perm;

    public PerlinNoise2D(int seed) {
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;

        // Fisher-Yates shuffle, deterministic per seed https://www.geeksforgeeks.org/dsa/shuffle-a-given-array-using-fisher-yates-shuffle-algorithm/
        System.Random rng = new System.Random(seed);
        for (int i = 255; i > 0; i--) {
            int j = rng.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }

        perm = new int[512];
        for (int i = 0; i < 512; i++) perm[i] = p[i & 255];
    }

    // quintic fade curve https://rtouti.github.io/graphics/perlin-noise-algorithm
    private static float Fade(float t) => t * t * t * (t * (t * 6f - 15f) + 10f);

    private Vector2 GradAt(int xi, int zi) {
        int hash = perm[perm[xi & 255] + (zi & 255)];
        return gradients[hash & 7];
    }

    public float Sample(float x, float z) {
        int xi = Mathf.FloorToInt(x);
        int zi = Mathf.FloorToInt(z);
        float xf = x - xi;
        float zf = z - zi;

        float n00 = Vector2.Dot(GradAt(xi,     zi),     new Vector2(xf,      zf     ));
        float n10 = Vector2.Dot(GradAt(xi + 1, zi),     new Vector2(xf - 1f, zf     ));
        float n01 = Vector2.Dot(GradAt(xi,     zi + 1), new Vector2(xf,      zf - 1f));
        float n11 = Vector2.Dot(GradAt(xi + 1, zi + 1), new Vector2(xf - 1f, zf - 1f));

        float u = Fade(xf);
        float v = Fade(zf);

        float nx0 = Mathf.Lerp(n00, n10, u);
        float nx1 = Mathf.Lerp(n01, n11, u);
        float n   = Mathf.Lerp(nx0, nx1, v);

        // raw output is in roughly [-sqrt(2)/2, sprt(2)/2]; remap to [0, 1]
        return n * 0.7071f + 0.5f;
    }
}
