/// <summary>
/// Boss Enemy using Interactive Body as its parent class.
/// Provides functionality for Boss Enemy. Uses MoveTowardsPlayerOffset for movement.
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : InteractiveBody
{
    private enum bossStateType { FLY_IN = -1, TURRET_ATTACKS, SCATTER_ATTACK, WAIT_FOR_PLAYER };

    [Header("Boss Settings")]
    [SerializeField]
    [Tooltip("Defines the amount of time for boss to move to first position to attack.")]
    private float timeToFyIn = 4.0f;

    [SerializeField]
    [Tooltip("Contains list of shot emitters to fire from at once for Scatter Effect.")]
    private List<GameObject> shotEmitterList = null;

    [SerializeField]
    private float rateOfFire = 1.5f;

    [SerializeField]
    [Tooltip("Displays current state of boss.")]
    private bossStateType bossState = bossStateType.SCATTER_ATTACK;

    [Header("Turret Settings")]

    [SerializeField]
    [Tooltip("Turret prefab to attach to Boss.")]
    private GameObject turretPrefab;

    [SerializeField]
    [Tooltip("Use these positions to attach turrets to Boss. Number of positions deterimes number of turrets to create.")]
    private List<Transform> turretAttachTransform;

    [SerializeField]
    [Tooltip("Use these positions to attach turrets to Boss. Number of positions deterimes number of turrets to create.")]
    private float[] turretScalesFactor;

    [SerializeField]
    [Tooltip("Sets the time for each turret (and scatter) attacks.")]
    private float timeOfEachAttack = 3.0f;

    [Tooltip("Game Object references for all extra player parts.")]
    [SerializeField]
    private GameObject[] extraParts;

    private List<GameObject> turretList;
    private MoveTowardsPlayerOffset moveTowardsPlayerOffSet;
    private Collider2D bossCollider;
    private int currentTurretAttackingIndex;
    private float attackTimer;
    private float fireTimer;
    private float flyInTimer;


    protected override void Awake()
    {
        base.Awake();
        moveTowardsPlayerOffSet = GetComponent<MoveTowardsPlayerOffset>();
        fireTimer = 0f;
        flyInTimer = 0f;
        bossCollider = GetComponent<Collider2D>();
        currentTurretAttackingIndex = 0;
        attackTimer = 0f;
        bossState = bossStateType.FLY_IN;
        turretList = new List<GameObject>();
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        // Disable the collider on the boss's body.
        if (bossCollider)
        {
            bossCollider.enabled = false;
        }

        // Create the turrets.
        CreateAttachedTurrets();

        // Disable the turrets.
        EnableTankAtIndexOnly(-1,false);
    }

    
    protected override void Update()
    {
        base.Update();

        if (isDead)
        {
            return;
        }

        if (GameManager.MainPlayer.IsDead) 
        {
            // Disable the turrets.
            EnableTankAtIndexOnly(-1, false);

            // Disable movement.
            if (moveTowardsPlayerOffSet)
                moveTowardsPlayerOffSet.enabled = false;
            return;
        }

        // Enable movement.
        if (moveTowardsPlayerOffSet)
            moveTowardsPlayerOffSet.enabled = true;

        if (CheckAllTurretsDestroyed())
        {
            // Enable collider on boss's body.
            if (bossCollider)
                bossCollider.enabled = true;

            // Set the state to the scatter attack when all turrets are destroyed.
            bossState = bossStateType.SCATTER_ATTACK;
        }

        // Update boss depending on state.
        switch (bossState)
        {
            case bossStateType.FLY_IN:
                {
                    FlyInUpdate();
                    break;
                }
            case bossStateType.TURRET_ATTACKS:
                {
                    TurretsAttackUpdate();
                    break;
                }
            case bossStateType.SCATTER_ATTACK:
                {
                    ScatterAttackUpdate();
                    break;
                }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////
        //DEBUG
        #region Debug
        if (shotEmitter != null)
        {
            Debug.DrawRay(shotEmitter.transform.position, shotEmitter.transform.rotation * new Vector3(0,1,0), Color.red);
        }
        for(int i=0; i < shotEmitterList.Count; i++)
        {
            if (shotEmitterList[i])
            {
                Debug.DrawRay(shotEmitterList[i].transform.position, shotEmitterList[i].transform.rotation * new Vector3(0, 1, 0), Color.red);
            }
        }
        #endregion 
        ////////////////////////////////////////////////////////////////////////////////////////////
       
    }
    /// <summary>
    /// Returns true if all turrets have been destroyed.
    /// </summary>
    /// <returns></returns>
    bool CheckAllTurretsDestroyed()
    {
        bool allDestroyed = false;
        int numTurretsDestroyed = 0;   
        for (int i = 0; i < turretList.Count; i++)
        {
            if (turretList[i] == null)
            {
                // Current number of turrets attached to the boss that have been destroyed.
                numTurretsDestroyed++;
            }
        }
        if (numTurretsDestroyed == turretList.Count)
        {
            // All turrets have been destroyed.
            allDestroyed = true;
        }
        return allDestroyed;
    }
    /// <summary>
    /// Shoots scatter shot using regularBullet prefab and shot emitter points.
    /// </summary>
    void ShootScatter()
    {
        // Shoot from default shot emitter.
        Shoot(regularBullet, shotEmitter.transform);

        AudioManager.PlayRandomSoundOnSource(shootAudioSource, shootSound, shootSoundPitchVarianceLow, shootSoundPitchVarianceHigh);

        // Shoot from additional shot emitters.
        for (int i = 0; i < shotEmitterList.Count; i++)
        {
            if (shotEmitterList[i])
            {
                Shoot(regularBullet, shotEmitterList[i].transform);
            }
        }
    }
    /// <summary>
    /// Enables/Disables the tank at index.  If index is -1 all tanks are enabled/disabled.
    /// </summary>
    /// <param name="index"></param>
    /// <param name="enable"></param>
    void EnableTankAtIndexOnly(int index, bool enable)
    {
        // Set all tanks to "enable" bool if index == -1.
        if (index == -1)
        {
            for (int i = 0; i < turretList.Count; i++)
            {
                if (turretList[i] != null)
                {
                    TurretEnemy turretEnemyScript = turretList[i].GetComponent<TurretEnemy>();
                    if (turretEnemyScript)
                    {
                        turretEnemyScript.EnableTurretGun(enable);
                    }
                }
            }
        }
        else if (index >= 0 && index < turretList.Count)
        {
            // Set all tank at index to "enable" bool.
            if (turretList[index] != null)
            {
                TurretEnemy turretEnemyScript = turretList[index].GetComponent<TurretEnemy>();
                if (turretEnemyScript)
                {
                    turretEnemyScript.EnableTurretGun(enable);
                }
            }
        }
        
    }
    /// <summary>
    /// Creates turrets using the turretPrefab. The number of turrets created are determined by the 
    /// number of turretAttachTransforms. Note: position and rotation of transforms (in turretAttachTransform)  are used for creation.
    /// </summary>
    void CreateAttachedTurrets()
    {
        if (turretPrefab != null)
        {
            for (int i = 0; i < turretAttachTransform.Count; i++)
            {
                GameObject spawnedTurret = Instantiate(turretPrefab, turretAttachTransform[i].position, turretAttachTransform[i].rotation);
                Vector3 newScale = spawnedTurret.transform.localScale;
                if(turretScalesFactor != null && turretScalesFactor.Length == turretAttachTransform.Count )
                {
                    newScale.x *= turretScalesFactor[i];
                    newScale.y *= turretScalesFactor[i];
                    spawnedTurret.transform.localScale = newScale;
                }
                

                // Attach the spawned turret to the boss.
                spawnedTurret.transform.parent = this.transform;
                turretList.Add(spawnedTurret);
            }
        }
    }
    /// <summary>
    /// Update for TURRET_ATTACKS state.
    /// </summary>
    void TurretsAttackUpdate()
    {
        // Check if current turret is still valid.
        if (turretList[currentTurretAttackingIndex] == null)
        {
            // Current turret attacking has been destroyed, force next attack by setting time.
            attackTimer = timeOfEachAttack;
        }

        attackTimer += Time.deltaTime;

        // Check if it is time for next attack.
        if (attackTimer >= timeOfEachAttack)
        {
            attackTimer = 0.0f;
            // Disable the current turret that is attacking.
            EnableTankAtIndexOnly(currentTurretAttackingIndex, false);
            // Set the next turret index to attack.
            currentTurretAttackingIndex++;
            if (currentTurretAttackingIndex >= turretList.Count)
            {
                currentTurretAttackingIndex = 0;
                // All turrets have attack so set state to scatter attack
                bossState = bossStateType.SCATTER_ATTACK;
                // Disable the turrets.
                EnableTankAtIndexOnly(-1, false);
            }
            else
            {
                EnableTankAtIndexOnly(currentTurretAttackingIndex, true);
            }
        }
    }

    /// <summary>
    /// Update for SCATTER_ATTACK state.
    /// </summary>
    void ScatterAttackUpdate()
    {
        // Check if time to shoot.
        fireTimer += Time.deltaTime;
        if (fireTimer > rateOfFire)
        {
            ShootScatter();
            fireTimer = 0f;
        }
        if (!CheckAllTurretsDestroyed())
        {
            // Check if time to switch attacks.
            attackTimer += Time.deltaTime;
            if (attackTimer >= timeOfEachAttack)
            {
                attackTimer = 0.0f;
                currentTurretAttackingIndex = 0;
                // All turrets have attack so set state to scatter attack.
                bossState = bossStateType.TURRET_ATTACKS;
                EnableTankAtIndexOnly(currentTurretAttackingIndex, true);
            }
        }
    }
    /// <summary>
    /// Update for FLY_IN state.
    /// </summary>
    void FlyInUpdate()
    {
        // Wait to fly in.
        flyInTimer += Time.deltaTime;
        if (flyInTimer >= timeToFyIn)
        {
            // After time up, start turret attacks.
            flyInTimer = 0.0f;
            bossState = bossStateType.TURRET_ATTACKS;
            attackTimer = 0.0f;
            currentTurretAttackingIndex = 0;
            EnableTankAtIndexOnly(currentTurretAttackingIndex, true);
        }
    }

    /// <summary>
    /// Called on start of death.
    /// </summary>
    protected override void DeathStart()
    {
        base.DeathStart();

        if (extraParts != null)
        {
            for (int i = 0; i < extraParts.Length; i++)
            {
                extraParts[i].SetActive(false);
            }
        }
    }
}


