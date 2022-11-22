using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerManager : MonoBehaviour
{
    // Layers 12 through 15 are gun layers.
    private static int allGunsMask = (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);

    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerManager killer, PlayerManager victim);

    public HitEvent onDeath;

    public int chips;

    public FPSInputManager fpsInput;

    [SerializeField]
    private GameObject meshBase;

    [SerializeField]
    private List<Item> items;

    private GunController gunController;

    // TODO do this in a better way?
    [SerializeField]
    private GameObject body;
    [SerializeField]
    private GameObject barrel;
    [SerializeField]
    private GameObject extension;

    private HealthController healthController;

    void Start()
    {
        healthController = GetComponent<HealthController>();
        healthController.onDamageTaken += OnDamageTaken;
        healthController.onDeath += OnDeath;
    }

    public void PerformTransaction(Item item, int cost)
    {
        items.Add(item);
        chips -= cost;
    }

    void OnDamageTaken(HealthController healthController, float damage, DamageInfo info)
    {
        Debug.Log(this.ToString() + " took " + damage + " damage from " + info.sourcePlayer.ToString());
    }

    void OnDeath(HealthController healthController, float damage, DamageInfo info)
    {
        Debug.Log(this.ToString() + " was killed by " + info.sourcePlayer.ToString());
        onDeath?.Invoke(info.sourcePlayer, this);
    }

    /// <summary>
    /// Function for setting a playerInput and adding movement related listeners to it.
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayerInput(FPSInputManager player)
    {
        fpsInput = player;
        GetComponent<PlayerMovement>().SetPlayerInput(fpsInput);
        SetGunOffset(fpsInput.transform);
        fpsInput.onFirePerformed += OnFire;
        fpsInput.onFireCanceled += OnFireEnd;
    }

    void OnDestroy()
    {
        healthController.onDamageTaken -= OnDamageTaken;
        healthController.onDeath -= OnDeath;
        fpsInput.onFirePerformed -= OnFire;
        fpsInput.onFireCanceled -= OnFireEnd;
    }

    private void OnFire(InputAction.CallbackContext ctx)
    {
        gunController.triggerHeld = true;
        gunController.triggerPressed = true;
        StartCoroutine(UnpressTrigger());
    }

    IEnumerator UnpressTrigger()
    {
        yield return new WaitForFixedUpdate();
        gunController.triggerPressed = false;
    }

    private void OnFireEnd(InputAction.CallbackContext ctx)
    {
        gunController.triggerHeld = false;
    }

    public void SetLayer(int playerIndex)
    {
        int playerLayer = LayerMask.NameToLayer("Player " + playerIndex);

        // Set layers for the camera to ignore (the other players' gun layers, and this layer)
        // Bitwise negation of this player's model layer and all gun layers that do not belong to this player
        // Gun layers are 4 above their respective player layers.
        var mask = ((1 << 16) - 1) ^ ((1 << playerLayer) | ((1 << (playerLayer + 4)) ^ allGunsMask));
        fpsInput.GetComponent<Camera>().cullingMask = mask;

        // Set correct layer on self, mesh and gun (TODO)
        gameObject.layer = playerLayer;
        SetLayerOnSubtree(meshBase, playerLayer);
    }

    private void SetGunOffset(Transform offset)
    {
        var gun = GunFactory.InstantiateGun(body, barrel, extension, offset);
        // Set specific local transform
        gun.transform.localPosition = new Vector3(0.3f, -0.12f, -0.4f);
        gun.transform.localRotation = Quaternion.AngleAxis(-12.5f, Vector3.up);
        // Remember gun controller
        gunController = gun.GetComponent<GunController>();
        // Make gun remember who shoots with it
        gunController.player = this;
    }

    private void SetLayerOnSubtree(GameObject node, int layer)
    {
        node.layer = layer;
        foreach (Transform child in node.transform)
        {
            SetLayerOnSubtree(child.gameObject, layer);
        }
    }

    new public string ToString()
    {
        return "Player " + fpsInput.playerInput.playerIndex;
    }
}
