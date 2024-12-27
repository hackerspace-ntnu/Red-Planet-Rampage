using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CriticalTextAnimator : MonoBehaviour
{
    public TMP_Text textMesh;
    private bool isAnimated = false;
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
                    charVertices[charInfo.vertexIndex + j] = position + new Vector3(0f, Mathf.Sin(Time.time * 16f + position.x * 0.1f) * 1f, 0f);
                    Color32 gradient = ColorPallete.Health(1 - Mathf.Abs(Mathf.Sin(Time.time * 4f + position.x * 0.1f)));
                    textInfo.meshInfo[charInfo.materialReferenceIndex].colors32[charInfo.vertexIndex + j] = gradient;
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
