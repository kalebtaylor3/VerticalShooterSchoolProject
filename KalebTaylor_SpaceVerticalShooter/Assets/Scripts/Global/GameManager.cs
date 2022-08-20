/// <summary>
/// Responsible for managing the flow of the entire game. Manages game states, menus, etc. 
/// Also has helpers for acessing various parts of the main player.
/// </summary>

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GameStages
{
    STAGE_INIT = -1,
    STAGE_1,
    STAGE_2,
    STAGE_3,
    STAGE_4,
    STAGE_BOSS,
    STAGE_END,
    MAX_STAGES
}

public class GameManager : MonoBehaviour
{
    //Used for bounds checks for the main player. 
    //You may need to change these if your player has a dramatically different size or odd proportions. 
    public static float MIN_X = -8;
    public static float MAX_X = 8;
    public static float MIN_Y = -4;
    public static float MAX_Y = 4;

    private int gameScore = 0;
    public int GameScore
    {
        get { return gameScore; }
    }

    private float gameTime = 0;
    public float GameTime
    {
        get { return gameTime; }
    }

    [SerializeField]
    [Tooltip("Reference to the main player game object.")]
    private GameObject player;
    private static Transform playerTransform;
    private static MainPlayer mainPlayer;

    /// <summary>
    /// Returns the main players transform.
    /// </summary>
    public static Transform PlayerTransform
    {
        get { return playerTransform; }
    }

    /// <summary>
    /// Returns the main players game object.
    /// </summary>
    public static GameObject PlayerGameObject
    {
        get { return playerTransform.gameObject; }
    }

    /// <summary>
    /// Returns the MainPlayer componentfrom the main player.
    /// </summary>
    public static MainPlayer MainPlayer
    {
        get { return mainPlayer; }
    }

    //instance for the GameManager
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }

            if (!instance)
            {
                Debug.LogError("No Game Manager Present !!!");
            }

            return instance;

        }
    }

    [SerializeField]
    private GameStages gameStage = GameStages.STAGE_INIT;
    public GameStages GameStage
    {
        get { return gameStage; }
    }
    private float currentStageTime = 0f;

    [Header("Stage Times ")]
    [SerializeField]
    private float[] stageTimes = new float[(int)GameStages.STAGE_BOSS];

    //stores reference for boss game object when using enemy spawner with one enemy for STAGE_BOSS
    private GameObject bossGameObject = null;
    private bool bossSpawned = false;

    [Header("Player Pickups")]
    [SerializeField]
    private bool testPickUps = true;
    [SerializeField]
    [Tooltip("Time in between pickup drops; Should be greater than Pick Up lifetimes")]
    private float testPickUpInterval = 5;
    [SerializeField]
    private GameObject[] pickUps;
    private float timeSinceLastPickUp = 0f;
    

    [Header("Debug Settings")]
    [SerializeField]
    private bool debugMessagesOn = true;
    [SerializeField]
    public Text debugText;

    private List<EnemySpawner> spawners;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        instance.spawners = new List<EnemySpawner>();

        if (instance.player)
        {
            //initialize main player member variables for quick access
            playerTransform = instance.player.transform;

            mainPlayer = instance.player.GetComponent<MainPlayer>();
            if (mainPlayer == null)
            {
                Debug.LogError("Main player is missing Main Player component.");
            }
        }
    }

    private void Start()
    {
        if (instance.player != null)
        {
            instance.player.SetActive(false);
        }

        if (gameStage > GameStages.STAGE_INIT)
        {
            GameStart();
        }
    }

    void Update()
    {
        UpdateStage();
        UpdatePlayerPickUps();
        UpdateDebug();
    }

    /// <summary>
    /// Updates the current stage based on the time or whether a boss needs to be destroyed
    /// </summary>
    private void UpdateStage()
    {
        if (gameStage == GameStages.STAGE_END)
        {
            return;
        }

        if (gameStage >= GameStages.STAGE_1)
        {
            gameTime += Time.deltaTime;
        }
            
        if (MainPlayer.OutOfLives)
        {
            GameEnd(false);
        }

        if (gameStage >= GameStages.STAGE_1 && gameStage <= GameStages.STAGE_4)
        {
            currentStageTime += Time.deltaTime;
            if(currentStageTime > stageTimes[(int)gameStage])
            {
                gameStage++;
                currentStageTime = 0f;
            }
        }
        else if (gameStage == GameStages.STAGE_BOSS)
        {
            if (bossSpawned && bossGameObject == null) //Boss dead
            {
                GameEnd(true); ;
            }
        }
    }

    /// <summary>
    /// Controls timed drops of player pickups in the game. Used primarily for testing purposes.
    /// </summary>
    private void UpdatePlayerPickUps()
    {
        if(testPickUps && IsGameInPlay())
        {
            timeSinceLastPickUp += Time.deltaTime;
            if (timeSinceLastPickUp > testPickUpInterval)
            {
                DropPickup(Vector3.zero);
                timeSinceLastPickUp = 0f;
            }
        }
    }

    /// <summary>
    /// Update any debug elements of the game.
    /// </summary>
    private void UpdateDebug()
    {
        if (debugText != null)
        {
            if (debugMessagesOn != debugText.gameObject.activeSelf)
            {
                debugText.gameObject.SetActive(debugMessagesOn);
            }

            if (debugMessagesOn)
            {
                debugText.text = gameStage.ToString();
            }
        }   
    }

    /// <summary>
    /// Register the boss with the game manager
    /// </summary>
    public static void RegisterBossSpawn(GameObject boss)
    {
        instance.bossSpawned = true;
        instance.bossGameObject = boss;
    }

    /// <summary>
    /// Register enemy spawners with the boss for reset purposes
    /// </summary>
    public static void RegisterSpawner(EnemySpawner spawner)
    {
        instance.spawners.Add(spawner);
    }

    /// <summary>
    /// Add score to the game
    /// </summary>
    public static void AddScore(int amount)
    {
        instance.gameScore += amount;
    }

    /// <summary>
    /// Called when the game is started from Main Menu exclusively
    /// </summary>
    public static void MainMenuGameStart()
    {
        instance.gameStage = GameStages.STAGE_1;
    }

    /// <summary>
    /// Called when the game is started (including starting on a different level in editor)
    /// </summary>
    public static void GameStart()
    {
        if (instance.player != null)
        {
            instance.player.SetActive(true);
        }
        instance.currentStageTime = 0f;
        AudioManager.GameStart();
    }

    /// <summary>
    /// Called when the game is ended
    /// </summary>
    public void GameEnd(bool victory)
    {
        gameStage = GameStages.STAGE_END;
        AudioManager.GameEnd(victory);
        UIManager.Instance.ShowGameOVer(victory);

    }

    /// <summary>
    /// Called when the game is restarted.
    /// </summary>
    public void GameRestart()
    {
        gameScore = 0;
        gameTime = 0;
        currentStageTime = 0;
        bossSpawned = false;
        bossGameObject = null;
        MainPlayer.RestartGame();
        AudioManager.GameStart();

        for (int i = 0; i < spawners.Count; i++)
        {
            spawners[i].GameRestart();
        }

        gameStage = GameStages.STAGE_1;
    }

    /// <summary>
    /// Called when the game is shut down from game over menu.
    /// </summary>
    public static void GameQuit()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    /// <summary>
    /// Returns true if the game is in a gameplay state (not in menus).
    /// </summary>
    public static bool IsGameInPlay()
    {
        bool bPlaying = false;
        if (instance.gameStage > GameStages.STAGE_INIT && instance.gameStage < GameStages.STAGE_END)
        {
            bPlaying = true;
        }
        return bPlaying;
    }

    /// <summary>
    /// Returns true if the game is in a End game state.
    /// </summary>
    public static bool GameOver()
    {
        return instance.gameStage == GameStages.STAGE_END;
    }

    /// <summary>
    /// Returns true if the main player exists and is currently dead
    /// </summary>
    public static bool MainPlayerIsDead()
    {
        bool retVal = false; //return false if there is no main player at all in the game 
        if (MainPlayer)
        {
            retVal = MainPlayer.IsDead;
        }

        return retVal;
    }

    /// <summary>
    /// Wrapper function for debug messages for collisions, etc. 
    /// Messages can be turned off by changing the debugMessagesOn property.
    /// </summary>
    public static void DebugMessage(string msg)
    {

        if (instance && instance.debugMessagesOn)
        {
            Debug.Log(msg);
        }
    }

    /// <summary>
    /// Drops a player pickup, called by enemies or obstacles based on their % chance of 
    /// dropping a pickup on death.
    /// </summary>
    public static void DropPickup(Vector3 dropPos)
    {
        if (instance.pickUps != null && instance.pickUps.Length > 0 )
        {
            int index = Random.Range(0, instance.pickUps.Length);
            Instantiate(instance.pickUps[index], dropPos, Quaternion.identity);
        }
    }
}
