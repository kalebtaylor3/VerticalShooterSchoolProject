/// <summary>
/// Manages audio for the game, such as music, pickup sand menu sounds
/// </summary>
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    //instance for the GameManager
    private static AudioManager instance;
    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AudioManager>();
            }

            if (!instance)
            {
                Debug.LogError("No Audio Manager Present !!!");
            }

            return instance;

        }
    }

    [SerializeField]
    private AudioSource gameMusicSource;
    [SerializeField]
    private AudioSource gameSoundsSource;

    [SerializeField]
    private AudioClip startGameClip;
    [SerializeField]
    private AudioClip gameOverDeathClip;
    [SerializeField]
    private AudioClip gameOverVictoryClip;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    /// <summary>
    /// Called when the game is started
    /// </summary>
    public static void GameStart()
    {
        if (instance && instance.gameMusicSource)
        {
            instance.gameMusicSource.Play();
        }

        if (instance)
        {
            PlayGameSound(instance.startGameClip);
        }
    }

    /// <summary>
    /// Called when the game is ended
    /// </summary>
    public static void GameEnd(bool victory)
    {
        if (instance.gameMusicSource)
        {
            instance.gameMusicSource.Stop();
        }

        if (instance.gameOverDeathClip && !victory)
        {
            PlayGameSound(instance.gameOverDeathClip);
        }
        else if (instance.gameOverVictoryClip && victory)
        {
            PlayGameSound(instance.gameOverVictoryClip);
        }
    }

    /// <summary>
    /// Playes a game sound on the game sound audio source; Game sounds are sounds that are not specific
    /// to a player or enemy but are intended for player feedback
    /// </summary>
    public static void PlayGameSound(AudioClip clip)
    {
        if (clip != null && instance)
        {
            instance.gameSoundsSource.clip = clip;
            instance.gameSoundsSource.Play();
        }
    }

    /// <summary>
    /// Playes a provided audio clip on a provided audio source; 
    /// optionaly add, low and high end pitch values.
    /// </summary>
    public static void PlayRandomSoundOnSource(AudioSource source, AudioClip audioClip, float pitchRangeLow = 1f, float pitchRangeHigh = 1f)
    {
        if (source && audioClip)
        {
            source.clip = audioClip;
            source.pitch = Random.Range(pitchRangeLow, pitchRangeHigh);
            source.Play();
        }
    }

}
