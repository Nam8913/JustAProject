using System;
using UnityEngine;

public class ModernHashNoise
{
    private readonly int seed;

    public ModernHashNoise(int seed)
    {
        this.seed = seed;
    }

    // =====================================================
    // HASH CORE
    // =====================================================

    private uint Hash(int x, int y)
    {
        unchecked
        {
            uint h = (uint)(x * 374761393);
            h += (uint)(y * 668265263);
            h += (uint)(seed * 1442695041);

            h ^= h >> 13;
            h *= 1274126177;
            h ^= h >> 16;

            return h;
        }
    }

    private float Hash01(int x, int y)
    {
        return Hash(x, y) / (float)uint.MaxValue;
    }

    // =====================================================
    // SMOOTHING
    // =====================================================

    private float Fade(float t)
    {
        // smootherstep
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    private float Lerp(float a, float b, float t)
    {
        return a + (b - a) * t;
    }

    // =====================================================
    // BASIC PERLIN NOISE 2D
    // =====================================================

    public float PerlinNoise(float x, float y)
    {
        return UnityEngine.Mathf.PerlinNoise(x, y);
    }

    public float PerlinNoise1D(float x)
    {
        return UnityEngine.Mathf.PerlinNoise1D(x);
    }

    // =====================================================
    // BASIC VALUE NOISE 2D
    // =====================================================

    public float Noise(float x, float y)
    {
        int x0 = Mathf.FloorToInt(x);
        int y0 = Mathf.FloorToInt(y);

        int x1 = x0 + 1;
        int y1 = y0 + 1;

        float tx = x - x0;
        float ty = y - y0;

        float u = Fade(tx);
        float v = Fade(ty);

        // 4 góc
        float a = Hash01(x0, y0);
        float b = Hash01(x1, y0);
        float c = Hash01(x0, y1);
        float d = Hash01(x1, y1);

        // nội suy ngang
        float ab = Lerp(a, b, u);
        float cd = Lerp(c, d, u);

        // nội suy dọc
        return Lerp(ab, cd, v);
    }

    // =====================================================
    // FBM
    // =====================================================

    public float FBM(
        float x,
        float y,
        int octaves = 5,
        float lacunarity = 2f,
        float gain = 0.5f)
    {
        float total = 0f;
        float amplitude = 1f;
        float frequency = 1f;
        float maxValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            total += Noise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;

            amplitude *= gain;
            frequency *= lacunarity;
        }

        return total / maxValue;
    }

    // =====================================================
    // RIDGED FBM (núi nhọn)
    // =====================================================

    public float RidgedFBM(
        float x,
        float y,
        int octaves = 5)
    {
        float total = 0f;
        float amplitude = 0.5f;
        float frequency = 1f;

        for (int i = 0; i < octaves; i++)
        {
            float n = Noise(x * frequency, y * frequency);
            n = 1f - Mathf.Abs(n * 2f - 1f);

            total += n * amplitude;

            frequency *= 2f;
            amplitude *= 0.5f;
        }

        return Mathf.Clamp01(total);
    }

    // =====================================================
    // DOMAIN WARP
    // =====================================================

    public float DomainWarp(
        float x,
        float y,
        float warpStrength = 2f)
    {
        float wx = FBM(x + 37.2f, y + 11.7f, 3);
        float wy = FBM(x - 91.4f, y + 53.8f, 3);

        return FBM(
            x + wx * warpStrength,
            y + wy * warpStrength,
            5);
    }
}