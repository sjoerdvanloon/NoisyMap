using UnityEngine;
using System.Linq;

public partial class EndlessTerrain
{
    private class TerrainChunk
    {
        GameObject m_meshObject;
        Vector2 m_Position;
        Bounds m_Bounds;

        MeshRenderer m_MeshRenderer;
        MeshFilter m_meshFilter;
        LODInfo[] detailLevels;
        LODMesh[] lodMeshes;
        MapData mapData;
        bool mapDataReceived;
        int previousLodIndex = -1;

        public TerrainChunk(Vector2 coordinate, int size, LODInfo[] detailLevels, Transform transform, Material material)
        {
            this.detailLevels = detailLevels;

            m_Position = coordinate * size;
            m_Bounds = new Bounds(m_Position, Vector2.one * size);
            var positionV3 = new Vector3(m_Position.x, 0, m_Position.y);

            var name = $"Chunk {coordinate.x}, {coordinate.y}";
            //m_meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            m_meshObject = new GameObject(name);
            m_MeshRenderer = m_meshObject.AddComponent<MeshRenderer>();
            m_MeshRenderer.material = material;
            m_meshFilter = m_meshObject.AddComponent<MeshFilter>();
            //m_meshObject.name = name;

            m_meshObject.transform.position = positionV3;
            //    m_meshObject.transform.localScale = Vector3.one * size / 10; // 10 is default size of plane
            m_meshObject.transform.parent = transform;

            SetVisible(false);

            lodMeshes = this.detailLevels.Select(level => new LODMesh(level.lod,UpdateTerrainChunk)).ToArray();

            _mapGenerator.RequestMapData(m_Position, OnMapDataReceived);

        }

        //public void OnMeshDataReceived(MeshData meshData)
        //{
        //    m_meshFilter.mesh = meshData.CreateMesh();
        //}

        public void UpdateTerrainChunk()
        {
            if (!mapDataReceived)
                return;

            var viewerDstFromNearestEdge = Mathf.Sqrt(m_Bounds.SqrDistance(ViewerPosition));
            bool visible = viewerDstFromNearestEdge <= MaxViewDistance;

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++) // dont look at last
                {
                    if (viewerDstFromNearestEdge > detailLevels[i].visibleDistanceThreshold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (lodIndex != previousLodIndex)
                {
                    var lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.HasMesh)
                    {
                        previousLodIndex = lodIndex;
                        m_meshFilter.mesh = lodMesh.Mesh;
                    }
                    else
                    {
                        if (!lodMesh.HasBeenRequested)
                        {
                            lodMesh.RequestMesh(mapData);
                        }
                    }
                }
            }

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

        public void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;            ////	print("Map data received");
            //_mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            var texture = TextureGenerator.TextureFromColorMap(mapData.ColorMap, MapGenerator.Map_Chunk_Size, MapGenerator.Map_Chunk_Size);
            m_MeshRenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }

    }
}