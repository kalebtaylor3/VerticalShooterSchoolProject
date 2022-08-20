/// <summary>
/// Turret Enemy using Interactive Body as its parent class
/// (Required because InteractiveBody is abstract.)  The turret body remains stationary. 
/// The turret rotates to face player (turret object uses MoveTowardsPlayer script to continually rotate towards player).
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretEnemy : InteractiveBody
{
    [Header("Turret Settings")]
    [SerializeField]
    protected Transform turretBarrel = null;

    [SerializeField]
    protected float rotationSpeed = 20f;

    [SerializeField]
    [Tooltip("Defines the rate at which to shoot.")]
    private float rateOfFire = 1.5f;
    private float fireTimer = 0f;

    private bool turretEnabled = true;
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        turretBarrel.rotation = Movement.GetRotationTowardsTarget(turretBarrel, GameManager.PlayerTransform);
    }

    protected override void Update()
    {
        base.Update();

        if (GameManager.MainPlayerIsDead() || !turretEnabled || isDead)
        {
            // Do nothing if player is dead.
            return;
        }

        UpdateRotation();

        fireTimer += Time.deltaTime;
        if (fireTimer > rateOfFire)
        {
            // Shoot when it is time.
            Shoot();
            AudioManager.PlayRandomSoundOnSource(shootAudioSource, shootSound, shootSoundPitchVarianceLow, shootSoundPitchVarianceHigh);
            fireTimer = 0f;
        }
    }
    /// <summary>
    /// Updates the rotation for the turret portion of the script
    /// </summary>
    /// <param name="enable"></param>
    private void UpdateRotation()
    {
       Quaternion newRot = Movement.GetRotationTowardsTarget(turretBarrel, GameManager.PlayerTransform);
       turretBarrel.rotation = Quaternion.RotateTowards(turretBarrel.rotation, newRot, rotationSpeed * Time.deltaTime);
    }


    /// <summary>
    /// Enables/Disables the turret movement script and starts/stops guns from rotating.
    /// </summary>
    /// <param name="enable"></param>
    public void EnableTurretGun(bool enable)
    {
        turretEnabled = enable;
    }
}
