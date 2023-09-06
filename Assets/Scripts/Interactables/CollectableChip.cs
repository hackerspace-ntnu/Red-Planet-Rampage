using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableChip : MonoBehaviour
{
    [SerializeField]
    private GameObject chipModel;

    void Start()
    {
        // Animate spin and bounce
        LeanTween.sequence()
        .append(LeanTween.moveLocalY(chipModel, 0.08f, 0.5f)
        .setLoopPingPong().setEaseInOutCubic())
        .insert(LeanTween.rotateAroundLocal(chipModel, Vector3.up, chipModel.transform.eulerAngles.y + 360, 1.5f)
        .setLoopType(LeanTweenType.easeInOutCubic).setLoopCount(-1));
    }

}
