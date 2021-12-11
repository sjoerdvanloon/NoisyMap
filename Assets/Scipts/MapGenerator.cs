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

    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    const int MapChunkSize = 241;

    [SerializeField] DrawMode _drawMode = DrawMode.NoiseMap;
    [Header("Procedural generations")]
    [SerializeField] float _noiseScale = 2;
    [SerializeField] int _octaves = 4;
    [Range(0f, 1f)]
    [SerializeField] float _persistance = 0.5f;
    [SerializeField] float _lacunarity = 0.2f;
    [SerializeField] int _seed = 1;
    [SerializeField] Vector2 _offset = new Vector2(0.1f, 0.1f);

    [Header("Mesh")]
    [SerializeField] [Range(0, 6)] int _detailLevel = 0;
    [SerializeField] float _meshHeightMultiPlier = 2f;
    [SerializeField] AnimationCurve _meshHeightCurve = null;
    [SerializeField]
    [FormerlySerializedAs("_terrainTypes")] 
    TerrainType[] _regions;


    public bool AutoUpdate;

    public void GenerateMap()
    {

        var noiseMap = Noise.GenerateNoiseMap(
            MapChunkSize,
            MapChunkSize,
            _seed,
            _noiseScale,
            _octaves,
            _persistance,
            _lacunarity,
            _offset);

        Color[] colorMap = new Color[MapChunkSize * MapChunkSize];
        for (int y = 0; y < MapChunkSize; y++)
        {
            for (int x = 0; x < MapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                foreach (var region in _regions)
                {
                    if (currentHeight <= region.Height)
                    {
                        colorMap[y * MapChunkSize + x] = region.Color;
                        break;
                    }
                }
            }
        }

        var mapDisplay = FindObjectOfType<MapDisplay>();
        var noiseTexture = TextureGenerator.TextureFromHeightMap(noiseMap);
        var colorTexture = TextureGenerator.TextureFromColorMap(colorMap, MapChunkSize, MapChunkSize);


        switch (this._drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.DrawTexture(noiseTexture);
                break;
            case DrawMode.ColorMap:
                mapDisplay.DrawTexture(colorTexture);
                break;
            case DrawMode.Mesh:
                var meshData = MeshGenerator.GenerateTerrainMeshData(noiseMap, _meshHeightMultiPlier, _meshHeightCurve, _detailLevel);
                mapDisplay.DrawMesh(meshData, colorTexture);
                break;
            default:
                throw new NotImplementedException();
        }


    }

    private void OnValidate()
    {
        if (_lacunarity < 1)
            _lacunarity = 1;

        if (_octaves < 0)
            _octaves = 0;
    }
}
