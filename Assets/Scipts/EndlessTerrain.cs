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
        public float visibleDstThreshold;
        public bool useForCollider;

    }
    const float SCALE = 2f;
    const float VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = 25f;
    const float SQUARED_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE = VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE * VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE;

    public Transform viewer;
    public Material MapMaterial;
    public LODInfo[] DetailLevels;
    public static float maxViewDst;

    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDst;
    static MapGenerator _mapGenerator;
    Vector2 _viewerPositionOld;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        maxViewDst = DetailLevels.Last().visibleDstThreshold;
        _mapGenerator = FindObjectOfType<MapGenerator>();
        chunkSize = MapGenerator.MAP_CHUNK_SIZE - 1;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / SCALE;

        if ((_viewerPositionOld - viewerPosition).sqrMagnitude > SQUARED_VIEWER_MOVE_THRESHOLD_FOR_CHUNK_UPDATE)
        {

            _viewerPositionOld = viewerPosition;
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

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord,  new TerrainChunk(viewedChunkCoord, chunkSize, DetailLevels, transform, MapMaterial));
                }

            }
        }
    }
}