using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{

    [SerializeField] Renderer _textureRenderer = null;
    [SerializeField] MeshFilter _meshFilter = null;
    [SerializeField] MeshRenderer _meshRenderer = null;


    public void DrawTexture(Texture2D texture)
    {
       

        // _textureRenderer.material  only instantiated at runtime
        _textureRenderer.sharedMaterial.mainTexture = texture;
        _textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height); 
    }

    internal void DrawMesh(MeshData meshData, Texture2D texture)
    {
        _meshFilter.sharedMesh = meshData.CreateMesh();
        _meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
