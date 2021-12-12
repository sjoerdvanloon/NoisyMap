using UnityEngine;

[System.Serializable]
public struct TerrainType
{
    [Range(0f, 1f)]
    public float Height;
    public Color Color;
    public string Name;
}
