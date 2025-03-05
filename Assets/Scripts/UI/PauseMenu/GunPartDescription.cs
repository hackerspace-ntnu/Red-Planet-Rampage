using TMPro;
using UnityEngine;

public class GunPartDescription : MonoBehaviour
{
    [SerializeField]
    private TMP_Text title;

    [SerializeField]
    private TMP_Text description;

    public void UpdateDescription(Item item)
    {
        title.text = $"<sprite name=\"{item.augmentType.ToString().ToLower()}_white\"> {item.augmentType}: {item.displayName}";
        description.text = item.displayDescription;
    }
}
