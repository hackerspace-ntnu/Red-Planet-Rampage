using UnityEngine;

public class TrainingModeItem : MonoBehaviour, Interactable
{
    public void Interact(PlayerManager player)
    {
        Debug.Log("INTERACTED WITH!");
    }
}
