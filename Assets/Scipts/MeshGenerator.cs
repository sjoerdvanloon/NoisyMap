using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator 
{
    public static MeshData GenerateTerrainMeshData(float[,] heightMap, float heigthMultiplier, AnimationCurve heightCurve, int detailLevel)
    {
        var width = heightMap.GetLength(0);
        var height = heightMap.GetLength(1);
        float topLeftX = (width-1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (detailLevel == 0) ? 1 : detailLevel * 2;

        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        var meshData = new MeshData(verticesPerLine, verticesPerLine);
        var vertexIndex = 0;
        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                var vertexHeight = heightCurve.Evaluate( heightMap[x, y]) * heigthMultiplier;
                meshData.Vertices[vertexIndex] = new Vector3(topLeftX + x, vertexHeight, topLeftZ- y);
                meshData.Uv[vertexIndex] = new Vector2(x / (float)width, y / (float)height);
                if (x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
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

    public void AddTriangle(int a, int b, int c)
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