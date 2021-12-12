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


    public enum DrawMode { NoiseMap, ColorMap, Mesh, FallOffMap };
    public const int MAP_CHUNK_SIZE = 241;

    [SerializeField] DrawMode _drawMode = DrawMode.NoiseMap;
    [Header("Procedural generations")]
    [SerializeField] float _noiseScale = 2;
    [SerializeField] int _octaves = 4;
    [Range(0f, 1f)]
    [SerializeField] float _persistance = 0.5f;
    [SerializeField] float _lacunarity = 0.2f;
    [SerializeField] int _seed = 1;
    [SerializeField] Vector2 _offset = new Vector2(0.1f, 0.1f);
    [SerializeField] bool _useFallOffMap = false;

    [Header("Mesh")]
    [SerializeField] [Range(0, 6)] int _editorPreviewLOD = 0;
    [SerializeField] float _meshHeightMultiplier = 2f;
    [SerializeField] AnimationCurve _meshHeightCurve = null;
    [SerializeField] NormalizeMode _normalizeMode = NormalizeMode.Local;
    [SerializeField]
    [FormerlySerializedAs("_terrainTypes")]
    TerrainType[] _regions;

    float[,] _fallOffMap;

    Queue<MapThreadInfo<MapData>> _mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> _meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

    private float[,] FallOffMap
    {
        get
        {
            if (_fallOffMap == null)
                _fallOffMap = FallOffGenerator.GenerateFallOffMap(MAP_CHUNK_SIZE);
            return _fallOffMap;
        }
    }


    public bool AutoUpdate;

    private void Awake()
    {
        //  _fallOffMap = FallOffGenerator.GenerateFallOffMap(MAP_CHUNK_SIZE);
    }

    private MapData GenerateMapData(Vector2 centre)
    {
        var noiseMap = Noise.GenerateNoiseMap(
            MAP_CHUNK_SIZE,
            MAP_CHUNK_SIZE,
            _seed,
            _noiseScale,
            _octaves,
            _persistance,
            _lacunarity,
            _offset + centre,
            _normalizeMode);

        Color[] colorMap = new Color[MAP_CHUNK_SIZE * MAP_CHUNK_SIZE];
        for (int y = 0; y < MAP_CHUNK_SIZE; y++)
        {
            for (int x = 0; x < MAP_CHUNK_SIZE; x++)
            {
                if (_useFallOffMap)
                {
                    noiseMap[x, y] = Mathf.Clamp01(noiseMap[x, y] - FallOffMap[x, y]);
                }
                float currentHeight = noiseMap[x, y];
                foreach (var region in _regions)
                {
                    if (currentHeight >= region.Height)
                    {
                        colorMap[y * MAP_CHUNK_SIZE + x] = region.Color;
                    }
                    else
                    {

                        break; // reach a value which is less then the regions height   
                    }
                }
            }
        }

        var mapData = new MapData(noiseMap, colorMap);

        return mapData;
    }

    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData(Vector2.zero);
        var mapDisplay = FindObjectOfType<MapDisplay>();
        var noiseTexture = TextureGenerator.TextureFromHeightMap(mapData.HeightMap);
        var fallOffTexture = TextureGenerator.TextureFromHeightMap(FallOffMap);
        var colorTexture = TextureGenerator.TextureFromColorMap(mapData.ColorMap, MAP_CHUNK_SIZE, MAP_CHUNK_SIZE);

        switch (this._drawMode)
        {
            case DrawMode.NoiseMap:
                mapDisplay.DrawTexture(noiseTexture);
                break;
            case DrawMode.ColorMap:
                mapDisplay.DrawTexture(colorTexture);
                break;
            case DrawMode.Mesh:
                var meshData = MeshGenerator.GenerateTerrainMeshData(mapData.HeightMap, _meshHeightMultiplier, _meshHeightCurve, _editorPreviewLOD);
                mapDisplay.DrawMesh(meshData, colorTexture);
                break;
            case DrawMode.FallOffMap:
                mapDisplay.DrawTexture(fallOffTexture);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate { MeshDataThread(mapData, lod, callback); };
        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        var meshData = MeshGenerator.GenerateTerrainMeshData(mapData.HeightMap, _meshHeightMultiplier, _meshHeightCurve, lod);
        lock (_meshDataThreadInfoQueue)
        {
            _meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate { MapDataThread(centre, callback); };
        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        var mapData = GenerateMapData(centre);
        lock (_mapDataThreadInfoQueue)
        {
            _mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }

    }

    private void Update()
    {
        while (_mapDataThreadInfoQueue.Count > 0)
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
