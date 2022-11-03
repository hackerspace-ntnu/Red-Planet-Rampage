using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerStateController : MonoBehaviour
{
    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerStateController killer, PlayerStateController victim);

    public HitEvent onDeath;

    private FPSInputManager fpsInput;

    /// <summary>
    /// Function for setting a playerInput and adding movement related listeners to it.
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayerInput(FPSInputManager player)
    {
        fpsInput = player;
        // TODO Don't commit sudoku.
        fpsInput.onFire += (ctx) => onDeath?.Invoke(this, this);
    }
}
