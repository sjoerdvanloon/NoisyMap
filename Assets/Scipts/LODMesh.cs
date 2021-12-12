using UnityEngine;

public partial class EndlessTerrain
{
    class LODMesh
    {
        public Mesh Mesh;
        public bool HasBeenRequested;
        public bool HasMesh;
        public int lod;
        public System.Action UpdateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.UpdateCallback = updateCallback;
        }

        public void RequestMesh(MapData mapData)
        {
            HasBeenRequested = true;
            _mapGenerator.RequestMeshData(mapData, lod, (meshData) => { 
                Mesh = meshData.CreateMesh(); 
                HasMesh = true;
                UpdateCallback();
              });
        }
    }
}