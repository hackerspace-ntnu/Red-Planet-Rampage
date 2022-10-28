using UnityEngine;

[CreateAssetMenu(menuName = "Auction/Item")]
public class Item : ScriptableObject
{
    [SerializeField] private string displayName;
}
