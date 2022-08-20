/// <summary>
/// Spiral enemy. Enemy which moves in spiral motion around object with  "CenterObject_Tag".  Does not follow object.
/// Assumes the use of the MoveInSpiral script for movement.  Currenlty does not shoot at player. 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralEnemy : InteractiveBody
{
    [Header("Simple Enemy Settings")]
    private MoveInSpiral movement;          // Movement script. 

    private GameObject centerObject;
    public GameObject CenterObject
    {
        get { return centerObject; }
        set { centerObject = value; }
    }

    bool playerDied = false;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<MoveInSpiral>();
    }

    protected override void Update()
    {
        base.Update();

        if (isDead)
        {
            return;
        }
        if (!playerDied && GameManager.MainPlayer.IsDead)
        {
            playerDied = true;
            movement.SetFlyState(MoveInSpiral.FlyState.FlyOut);
        }
    }

    /// <summary>
    /// Called on completion of death animation
    /// Uses to make any changes in between animation completion and death completion
    /// typically used for disabling movement 
    /// </summary>
    protected override void DeathAnimationComplete()
    {
        base.DeathAnimationComplete();
        if (movement)
        {
            movement.enabled = false;
        }
    }

}
