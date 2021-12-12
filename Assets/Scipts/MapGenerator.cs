using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using System.Threading;

public partial class MapGenerator : MonoBehaviour
{

    public struct MapThreadInfo<T>
    {
        public Action<T> Callback { get; }
        public T parameter { get; }

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            Callback = callback;
            this.parameter = parameter;
        }
    }


    public enum DrawMode { NoiseMap, ColorMap, Mesh };
    public const int Map_Chunk_Size = 241;

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
    [SerializeField] float _meshHeightMultiplier = 2f;
    [SerializeField] AnimationCurve _meshHeightCurve = null;
    [SerializeField]
    [FormerlySerializedAs("_terrainTypes")]
    TerrainType[] _regions;

    Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>> ();
    Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>> ();


    public bool AutoUpdate;

    private MapData GenerateMapData()
    {
        var noiseMap = Noise.GenerateNoiseMap(
            Map_Chunk_Size,
            Map_Chunk_Size,
            _seed,
            _noiseScale,
            _octaves,
            _persistance,
            _lacunarity,
            _offset);

        Color[] colorMap = new Color[Map_Chunk_Size * Map_Chunk_Size];
        for (int y = 0; y < Map_Chunk_Size; y++)
        {
            for (int x = 0; x < Map_Chunk_Size; x++)
            {
                float currentHeight = noiseMap[x, y];
                foreach (var region in _regions)
                {
                    if (currentHeight <= region.Height)
                    {
                        colorMap[y * Map_Chunk_Size + x] = region.Color;
                        break;
                    }
                }
            }
        }

        var mapData = new MapData(noiseMap, colorMap);

        return mapData;
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData();
        var mapDisplay = FindObjectOfType<MapDisplay>();
        var noiseTexture = TextureGenerator.TextureFromHeightMap(mapData.HeightMap);
        var colorTexture = TextureGenerator.TextureFromColorMap(mapData.ColorMap, Map_Chunk_Size, Map_Chunk_Size);

        switch (this._drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.DrawTexture(noiseTexture);
                break;
            case DrawMode.ColorMap:
                mapDisplay.DrawTexture(colorTexture);
                break;
            case DrawMode.Mesh:
                var meshData = MeshGenerator.GenerateTerrainMeshData(mapData.HeightMap, _meshHeightMultiplier, _meshHeightCurve, _detailLevel);
                mapDisplay.DrawMesh(meshData, colorTexture);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void RequestMeshData(MapData mapData, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, callback); };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, Action<MeshData> callback)
    {
        var meshData = MeshGenerator.GenerateTerrainMeshData(mapData.HeightMap, _meshHeightMultiplier, _meshHeightCurve, _detailLevel);
        lock (_meshDataThreadInfoQueue)
        {
            _meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    public void RequestMapData(Action<MapData> callback)
    {
        ThreadStart threadStart = delegate { MapDataThread(callback); };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback)
    {
        var mapData = GenerateMapData();
        lock (_mapDataThreadInfoQueue)
        {
            _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }

    }

    private void Update()
    {
        while(_mapDataThreadInfoQueue.Count > 0)
        {
            var threadInfo = _mapDataThreadInfoQueue.Dequeue();
            threadInfo.Callback(threadInfo.parameter);
        }

        while (_meshDataThreadInfoQueue.Count > 0)
        {
            var threadInfo = _meshDataThreadInfoQueue.Dequeue();
            threadInfo.Callback(threadInfo.parameter);
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
