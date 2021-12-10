using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [SerializeField] int _width = 100;
    [SerializeField] int _height = 100;
    [SerializeField] float _noiseScale = 2;
    [SerializeField] int _octaves= 4;
    [Range(0f, 1f)]
    [SerializeField] float _persistance = 0.5f;
    [SerializeField] float _lacunarity = 0.2f;
    [SerializeField] int _seed = 1;
    [SerializeField] Vector2 _offset = new Vector2(0.1f, 0.1f);


    public bool AutoUpdate;

    public void GenerateMap()
    {
        var mapDisplay = FindObjectOfType<MapDisplay>();
        
        var noiseMap = Noise.GenerateNoiseMap(
            _width,
            _height,
            _seed,
            _noiseScale,
            _octaves,
            _persistance,
            _lacunarity,
            _offset);
        mapDisplay.DrawNoiseMap(noiseMap);
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
