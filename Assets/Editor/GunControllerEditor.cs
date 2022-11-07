using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GunController))]
public class GunControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GunController myScript = (GunController)target;
        if (GUILayout.Button("Build Gun"))
        {
            myScript.InitializeGun();
        }
    }
}