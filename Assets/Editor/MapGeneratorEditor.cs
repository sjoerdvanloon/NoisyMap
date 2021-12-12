using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var  mapGenerator = (MapGenerator)target;
        var anyValueChanged = DrawDefaultInspector();

        if (anyValueChanged)
        {
            if (mapGenerator.AutoUpdate)
            {
                mapGenerator.DrawMapInEditor();

            }
        }

        if (GUILayout.Button("Generate"))
        {
        mapGenerator.DrawMapInEditor();
        }
    }
}
