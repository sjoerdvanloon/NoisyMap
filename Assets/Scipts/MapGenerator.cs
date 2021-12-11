using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MapGenerator : MonoBehaviour
{
    [System.Serializable]
    public struct TerrainType
    {
        [Range(0f, 1f)]
        public float Height;
        public Color Color;
        public string Name;
    }

    public enum DrawMode { NoiseMap, ColorMap };

    [SerializeField] DrawMode _drawMode = DrawMode.NoiseMap;
    [SerializeField] int _width = 100;
    [SerializeField] int _height = 100;
    [SerializeField] float _noiseScale = 2;
    [SerializeField] int _octaves = 4;
    [Range(0f, 1f)]
    [SerializeField] float _persistance = 0.5f;
    [SerializeField] float _lacunarity = 0.2f;
    [SerializeField] int _seed = 1;
    [SerializeField] Vector2 _offset = new Vector2(0.1f, 0.1f);
    [SerializeField] [FormerlySerializedAs("_terrainTypes")] TerrainType[] _regions;


    public bool AutoUpdate;

    public void GenerateMap()
    {

        var noiseMap = Noise.GenerateNoiseMap(
            _width,
            _height,
            _seed,
            _noiseScale,
            _octaves,
            _persistance,
            _lacunarity,
            _offset);

        Color[] colorMap = new Color[_width * _height];
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float currentHeight = noiseMap[x, y];
                foreach (var region in _regions)
                {
                    if (currentHeight <= region.Height)
                    {
                        colorMap[y * _width + x] = region.Color;
                        break;
                    }
                }
            }
        }

        var mapDisplay = FindObjectOfType<MapDisplay>();

        switch (this._drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColorMap:
                mapDisplay.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, _width, _height));
                break;
            default:
                throw new NotImplementedException();
        }


    }

    private void OnValidate()
    {
        if (_width < 1)
            _width = 1;

        if (_height < 1)
            _height = 1;

        if (_lacunarity < 1)
            _lacunarity = 1;

        if (_octaves < 0)
            _octaves = 0;
    }
}
