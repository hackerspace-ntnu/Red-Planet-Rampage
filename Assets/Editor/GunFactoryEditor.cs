using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GunFactory))]
public class GunFactoryEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GunFactory gunFactory = (GunFactory)target;
        if (GUILayout.Button("Build Gun"))
        {
            gunFactory.InitializeGun();
        }
    }
}
