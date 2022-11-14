using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class PlayerManager : MonoBehaviour
{
    // TODO add context when shooty system is done
    public delegate void HitEvent(PlayerManager killer, PlayerManager victim);

    public HitEvent onDeath;

    public int chips;

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

    new public string ToString()
    {
        return "Player " + fpsInput.playerInput.playerIndex;
    }
}
