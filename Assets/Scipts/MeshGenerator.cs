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
        var personalHeightCurve = new AnimationCurve(heightCurve.keys);

        int meshSimplificationIncrement = (detailLevel == 0) ? 1 : detailLevel * 2;

        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        var meshData = new MeshData(verticesPerLine, verticesPerLine);
        var vertexIndex = 0;
        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                var vertexHeight = personalHeightCurve.Evaluate( heightMap[x, y]) * heigthMultiplier;
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

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[Vertices.Length];
        int triangleCount = Triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;
            int vertexIndexA = Triangles[normalTriangleIndex];
            int vertexIndexB = Triangles[normalTriangleIndex+1];
            int vertexIndexC = Triangles[normalTriangleIndex+2];

            var triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;

        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = Vertices[indexA];
        Vector3 pointB = Vertices[indexB];
        Vector3 pointC = Vertices[indexC];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;

        return Vector3.Cross(sideAB, sideAC).normalized;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.uv = Uv;
       // mesh.RecalculateNormals();
       mesh.normals = CalculateNormals();
        return mesh;
    }
}