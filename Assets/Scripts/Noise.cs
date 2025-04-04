using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public static class Noise
{
  private static int[] permutation;

  public static void Initialize(int seed)
  {
    permutation = GeneratePermutation(seed);
  }

  private static void EnsureInitialized()
  {
    if (permutation == null)
    {
      Debug.LogWarning("Noise system was not initialized! Using default seed 0.");
      Initialize(0);
    }
  }

  public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float noiseScale, int seed, int octaves, float persistance, float lacunarity)
  {
    Initialize(seed);
    noiseScale = Mathf.Max(noiseScale, 0.0001f); // makes sure that scale isn't 0 or negative

    float[,] noiseMap = new float[mapWidth, mapHeight];
    float maxNoiseHeight = float.MinValue;
    float minNoiseHeight = float.MaxValue;

    //so it zooms in to the center
    float halfMapWidth = mapWidth / 2f;
    float halfMapHeight = mapHeight / 2f;

    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        float sampleX = (x - halfMapWidth) / noiseScale;
        float sampleY = (y - halfMapHeight) / noiseScale;

        float fractalPerlinValue = FractalBrownianMotion(sampleX, sampleY, octaves, persistance, lacunarity);

        maxNoiseHeight = Mathf.Max(maxNoiseHeight, fractalPerlinValue);
        minNoiseHeight = Mathf.Min(minNoiseHeight, fractalPerlinValue);
        noiseMap[x, y] = fractalPerlinValue;
      }
    }

    for (int y = 0; y < mapHeight; y++)
    {
      for (int x = 0; x < mapWidth; x++)
      {
        noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
      }
    }
    return noiseMap;
  }


  private static Vector2 GetConstantVector(int v)
  {
    switch (v & 3)
    {
        case 0:
            return new Vector2(1.0f, 1.0f);
        case 1:
            return new Vector2(-1.0f, 1.0f);
        case 2:
            return new Vector2(-1.0f, -1.0f);
        case 3:
        default:
            return new Vector2(1.0f, -1.0f);
    }
  }

  private static float Fade(float t)
  {
    return ((6 * t - 15) * t + 10) * t * t * t;
  }

  private static float Lerp(float t, float a1, float a2)
  {
    return a1 + t * (a2 - a1);
  }

  public static float CreatePerlinNoise(float x, float y)
  {
    EnsureInitialized();
    int X = Mathf.FloorToInt(x) & 255;
    int Y = Mathf.FloorToInt(y) & 255;

    float xf = x - Mathf.Floor(x);
    float yf = y - Mathf.Floor(y);

    Vector2 topRight = new Vector2(xf - 1.0f, yf - 1.0f);
    Vector2 topLeft = new Vector2(xf, yf - 1.0f);
    Vector2 bottomRight = new Vector2(xf - 1.0f, yf);
    Vector2 bottomLeft = new Vector2(xf, yf);

    // Select a value from the permutation array for each of the 4 corners
    int valueTopRight = permutation[permutation[X + 1] + Y + 1];
    int valueTopLeft = permutation[permutation[X] + Y + 1];
    int valueBottomRight = permutation[permutation[X + 1] + Y];
    int valueBottomLeft = permutation[permutation[X] + Y];

    float dotTopRight = Vector2.Dot(topRight, GetConstantVector(valueTopRight));
    float dotTopLeft = Vector2.Dot(topLeft, GetConstantVector(valueTopLeft));
    float dotBottomRight = Vector2.Dot(bottomRight, GetConstantVector(valueBottomRight));
    float dotBottomLeft = Vector2.Dot(bottomLeft, GetConstantVector(valueBottomLeft));

    float u = Fade(xf);
    float v = Fade(yf);

    return Lerp(u,
        Lerp(v, dotBottomLeft, dotTopLeft),
        Lerp(v, dotBottomRight, dotTopRight)
    );
  }
  public static float FractalBrownianMotion(float x, float y, int numOctaves, float persistance = 0.5f, float lacunarity = 2.0f, Vector2 offset = default(Vector2))
  {
    Vector2[] octaveOffsets = new Vector2[numOctaves];
    for (int i = 0; i < numOctaves; i++)
    {
      float offsetX = Random.Range(-100000, 100000) + offset.x;
      float offsetY = Random.Range(-100000, 100000) + offset.y;
      octaveOffsets[i] = new Vector2(offsetX, offsetY);
    }

    float result = 0.0f;
    float amplitude = 1.0f;
    float frequency = 0.005f;

    for (int octave = 0; octave < numOctaves; octave++)
    {
      float n = amplitude * (CreatePerlinNoise(x * frequency, y * frequency) * 2 - 1);
      result += n;

      amplitude *= persistance;
      frequency *= lacunarity;
    }

    return result;
  }

  private static int[] GeneratePermutation(int seed)
  {
    int[] p = new int[256];
    for (int i = 0; i < 256; i++) p[i] = i;

    if (seed != null) Random.InitState(seed);

    // Fisher-Yates shuffle
    for (int i = 255; i > 0; i--)
    {
      int swapIndex = Random.Range(0, i + 1);
      
      //Swap values
      int temp = p[i];
      p[i] = p[swapIndex];
      p[swapIndex] = temp;
    }

    // We duplicate the array for seamless wrapping
    int[] perm = new int[512];
    for (int i = 0; i < 512; i++) perm[i] = p[i % 256];

    return perm;
  }  
}