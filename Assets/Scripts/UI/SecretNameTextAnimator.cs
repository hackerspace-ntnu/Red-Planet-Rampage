using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SecretNameTextAnimator : MonoBehaviour
{
    public TMP_Text textMesh;
    private bool isAnimated;
    public bool IsAnimated 
    {
        get
        {
            return isAnimated;
        }
        set
        {
            if (value)
                StartCoroutine(AnimateText());
            if (!value)
                StopAllCoroutines();
            isAnimated = value;
        }
    }

    private IEnumerator AnimateText()
    {
        while (true)
        {
            textMesh.ForceMeshUpdate();
            var textInfo = textMesh.textInfo;
            for (int i = 0; i < textInfo.characterCount; i++)
            {
                var charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible)
                    continue;

                var charVertices = textInfo.meshInfo[charInfo.materialReferenceIndex].vertices;
                for (int j = 0; j < 4; j++)
                {
                    var position = charVertices[charInfo.vertexIndex + j];
                    charVertices[charInfo.vertexIndex + j] = position + new Vector3(0f, Mathf.Sin(Time.time * 2f + position.x * 0.01f) * 10f, 0f);
                    Color32 rainbow = Color.HSVToRGB(Mathf.Abs(Mathf.Sin(Time.time * 0.5f + position.x * 0.1f)), 0.8f, 1.0f);
                    textInfo.meshInfo[charInfo.materialReferenceIndex].colors32[charInfo.vertexIndex + j] = rainbow;
                }
            }
            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                var meshInfo = textInfo.meshInfo[i];
                meshInfo.mesh.vertices = meshInfo.vertices;
                textMesh.UpdateGeometry(meshInfo.mesh, i);
            }
            yield return null;
        }
    }
}
