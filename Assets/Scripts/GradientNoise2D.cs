using UnityEngine;

public class GradientNoise2D : INoise2D {
    private readonly uint seed;

    public GradientNoise2D(int seed) {
        this.seed = (uint)seed;
    }

    // lowbias32 by Chris Wellons, refined by TheIronBorn
    private static uint Hash(uint x) {
        x = (x ^ (x >> 16)) * 0x21f0aaadU;
        x = (x ^ (x >> 15)) * 0x735a2d97U;
        return x ^ (x >> 15);
    }

    // Combine 2D integer coordinates + seed into a single hash
    private static uint Hash2(int xi, int zi, uint seed) {
        // Cast through uint preserves the bit pattern for negative ints (two's complement)
        return Hash((uint)xi ^ Hash((uint)zi ^ seed));
    }

    // uint -> uniform float in [0, 1) using top 24 mantissa bits
    private const float TwoPow_Neg24 = 1f / 16777216f; // 2^-24
    private static float U2F(uint x) => (x >> 8) * TwoPow_Neg24;

    // Quintic fade: 6t^5 - 15t^4 + 10t^3
    private static float Fade(float t) => ((6f * t - 15f) * t + 10f) * t * t * t;

    // Random unit gradient: any direction on the unit circle
    private static Vector2 Grad(int xi, int zi, uint seed) {
        float angle = U2F(Hash2(xi, zi, seed)) * (2f * Mathf.PI);
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public float Sample(float x, float z) {
        int xi = Mathf.FloorToInt(x);
        int zi = Mathf.FloorToInt(z);
        float xf = x - xi;
        float zf = z - zi;

        Vector2 g00 = Grad(xi,     zi,     seed);
        Vector2 g10 = Grad(xi + 1, zi,     seed);
        Vector2 g01 = Grad(xi,     zi + 1, seed);
        Vector2 g11 = Grad(xi + 1, zi + 1, seed);

        // Dot each gradient with its corner-to-point vector
        float v00 = g00.x * xf        + g00.y * zf;
        float v10 = g10.x * (xf - 1f) + g10.y * zf;
        float v01 = g01.x * xf        + g01.y * (zf - 1f);
        float v11 = g11.x * (xf - 1f) + g11.y * (zf - 1f);

        float u = Fade(xf);
        float v = Fade(zf);

        float nx0 = Mathf.Lerp(v00, v10, u);
        float nx1 = Mathf.Lerp(v01, v11, u);
        float n   = Mathf.Lerp(nx0, nx1, v);

        // Raw range is roughly [-√2/2, √2/2]; remap to [0, 1]
        return n * 0.7071f + 0.5f;
    }
}
