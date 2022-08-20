/// <summary>
/// Handles the user interface for the game including MainMenu, HUD, and EndMenu
/// Includes menu visiblity, text updates, and event handling
/// </summary>

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    private static UIManager instance;
    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<UIManager>();
            }

            if (!instance)
            {
                Debug.LogError("No UI Manager Present !!!");
            }

            return instance;

        }
    }

    [SerializeField]
    private GameObject mainMenu;
    [SerializeField]
    private GameObject endMenu;
    [SerializeField]
    private GameObject gameHUD;

    [Header("HUD Settings")]
    [SerializeField]
    private GameObject[] livesImages;
    [SerializeField]
    private Text timeText;
    [SerializeField]
    private Text scoreText;
    [SerializeField]
    private Text healthText;

    [Header("Game Over Settings")]
    [SerializeField]
    private Text resultText;
    [SerializeField]
    private Text finalScoreText;
    [SerializeField]
    private Text playAgainText;
    [SerializeField]
    private string victoryMessage = "Congratulations. You won!";
    [SerializeField]
    private string failMessage = "Sorry. You lost!";
    [SerializeField]
    private string finalScoreMessage = "Your final score is: ";
    [SerializeField]
    private string playAgainMessage = "Would you like to play again?";


    [Header("Fade Settings")]
    [SerializeField]
    private float hudFadeTimeScale = 1.0f;
    [SerializeField]
    private float mainMenuFadeTimeScale = 1.5f;
    [SerializeField]
    private float endMenuFadeTimeScale = 1.5f;

    private Coroutine hudFadeCoroutine = null;
    private Coroutine mainMenuFadeCoroutine = null;
    private Coroutine endMenuFadeCoroutine = null;

    [Header("Bomb UI")]
    [SerializeField]
    private GameObject BombUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
       if (GameManager.Instance.GameStage > GameStages.STAGE_INIT)
       {
            ShowMenusOnStart();
       }
       else
       {
            ShowMainMenu(true, true);
            ShowEndMenu(false, true, false);
            ShowGameHUD(false, true);
        }
    }

    void Update()
    {
        UpdateHUD();
    }

    /// <summary>
    /// Update all elements of the HUD.
    /// </summary>
    private void UpdateHUD()
    {
        if (gameHUD)
        {
            if (timeText)
            {
                timeText.text = FormatTimeToString((int)GameManager.Instance.GameTime);
            }

            if (scoreText)
            {
                scoreText.text = GameManager.Instance.GameScore.ToString();
            }

            if (healthText)
            {
                healthText.text = GameManager.MainPlayer.HitPoints.ToString();
            }

            if(GameManager.MainPlayer.hasBomb == true)
            {
                BombUI.SetActive(true);
            }
            else
            {
                BombUI.SetActive(false);
            }

            if (livesImages.Length > 0)
            {
                for (int i = 0; i < livesImages.Length; i++)
                {
                    if (GameManager.MainPlayer.NumLives > i)
                    {
                        livesImages[i].SetActive(true);
                    }
                    else
                    {
                        livesImages[i].SetActive(false);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Handle event the start of the game.
    /// </summary>
    public void HandleStartGame()
    {
        GameManager.MainMenuGameStart();
        GameManager.GameStart();
        ShowMenusOnStart();
    }


    /// <summary>
    /// Show the appropriate menu for the start of the game (not main menu).
    /// </summary>
    private void ShowMenusOnStart()
    {
        ShowMainMenu(false, false);
        ShowEndMenu(false, true, false);
        ShowGameHUD(true, false);
    }


    /// <summary>
    /// Handle user interface for the end of the game.
    /// </summary>
    public void ShowGameOVer(bool victory)
    {
        ShowEndMenu(true, false, victory);
    }

    /// <summary>
    /// Handle menu visibility when the game is restarted.
    /// </summary>
    public void RestartGame()
    {
        ShowMainMenu(false, false);
        ShowEndMenu(false, false, false);
        ShowGameHUD(true, false);
        GameManager.Instance.GameRestart();
    }

    /// <summary>
    /// Handle event for exiting game.
    /// </summary>
    public void QuitGame()
    {
        GameManager.GameQuit();
    }

    /// <summary>
    /// Helper function for toggling Main Menu visibility.
    /// </summary>
    private void ShowMainMenu(bool show, bool immediate)
    {
        if (mainMenu)
        {
            CanvasGroup canvasGroupStartMenu = mainMenu.GetComponent<CanvasGroup>();
            if (canvasGroupStartMenu && !immediate)
            {
                if (mainMenuFadeCoroutine != null)
                {
                    StopCoroutine(mainMenuFadeCoroutine);
                    mainMenuFadeCoroutine = null;
                }

                StartCoroutine(FadeCanvasGroup(mainMenu, canvasGroupStartMenu, show, mainMenuFadeTimeScale, immediate));
            }
            else
            {
                mainMenu.SetActive(show);
            }
        }

        
    }

    /// <summary>
    /// Helper function for togglind End Game Menu visibility.
    /// </summary>
    private void ShowEndMenu(bool show, bool immediate, bool victory)
    {
        if (endMenu)
        {
            if (show)
            {
                if (resultText)
                {
                    if (victory)
                    {
                        resultText.text = victoryMessage;
                    }
                    else
                    {
                        resultText.text = failMessage;
                    }

                }

                if (finalScoreText)
                {
                    finalScoreText.text = finalScoreMessage + GameManager.Instance.GameScore.ToString();
                }

                if (playAgainText)
                {
                    playAgainText.text = playAgainMessage;
                }
            }


            endMenu.SetActive(show);

            CanvasGroup canvasGroupEndMenu = endMenu.GetComponent<CanvasGroup>();
            if (canvasGroupEndMenu && !immediate)
            {
                if (endMenuFadeCoroutine != null)
                {
                    StopCoroutine(endMenuFadeCoroutine);
                    endMenuFadeCoroutine = null;
                }

                endMenuFadeCoroutine = StartCoroutine(FadeCanvasGroup(endMenu, canvasGroupEndMenu, show, endMenuFadeTimeScale, immediate));
            }
            else
            {
                endMenu.SetActive(show);
            }
        }
    }

    /// <summary>
    /// Helper function for toggling HUD visibility.
    /// </summary>
    private void ShowGameHUD(bool show, bool immediate)
    {
        if (gameHUD)
        {
            CanvasGroup canvasGroupHUD = gameHUD.GetComponent<CanvasGroup>();
            if (canvasGroupHUD && !immediate)
            {
                if (hudFadeCoroutine != null)
                {
                    StopCoroutine(hudFadeCoroutine);
                    hudFadeCoroutine = null;
                }
                hudFadeCoroutine = StartCoroutine(FadeCanvasGroup(gameHUD, canvasGroupHUD, show, hudFadeTimeScale, immediate));
            }
            else
            {
                gameHUD.SetActive(show);
            }
            
        }
    }

    /// <summary>
    /// Coroutine for fading the alpha of a canvas group up or down
    /// </summary>
    private IEnumerator FadeCanvasGroup(GameObject canvasGO, CanvasGroup canvasGroup, bool show, float timeScale, bool immediate)
    {
        if (show)
        {
            canvasGroup.alpha = 0;
            canvasGO.SetActive(true);
         
            while (!immediate && canvasGroup && canvasGroup.alpha < 1)
            {
                canvasGroup.alpha += Time.deltaTime * timeScale;
                yield return null;
            }
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGO.SetActive(true);
            while (!immediate && canvasGroup && canvasGroup.alpha > 0)
            {
                canvasGroup.alpha -= Time.deltaTime * timeScale;
                yield return null;
            }
            canvasGroup.alpha = 0;
            canvasGO.SetActive(false);
            
        }

    }
    /// <summary>
    /// Helper function displaying the game time.
    /// </summary>
    private string FormatTimeToString(int time)
    {
        string timeString = "0";

        if (time < 10)
        {
            timeString = "00" + time;
        }
        else if (time < 100)
        {
            timeString = "0" + time;
        }
        else if (time < 1000)
        {
            timeString = "" + time;
        }

        return timeString;
    }
}
