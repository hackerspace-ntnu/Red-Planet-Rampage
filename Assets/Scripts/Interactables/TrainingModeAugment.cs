using UnityEngine;

public class TrainingModeAugment : MonoBehaviour, Interactable
{
    [SerializeField]
    private Item item;

    public Item Item
    {
        get
        {
            return item;
        }
        set
        {
            item = value;
        }
    }

    [SerializeField]
    private Transform modelHolder;

    private void Start()
    {
        var augmentModel = Instantiate(item.augment, modelHolder);
        augmentModel.transform.localScale = Vector3.one * 2;
        augmentModel.transform.localPosition = -Augment.Midpoint(augmentModel, item.augmentType).localPosition * 2;

        Augment.DisableInstance(augmentModel, item.augmentType);
    }

    public void Interact(PlayerManager player)
    {
        var body = player.identity.Body;
        var barrel = player.identity.Barrel;
        var extension = player.identity.Extension;
        switch (item.augmentType)
        {
            case AugmentType.Body:
                body = item;
                break;
            case AugmentType.Barrel:
                barrel = item;
                break;
            case AugmentType.Extension:
                extension = item;
                break;
        }

        player.identity.SetLoadout(body, barrel, extension);
        player.RemoveGun();
        player.SetGun(player.identity.transform);
    }
}
