using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class EndlessTerrain : MonoBehaviour
{

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;

    }

    const float Viewer_Move_Threshold_For_Chunk_Update = 25f;
    const float Squared_Viewer_Move_Threshold_For_Chunk_Update = Viewer_Move_Threshold_For_Chunk_Update * Viewer_Move_Threshold_For_Chunk_Update;

    public Transform viewer;
    public Material MapMaterial;
    public LODInfo[] DetailLevels;
    public static float MaxViewDistance;

    public static Vector2 ViewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;
    static MapGenerator _mapGenerator;
    Vector2 _viewerPositionOld;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        MaxViewDistance = DetailLevels.Last().visibleDistanceThreshold;
        _mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.Map_Chunk_Size - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(MaxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        ViewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if ((_viewerPositionOld - ViewerPosition).sqrMagnitude > Squared_Viewer_Move_Threshold_For_Chunk_Update)
        {

            _viewerPositionOld = ViewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {

        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(ViewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(ViewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord,  new TerrainChunk(viewedChunkCoord, chunkSize, DetailLevels, transform, MapMaterial));
                }

            }
        }
    }
}