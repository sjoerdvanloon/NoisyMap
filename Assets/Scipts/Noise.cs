using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum NormalizeMode { Local, Global }

public static class Noise
{
    public static float[,] GenerateNoiseMap(
        int mapWidth,
        int mapHeight,
        int seed,
        float scale,
        int octaves,
        float persistance,
        float lacunarity,
        Vector2 offset,
        NormalizeMode normalizeMode)
    {

        float maxPossibleGlobalHeight = 0f;
        float amplitude = 1;

        // Prep octaves with offsite
        var prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offSetX = prng.Next(-100000, 100000) + offset.x;
            float offSetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offSetX, offSetY);

            maxPossibleGlobalHeight += amplitude;
            amplitude *= persistance;
        }


        if (scale <= 0)
        {
            scale = 0.0001f;
        }

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float[,] noiseMap = new float[mapWidth, mapHeight];
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                 amplitude = 1; // reused
                float frequency = 1;
                float noiseHeight = 0;

                // Per octave
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - halfHeight + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; // times 2 minus 1 unit means that it also can have negative values
                    //noiseMap[x, y] = perlinValue;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Help range
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;

            }
        }

        // Make all the values to be between 0 .. 1
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    var estimate = 1f;
                    float normalizedHeight = noiseMap[x, y] + 1 / (2f * maxPossibleGlobalHeight / estimate);
                }
            }
        }

        return noiseMap;
    }
}
