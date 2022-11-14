using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GunFactory))]
public class GunFactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GunFactory myScript = (GunFactory)target;
        if (GUILayout.Button("Build Gun"))
        {
            myScript.InitializeGun();
        }
    }
}