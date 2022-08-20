/// <summary>
/// Main player component. Inherits from InteractiveBody but adds player specific
/// properties like player spawn rules, extra shot types.
/// </summary>

using System.Collections;
using UnityEngine;

public enum ShotType { REGULAR, THREE, LASER, CANNON, SCATTER };

public class MainPlayer : InteractiveBody
{
    public const int MAX_LIVES = 4;

    [Header("Player Spawn Settings")]
    private int numLives = 3;
    [SerializeField]
    private int numStartingLives = 3;
    public int NumLives
    {
        get { return numLives; }
    }

    [SerializeField]
    private bool infiniteLives = true;
    [SerializeField]
    private float invincibilityCoolDown = 10f;
    private float currentInvincibilityTime = 0;
    private bool invincibleAtStart = false;
    public GameObject invincibleUI;
    [SerializeField]
    private float respawnDelay = 2f;
    private Vector3 respawnPos;

    [Header("Player Shoot Settings")]
    [SerializeField]
    [Tooltip("Shot types should be added in the exact order: Regular, ThreeShot, Laser, Cannon, Scatter")]
    private GameObject[] shotTypePrefabs;
    private ShotType currentShotType = ShotType.REGULAR;

    [Header("Player Parts Settings")]
    [SerializeField]
    [Tooltip("Game Object references for all extra player parts.")]
    private GameObject[] extraParts;
    [SerializeField]
    [Tooltip("Game Object references cannon firing parts.")]
    private GameObject[] cannonEmitters;
    [SerializeField]
    [Tooltip("Game Object thrusters.")]
    private GameObject[] thrusters;
    [SerializeField]
    private float maxThrustAngle = 15.0f;
    [SerializeField]
    private float minThrustAngle = -15.0f;
    [SerializeField]
    private float thrusterRotationSpeed = 2.0f;
    private Quaternion leftThrustMax;                                       //Max rotation for left 
    private Quaternion rightThrustMax;                                      //Max rotation for right      
    private Quaternion neutralThrustMax;

    [Header("Player Sound Settings")]
    [SerializeField]
    private AudioClip threeShotSound;
    [SerializeField]
    private AudioClip laserShotSound;
    [SerializeField]
    private AudioClip cannonShotSound;
    [SerializeField]
    private AudioClip scatterShotSound;

    [Header("FX Settings")]
    [SerializeField]
    private GameObject invincibilityGO ;
    [SerializeField]
    private ParticleSystem[] playerFX;


    [Header("Debug Settings")]
    [SerializeField]
    private bool testCannons = false;
   [SerializeField]
    [Tooltip("Shot types should be added in the exact order: Regular, ThreeShot, Laser, Cannon, Scatter")]
    private GameObject testCannonPrefab;

    private PlayerMovement movement;
    private int maxHitPoints;
    private bool hasPickUpAnimParameter = false;
    private bool outOfLives = false;


    //New Variables
    [Header("Bomb Settings")]
    [SerializeField]
    public bool hasBomb = false;
    public GameObject bombPrefab;
    public GameObject bombEmitter;

    public bool OutOfLives
    {
        get { return outOfLives; }
    }

    protected override void Awake()
    {
        base.Awake();
        invincibleAtStart = invincible;
        ToggleInvincibility(invincible);
        respawnPos = transform.position;
        movement = GetComponent<PlayerMovement>();
        maxHitPoints = HitPoints;
        numLives = numStartingLives;

        leftThrustMax = Quaternion.AngleAxis(maxThrustAngle, new Vector3(0, 0, 1));
        rightThrustMax = Quaternion.AngleAxis(minThrustAngle, new Vector3(0, 0, 1));
        neutralThrustMax = Quaternion.AngleAxis(0f, new Vector3(0, 0, 1));

        //check for pick up anim
        if (animator)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Pickup")
                {

                    hasPickUpAnimParameter = true;
                }
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!GameManager.GameOver())
        {
            RotateCannons();
            CheckForFire();
            CheckInvincibility();
        }
    }

    /// <summary>
    /// Rotate the cannons on each frame.
    /// </summary>
    private void RotateCannons()
    {
        Quaternion rotateDir = neutralThrustMax; //Assume neutral rotation.
        float dx = movement.NormalizedVelocity.x;
        if (dx < 0)
        {
            //If we are moving to the left rotate thrusters to left.
            rotateDir = leftThrustMax;
        }
        else if (dx > 0)
        {
            //If we are moving to the right rotate thruster to the right
            rotateDir = rightThrustMax;
        }

        for(int i =0; i < thrusters.Length; i++)
        {
            thrusters[i].transform.rotation = Quaternion.Slerp(thrusters[i].transform.rotation, rotateDir, Time.deltaTime * thrusterRotationSpeed);
        }    
    }

    /// <summary>
    /// Checks "Fire1" input to fire shot. Also fires different shot types.
    /// </summary>
    private void CheckForFire()
    {
        if (isDead)
        {
            return;
        }

        //Check if user is pressing the Fire button.
        if (Input.GetButtonDown("Fire1"))
        {
            if (currentShotType != ShotType.CANNON && currentShotType != ShotType.SCATTER && !testCannons)
            {
                AudioClip currShootSound = shootSound;
                if (currentShotType == ShotType.THREE)
                {
                    currShootSound = threeShotSound;
                }
                else if (currentShotType == ShotType.LASER)
                {
                    currShootSound = laserShotSound;
                }

                if (currentShotType == ShotType.REGULAR)
                {
                    Shoot(regularBullet);
                }
                else
                {
                    Shoot(shotTypePrefabs[(int)currentShotType]);
                }
                
               
                AudioManager.PlayRandomSoundOnSource(shootAudioSource, currShootSound, shootSoundPitchVarianceLow, shootSoundPitchVarianceHigh);
            }
            else if (currentShotType == ShotType.SCATTER)
            {
                FireScatter();
            }
            else if (currentShotType == ShotType.CANNON || testCannons)
            {
                FireCannon();
            }
        }

        if (Input.GetButtonDown("Fire2") && hasBomb)
        {
            DropBomb();
        }
    }

    /// <summary>
    /// Fire the scatter shot.
    /// </summary>
    private void FireScatter()
    {
        GameObject upBullet = Shoot(shotTypePrefabs[(int)currentShotType], shotEmitter.transform);
        ChangeBulletDirection(upBullet, new Vector2(0f, 1f));
        GameObject rightBullet = Shoot(shotTypePrefabs[(int)currentShotType], shotEmitter.transform);
        ChangeBulletDirection(rightBullet, new Vector2(0.4f, 0.5f));
        GameObject leftBullet = Shoot(shotTypePrefabs[(int)currentShotType], shotEmitter.transform);
        ChangeBulletDirection(leftBullet, new Vector2(-0.4f, 0.5f));

        AudioManager.PlayRandomSoundOnSource(shootAudioSource, scatterShotSound, shootSoundPitchVarianceLow, shootSoundPitchVarianceHigh);
    }

    void DropBomb()
    {
        Instantiate(bombPrefab, bombEmitter.transform.position, Quaternion.identity);
        hasBomb = false;
    }

    /// <summary>
    /// Fire the cannon shot.
    /// </summary>
    private void FireCannon()
    {
        for(int i = 0; i < cannonEmitters.Length; i++)
        {
            GameObject cannonBullet;
            if (testCannons)
            {
                cannonBullet = Shoot(testCannonPrefab, cannonEmitters[i].transform);

            }
            else
            {
                cannonBullet = Shoot(shotTypePrefabs[(int)currentShotType], cannonEmitters[i].transform);
            }
            
            ChangeBulletDirection(cannonBullet, cannonEmitters[i].transform.up);
        }

        AudioManager.PlayRandomSoundOnSource(shootAudioSource, cannonShotSound, shootSoundPitchVarianceLow, shootSoundPitchVarianceHigh);

    }

    /// <summary>
    /// Sets the bullet that is passed in to have the direction parameter. The bullet requires a MoveInDirection component.
    /// </summary>
    private void ChangeBulletDirection(GameObject bullet, Vector2 direction)
    {
        if (bullet)
        {
            MoveInDirection moveInDirection = bullet.GetComponent<MoveInDirection>();
            if (moveInDirection)
            {
                moveInDirection.Direction = direction;
            }

            //Rotate bullet to face direction of fire
            bullet.transform.rotation = Movement.GetRotationInDirection(direction);
        }
    }

    /// <summary>
    /// Manages resetting invincibility. Only resets if invincible is not checked when the game is started.
    /// </summary>
    private void CheckInvincibility()
    {
        if (invincible && !invincibleAtStart)
        {
            currentInvincibilityTime += Time.deltaTime;
            invincibleUI.SetActive(true);
            if (currentInvincibilityTime > invincibilityCoolDown)
            {
                ToggleInvincibility(false);
                invincibleUI.SetActive(false);
            }
        }
        else
        {
            invincibleUI.SetActive(false);
        }
    }

    /// <summary>
    /// Player reaction to a pickup.
    /// </summary>
    public void PickUp()
    {
        if (animator && hasPickUpAnimParameter)
        {
            animator.SetTrigger("Pickup");
        }
    }

    /// <summary>
    /// Change the shot type on the player.
    /// </summary>
    public void ChangeShotType(ShotType newShotType)
    {
        currentShotType = newShotType;
    }

    /// <summary>
    /// Toggle invincibility on the player.
    /// </summary>
    public void ToggleInvincibility(bool isInvincible)
    {
        invincible = isInvincible;
        if (invincibilityGO)
        {
            invincibilityGO.SetActive(isInvincible);
        }
        if (isInvincible)
        {
            currentInvincibilityTime = 0f;   
        }   
    }

    /// <summary>
    /// Handle invincibilty pickup on the player.
    /// </summary>
    public void InvincibilityPickup()
    {
        ToggleInvincibility(true);
    }

    /// <summary>
    /// Add an extra life to the player.
    /// </summary>
    public void AddLife()
    {
        int newLives = numLives + 1;
        numLives = Mathf.Clamp(newLives, 0, MAX_LIVES);
    }

    /// <summary>
    /// Handle game over for the player, so that the player does not destroy itself.
    /// </summary>
    protected override void OnGameOver()
    {
       //main player should not destroy itself in this case
    }

    /// <summary>
    /// Called on start of death.
    /// </summary>
    protected override void DeathStart()
    {
        base.DeathStart();
        ToggleExtraParts(false);
        ToggleFX(false);
        ToggleInvincibility(false);
        movement.enabled = false;
        if (!infiniteLives)
        {
            numLives--;
            //TODO: Handle game over.
        }
    }

    /// <summary>
    /// Called when death is complete.
    /// </summary>
    protected override void DeathComplete()
    {
        if (animator != null)
        {
            animator.SetTrigger("Reset");
        }
        else
        {
            Debug.Log("If your player isn't re-appearing, make sure you have an animator and the reference is set");
        }

        if (numLives <= 0)
        {
            outOfLives = true;
        }
        else
        {
            StartCoroutine(Respawn(false));
        }
    }

    /// <summary>
    /// Called when the game is restarted. Resets all properties on the player.
    /// </summary>
    public void RestartGame()
    {
        outOfLives = false;
        numLives = numStartingLives;
        StartCoroutine(Respawn(true));
    }

    IEnumerator Respawn(bool newGame = false)
    {
        ToggleRendering(false);

        if (newGame)
        {
            yield return null;
        }
        else
        {
            yield return new WaitForSeconds(respawnDelay);
        }

        //reset default properties for player
        movement.enabled = true;
        ToggleRendering(true);
        ToggleExtraParts(true);
        ToggleFX(true);
        transform.position = respawnPos;
        isDead = false;
        currentShotType = ShotType.REGULAR;
        collision.enabled = true;  //re-enable collider 
        hitPoints = maxHitPoints;
    }

    /// <summary>
    /// Toggle visibility for the extra parts on the player.
    /// </summary>
    private void ToggleExtraParts(bool active)
    {
        for (int i = 0; i < extraParts.Length; i++)
        {
            extraParts[i].SetActive(active);
        }
    }

    /// <summary>
    /// Toggle visibility for the FX on the player.
    /// </summary>
    private void ToggleFX(bool active)
    {
        for (int i = 0; i < playerFX.Length; i++)
        {
            if (active)
            {
                playerFX[i].Play(true);
            }
            else
            {
                playerFX[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }
}
