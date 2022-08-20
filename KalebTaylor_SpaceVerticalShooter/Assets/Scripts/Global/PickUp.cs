/// <summary>
/// Class to be attached to player pick ups in the game
/// </summary>

using UnityEngine;

public enum PickUpType
{
    ONE_UP,
    INVINCIBILITY,
    THREESHOT,
    LASER,
    CANNON,
    SCATTER,
    BOMB
};

public class PickUp : MonoBehaviour 
{
    [SerializeField]
    [Tooltip("Life time of pick up before self-destruction")]
    private float lifeTime = 4f;

    [SerializeField]
    private PickUpType pickUpType;

    [SerializeField]
    private AudioClip pickUpSound;
 
    void Start()
    {
        Destroy(gameObject, lifeTime); //Destroy pick up after lifeTime is reached.
    }

    protected void Update()
    {
        if (GameManager.GameOver())
        {
            DestroyImmediate(gameObject);
        }
    }

    /// <summary>
    /// Handle each type of pick up by calling correct method on the player.
    /// </summary>
    private void HandlePickUp()
    {
        switch (pickUpType)
        {
            case PickUpType.ONE_UP:
                GameManager.MainPlayer.AddLife();
                break;
            case PickUpType.INVINCIBILITY:
                GameManager.MainPlayer.ToggleInvincibility(true);
                break;
            case PickUpType.THREESHOT:
                GameManager.MainPlayer.ChangeShotType(ShotType.THREE);
                break;
            case PickUpType.LASER:
                GameManager.MainPlayer.ChangeShotType(ShotType.LASER);
                break;
            case PickUpType.CANNON:
                GameManager.MainPlayer.ChangeShotType(ShotType.CANNON);
                break;
            case PickUpType.SCATTER:
                GameManager.MainPlayer.ChangeShotType(ShotType.SCATTER);
                break;
            case PickUpType.BOMB:
                GameManager.MainPlayer.hasBomb = true;
                break;

        }

        AudioManager.PlayGameSound(pickUpSound);
        GameManager.MainPlayer.PickUp();
    }

    private void OnTriggerEnter2D(Collider2D c)
    {
        //Compare collider game object reference to the player game object.
        if (c.gameObject == GameManager.PlayerGameObject)
        {
            //Handle the pick up and destroy game object.
            HandlePickUp();
            Destroy(gameObject);
        }
    }
}
