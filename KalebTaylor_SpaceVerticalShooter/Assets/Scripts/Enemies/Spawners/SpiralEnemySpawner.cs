/// <summary>
/// This component is used for spawning spiral enemies as a trail.  The enemies do not follow each other but, instead 
/// the center of the spiral for each enemy is set to the first spiral enemy so that they all form a spiral around the same point.
/// This script assumes that enemies to be spawed are derived from Interactive and use the MoveInSpiral script.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiralEnemySpawner : EnemySpawner
{

    [Header("Spiral Settings")]
    [SerializeField]
    [Tooltip("Set direction of spiral.")]
    MoveInCircle.CircleMovementType movmentType = MoveInCircle.CircleMovementType.Clockwise;

    [SerializeField]
    [Tooltip("Set position object will fly out from.")]
    private MoveInSpiral.FlyFromType flyOutFrom = MoveInSpiral.FlyFromType.Bottom;

    [SerializeField]
    [Tooltip("Used to look up transform to set center point of circle. (Main Player by Default)")]
    private GameObject centerObject;
    private Vector3 centerPos;

    [SerializeField]
    [Tooltip("Set the radius (size) of circle formation.")]
    protected float startRadius = 3;

    [SerializeField]
    [Tooltip("Set the final radius. No spiral if radius and finalRadius are equal.")]
    private float endRadius = 3;

    [SerializeField]
    [Tooltip("Set the rate at which the radius changes in spiral motion. Should always be positive.")]
    [Range(0.0f, 10.0f)]
    private float radiusInc = 0.5f;

    private bool firstSpawn = true;

    // Use this for initialization.
    void Start()
    {
        enemyCount = 0;
        GameManager.RegisterSpawner(this);
    }

    /// <summary>
    /// Spawns enemy type for this spawner
    /// </summary>
    protected override void Spawn()
    {
        if (firstSpawn)
        {
            FirstSpawn();
        }

        Vector3 spawnPosition = GetSpawnPosition();
        // Spawn the spiral enemy.
        GameObject enemy = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);

        MoveInSpiral moveSpiral = enemy.GetComponent<MoveInSpiral>();
        if (moveSpiral)
        {
            moveSpiral.SetMovementType(movmentType);
            moveSpiral.SetFlyOutFrom(flyOutFrom);
            moveSpiral.SetStartRadius(startRadius);
            moveSpiral.SetEndRadius(endRadius);
            moveSpiral.SetRadiusInc(radiusInc);
            moveSpiral.SetCenterPos(centerPos);
        }

        SpiralEnemy spiralEnemy = enemy.GetComponent<SpiralEnemy>();
        if (spiralEnemy)
        {
            spiralEnemy.CenterObject = centerObject;
        }

        currentTime = 0f;
        enemyCount++;
    }

    /// <summary>
    /// Called for keeping track of center position when the first enemy spawns.
    /// </summary>
    private void FirstSpawn()
    {
        firstSpawn = false;

        if (centerObject == null)
        {
            centerObject = GameManager.PlayerGameObject;
        }

        // Init center position to be the center object position.
        if (centerObject)
        {
            centerPos = centerObject.transform.position;
        }
        else
        {
            Debug.LogError("Could not find Center Game Object: " + centerObject.name);
        }

    }
}
