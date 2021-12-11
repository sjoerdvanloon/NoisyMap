using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator 
{
    public static MeshData GenerateTerrainMeshData(float[,] heightMap)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);
        float topLeftX = (width-1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        var meshData = new MeshData(width, height);
        var vertexIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var vertexHeight =  heightMap[x, y];
                meshData.Vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ- y);
                meshData.Uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                var isNotOnEdge = (x < width - 1 && y < height - 1);
                if(isNotOnEdge)
                {
                    meshData.AddTraingle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTraingle(vertexIndex + width + 1,vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }
        return meshData;
    }

}

public class MeshData
{
    public Vector3[] Vertices;
    public int[] Triangles;
    public Vector2[] Uv;

    int _triangleIndex= 0;

    public MeshData(int meshWidth, int meshHeight)
    {
        Vertices= new Vector3[meshWidth*meshHeight];
        Uv= new Vector2[meshWidth*meshHeight];
        Triangles= new int[(meshWidth-1)*(meshHeight-1)*6];
    }

    public void AddTraingle(int a, int b, int c)
    {
        Triangles[_triangleIndex] = a;
        Triangles[_triangleIndex+1] = b;
        Triangles[_triangleIndex+2] = c;

        _triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.uv = Uv;
        mesh.RecalculateNormals();

        return mesh;
    }
}