using UnityEngine;
using UnityEngine.SceneManagement;

public class XrayModifier : MonoBehaviour, ProjectileModifier
{
    public void Attach(ProjectileController projectile)
    {
        // TODO remove ugly safeguard eventually
        if (projectile.player && projectile.player.inputManager != null && SceneManager.GetActiveScene().name != "TrainingMode")
            projectile.player.inputManager.PlayerCamera.tag = "XrayCamera";
    }

    public void Detach(ProjectileController projectile)
    {
        if (projectile.player && projectile.player.inputManager != null && SceneManager.GetActiveScene().name != "TrainingMode")
            projectile.player.inputManager.PlayerCamera.tag = "Untagged";
    }
}
