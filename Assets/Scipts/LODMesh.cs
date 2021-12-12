using UnityEngine;

public partial class EndlessTerrain
{
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        public int lod;
        public System.Action UpdateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.UpdateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            _mapGenerator.RequestMeshData(mapData, lod, (meshData) => { 
                mesh = meshData.CreateMesh(); 
                hasMesh = true;
                UpdateCallback();
              });
        }
    }
}