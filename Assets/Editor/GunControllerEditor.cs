using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(GunController))]
public class GuncControllerEditor : Editor
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