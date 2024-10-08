using UnityEngine;

public class ProceduralGenerator
{
    private int seed;
    private float scale;
    private float riverThreshold;
    private float lakeThreshold;

    public ProceduralGenerator(int seed, float scale = 100f, float riverThreshold = 0.6f, float lakeThreshold = 0.55f)
    {
        this.seed = seed;
        this.scale = scale;
        this.riverThreshold = riverThreshold;
        this.lakeThreshold = lakeThreshold;
    }

    /// <summary>
    /// Generates a deterministic noise value based on position.
    /// </summary>
    public float GenerateNoise(Vector2Int pos, int octaves = 4, float persistence = 0.5f, float lacunarity = 2.0f)
    {
        float amplitude = 1;
        float frequency = 1;
        float noiseHeight = 0;
        float maxPossibleHeight = 0;
        for (int i = 0; i < octaves; i++)
        {
            float sampleX = (pos.x + seed) / scale * frequency;
            float sampleY = (pos.y + seed) / scale * frequency;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
            noiseHeight += perlinValue * amplitude;

            maxPossibleHeight += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }
        return noiseHeight / maxPossibleHeight;
    }

    /// <summary>
    /// Determines the elevation of the cell.
    /// </summary>
    public float GetElevation(Vector2Int pos)
    {
        // Base elevation using Perlin noise
        float elevation = GenerateNoise(pos) * 0.5f + 0.5f; // Normalize to [0,1]
        return elevation;
    }

    /// <summary>
    /// Determines the moisture of the cell.
    /// </summary>
    public float GetMoisture(Vector2Int pos)
    {
        // Different scale for moisture
        float moisture = GenerateNoise(pos, octaves: 2, persistence: 0.5f, lacunarity: 2.0f) * 0.5f + 0.5f; // Normalize to [0,1]
        return moisture;
    }

    /// <summary>
    /// Determines the type of LandObject based on elevation and moisture.
    /// </summary>
    public LandObject DetermineLandObject(Vector2Int pos, LandObject ocean, LandObject land, LandObject water)
    {
        float elevation = GetElevation(pos);
        float moisture = GetMoisture(pos);

        // Define sea level
        float seaLevel = 0.5f;

        if (elevation < seaLevel)
        {
            // Ocean
            return ocean;
        }
        else
        {
            // Land
            // Determine if this cell should have a river or lake
            // Simple logic: based on moisture and elevation
            if (moisture > riverThreshold && elevation < lakeThreshold)
            {
                return water; // Lake or river
            }
            else
            {
                return land;
            }
        }
    }
}
