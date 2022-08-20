/// <summary>
/// This component is used for spawning obstacles and enemies for different 
/// stages for the shooter project.
/// </summary>

using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Obstacle or Enemy to be spawned.")]
    protected GameObject obstaclePrefab;

    [SerializeField]
    [Tooltip("Amount of time to delay before starting spawning.")]
    protected float spawnDelay = 0f;
    private float currDelayTime = 0;
    private bool delayComplete = false;

    [SerializeField]
    [Tooltip("Spawn rate for the obstacle.")]
    protected float spawnFrequency = 5f;
    protected float currentTime = 0f;

    [SerializeField]
    [Tooltip("Number of Enemies to be spawned (-1 is unlimited).")]
    protected int numEnemies = 1;

    [SerializeField]
    [Tooltip("Potential spawn locations.")]
    protected Transform[] spawnPoints;
    [SerializeField]
    [Tooltip("Randomize which position to spawn from; Will cycle in order otherwise.")]
    protected bool randomizeSpawnPoint = true;
    protected int currentSpawnPoint = 0;

    [SerializeField]
    [Tooltip("Which states of the game is this spawner active during.")]
    protected GameStages[] spawnableStates;

    protected int enemyCount;

    void Start()
    {
        enemyCount = 0;
        GameManager.RegisterSpawner(this);
    }

    void Update()
    {
        if (CanSpawnDuringStage() && !delayComplete)
        {
            currDelayTime += Time.deltaTime;
            if (currDelayTime > spawnDelay)
            {
                delayComplete = true;
            }
        }

        //Only spawn if you are active for this stage and the main player is alive.
        if (!delayComplete || !CanSpawnDuringStage() || GameManager.MainPlayerIsDead())
        {
            return;
        }

        currentTime += Time.deltaTime;
        if (currentTime >= spawnFrequency && (numEnemies == -1 || enemyCount < numEnemies))
        {
            Spawn();
        }
    }

    /// <summary>
    /// Called to reset properties the spawner on game restart.
    /// </summary>
    public void GameRestart()
    {
        enemyCount = 0;
        delayComplete = false;
        currDelayTime = 0f;
    }

    /// <summary>
    /// Spawns enemy type for this spawner
    /// </summary>
    protected virtual void Spawn()
    {
        Vector3 spawnPosition = GetSpawnPosition();

        GameObject spawnedEnemy = Instantiate(obstaclePrefab, spawnPosition, Quaternion.identity);
        currentTime = 0f;
        enemyCount++;

        if (spawnableStates != null && spawnableStates.Length == 1 && spawnableStates[0] == GameStages.STAGE_BOSS && numEnemies == 1)
        {
            GameManager.RegisterBossSpawn(spawnedEnemy);
        }
    }

    /// <summary>
    /// Return the next spawn point for an obstacle based on random values. 
    /// Spawns at center of screen if no spawn positions are provided.
    /// </summary>
    protected Vector3 GetSpawnPosition()
    {
        if (randomizeSpawnPoint)
        {
            int randomInt = Random.Range(0, spawnPoints.Length);
            return spawnPoints[randomInt].position;
        }
        else if (spawnPoints.Length > 0)
        {
            currentSpawnPoint = (currentSpawnPoint + 1) % spawnPoints.Length;
            return spawnPoints[currentSpawnPoint].position;
        }
        else
        {
            return Vector3.zero;
        }
    }

    /// <summary>
    /// Returns true if the Obstacle can spawn during the current stage. 
    /// </summary>
    protected bool CanSpawnDuringStage()
    {
        for (int i = 0; i < spawnableStates.Length; i++)
        {
            if (spawnableStates[i] == GameManager.Instance.GameStage)
            {
                return true;
            }
        }

        return false;
    }
}
