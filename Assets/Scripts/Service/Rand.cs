
using System.Collections.Generic;
using UnityEngine;

public static class Rand
{
    static System.Random random = new System.Random();
    public static void SetSeed(int seed)
    {
        random = new System.Random(seed);
    }
    #region Random
    public static T Choice<T>(IList<T> list)
        => list[Range(0, list.Count)];

    public static void Shuffle<T>(IList<T> list) // Fisher-Yates
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public static List<T> ShuffleList<T>(List<T> list, int seed)
    {
        System.Random random = new System.Random(seed);
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
        return list;
    }

    public static int Range(int minInclusive, int maxExclusive)
    {
        return random.Next(minInclusive, maxExclusive);
    }
    public static float Range(float min, float max)
        => (float)(random.NextDouble() * (max - min) + min);

    public static float Value01() // 0..1
        => (float)random.NextDouble();

    public static bool Bool(float probability = 0.5f)
        => Value01() < probability;

    public static UnityEngine.Vector2 InsideUnitCircle()
    {
        float angle = Range(0f, Mathf.PI * 2f);
        float r = Mathf.Sqrt(Value01());
        return new UnityEngine.Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
    }

    public static UnityEngine.Vector2 InsideUnitSquare()
    {
        return InsideUnitRectangle(1f);
    }

    public static UnityEngine.Vector2 InsideUnitRectangle(float width, float height)
    {
        return new UnityEngine.Vector2(Range(0f, width), Range(0f, height));
    }

    public static UnityEngine.Vector2 InsideUnitRectangle(float per = 1f)
    {
        return new UnityEngine.Vector2(Range(0f, per), Range(0f, per));
    }

    public static UnityEngine.Vector2 InsideUnitTriangle()
    {
        float u = Value01();
        float v = Value01();
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }
        return new UnityEngine.Vector2(u, v);
    }

    public static UnityEngine.Vector2 InsideUnitHexagon()
    {
        float u = Value01();
        float v = Value01();
        if (v > Mathf.Sqrt(3f) * u)
        {
            u = 1f - u;
            v = 1f - v;
        }
        return new UnityEngine.Vector2(u, v);
    }

    public static UnityEngine.Vector2 GetRandomDirection()
    {
        float angle = Range(0f, Mathf.PI * 2f);
        return new UnityEngine.Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    #endregion

    #region Seeded Random
    public static int SeededRange(int minInclusive, int maxExclusive, int seed)
    {
        System.Random random = new System.Random(seed);
        return random.Next(minInclusive, maxExclusive);
    }

    public static float SeededRange(float min, float max, int seed)
    {
        System.Random random = new System.Random(seed);
        return (float)(random.NextDouble() * (max - min) + min);
    }

    public static float SeededValue01(int seed) // 0..1
    {
        System.Random random = new System.Random(seed);
        return (float)random.NextDouble();
    }

    public static bool SeededBool(int seed, float probability = 0.5f)
    {
        return SeededValue01(seed) < probability;
    }

    public static UnityEngine.Vector2 SeededInsideUnitCircle(int seed)
    {
        System.Random random = new System.Random(seed);
        float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
        float r = Mathf.Sqrt((float)random.NextDouble());
        return new UnityEngine.Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
    }

    public static UnityEngine.Vector2 SeededInsideUnitSquare(int seed)
    {
        System.Random random = new System.Random(seed);
        return new UnityEngine.Vector2((float)random.NextDouble(), (float)random.NextDouble());
    }

    public static UnityEngine.Vector2 SeededInsideUnitTriangle(int seed)
    {
        System.Random random = new System.Random(seed);
        float u = (float)random.NextDouble();
        float v = (float)random.NextDouble();
        if (u + v > 1f)
        {
            u = 1f - u;
            v = 1f - v;
        }
        return new UnityEngine.Vector2(u, v);
    }

    public static UnityEngine.Vector2 SeededInsideUnitHexagon(int seed)
    {
        System.Random random = new System.Random(seed);
        float u = (float)random.NextDouble();
        float v = (float)random.NextDouble();
        if (v > Mathf.Sqrt(3f) * u)
        {
            u = 1f - u;
            v = 1f - v;
        }
        return new UnityEngine.Vector2(u, v);
    }

    public static UnityEngine.Vector2 SeededGetRandomDirection(int seed)
    {
        System.Random random = new System.Random(seed);
        float angle = (float)(random.NextDouble() * Mathf.PI * 2f);
        return new UnityEngine.Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }
    #endregion
}
