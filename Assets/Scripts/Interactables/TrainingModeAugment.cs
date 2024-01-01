using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

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
        augmentModel.transform.localPosition = Vector3.zero;

        switch (item.augmentType)
        {
            case AugmentType.Body:
                augmentModel.GetComponent<GunBody>().enabled = false;
                break;
            case AugmentType.Barrel:
                augmentModel.GetComponent<ProjectileController>().enabled = false;
                augmentModel.GetComponentsInChildren<VisualEffect>().Select(vfx => vfx.enabled = false);
                break;
            case AugmentType.Extension:
                augmentModel.GetComponent<GunExtension>().enabled = false;
                break;
        }
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
