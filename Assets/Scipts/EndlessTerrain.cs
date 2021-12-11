using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EndlessTerrain : MonoBehaviour
{
    private class TerrainChunk
    {
        GameObject m_meshObject;
        Vector2 m_Position;
        Bounds m_Bounds;
        public TerrainChunk(Vector2 coordinate, int size, Transform transform)
        {
            m_Position = coordinate * size;
            m_Bounds = new Bounds(m_Position, Vector2.one * size);
            var positionV3 = new Vector3(m_Position.x, 0, m_Position.y);

            m_meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
			m_meshObject.name = $"Plane {coordinate.x}, {coordinate.y}";

			m_meshObject.transform.position = positionV3;
            m_meshObject.transform.localScale = Vector3.one * size / 10; // 10 is default size
			m_meshObject.transform.parent = transform;

			SetVisible(false);

        }

        public void UpdateTerrainChunk()
        {
            var viewerDstFromNearestEdge = Mathf.Sqrt(m_Bounds.SqrDistance(ViewerPosition));
            bool visible = viewerDstFromNearestEdge <= Max_View_Distance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            m_meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return m_meshObject.activeSelf;
        }
    }

    public const float Max_View_Distance = 450;
	public Transform viewer;

	public static Vector2 ViewerPosition;
	int chunkSize;
	int chunksVisibleInViewDst;

	Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
	List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

	void Start()
	{
		chunkSize = MapGenerator.Map_Chunk_Size - 1;
		chunksVisibleInViewDst = Mathf.RoundToInt(Max_View_Distance / chunkSize);
	}

	void Update()
	{
		ViewerPosition = new Vector2(viewer.position.x, viewer.position.z);
		UpdateVisibleChunks();
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
					terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
				}

			}
		}
	}
}