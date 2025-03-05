using UnityEngine;

public class GunInfo : MonoBehaviour
{
    [SerializeField]
    private GunPartDescription bodyDescription;

    [SerializeField]
    private GunPartDescription barrelDescription;

    [SerializeField]
    private GunPartDescription extensionDescription;

    public void UpdateInfo(PlayerIdentity player)
    {
        bodyDescription.UpdateDescription(player.Body);
        barrelDescription.UpdateDescription(player.Barrel);

        if (player.Extension)
        {
            extensionDescription.gameObject.SetActive(true);
            extensionDescription.UpdateDescription(player.Extension);
        }
        else
        {
            extensionDescription.gameObject.SetActive(false);
        }
    }
}
