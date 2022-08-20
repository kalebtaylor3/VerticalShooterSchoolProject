/// <summary>
/// Simple enemy which adds the ability to fire at the main player. 
/// </summary>

using UnityEngine;

public class SimpleEnemy : InteractiveBody
{
    [Header("Simple Enemy Settings")]
    [SerializeField]
    protected float rateOfFire = 1.5f;
    private float fireTimer = 0f;

    private MoveTowardsPlayer movement;

    bool playerDied = false;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<MoveTowardsPlayer>();
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
            movement.Speed = movement.Speed * 2f;
            playerDied = true;
        }

        //Wait until location is locked towards player before firing.
        if (movement.RotationLocked) 
        {
            fireTimer += Time.deltaTime;
            if (fireTimer > rateOfFire)
            {
                Shoot();
                AudioManager.PlayRandomSoundOnSource(shootAudioSource, shootSound, shootSoundPitchVarianceLow, shootSoundPitchVarianceHigh);

                fireTimer = 0f;
            }
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
