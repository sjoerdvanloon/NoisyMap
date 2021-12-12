using UnityEngine;

[System.Serializable]
public struct TerrainType
{
    public string Name;
    [Range(0f, 1f)]
    public float Height;
    public Color Color;
}
