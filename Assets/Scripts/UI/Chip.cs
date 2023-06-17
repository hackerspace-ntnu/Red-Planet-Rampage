using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chip : MonoBehaviour
{

    public void AnimateTransaction(int amount, Transform target)
    {
        if (amount == 0)
            return;

        gameObject.LeanMoveLocalY(2f, 0.2f).setLoopCount(Mathf.Abs(amount)).setEaseShake();

        // Create a flying chip animation when players are spending money
        if (amount < 0)
        {
            GameObject flyingChip = Instantiate(gameObject);
            flyingChip.transform.position = transform.position;
            flyingChip.transform.localScale = transform.localScale * 0.05f;
            flyingChip.transform.rotation = transform.rotation;
            flyingChip.LeanMove(target.position, 1f).setEaseInOutExpo();
            Destroy(flyingChip, 1f);
        }
    }

}
