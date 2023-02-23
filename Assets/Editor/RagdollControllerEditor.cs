using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RagdollController))]
public class RagdollControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RagdollController ragdoll = (RagdollController)target;
        if (GUILayout.Button("Ragdoll"))
        {
            if (Application.isPlaying)
            {
                ragdoll.EnableRagdoll();
            }
        }
    }
}
