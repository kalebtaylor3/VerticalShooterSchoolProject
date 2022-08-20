/// <summary>
/// Abstract interactive body for space shooter. Allows for basic functionality for 
/// health, death, animations and sounds. 
/// </summary>

using System.Collections;
using UnityEngine;

public abstract class InteractiveBody : MonoBehaviour
{
    private static int MAX_HEALTH = 999;

    [Header("Health Settings")]
    [SerializeField]
    protected int hitPoints = 1;
    public int HitPoints
    {
        get { return hitPoints; }
    }
    [SerializeField]
    [Tooltip("List of damage types that can damage this interactive body.")]
    protected DamageType[] acceptDamages;
    [SerializeField]
    [Tooltip("Is this body invincible. Should be used primarily for testing.")]
    protected bool invincible = false;
    [SerializeField]
    [Tooltip("Lifetme for this interactive body, set to -1 if it should live until destroyed by player or something else.")]
    protected float lifeTime = -1f;
    [SerializeField]
    [Tooltip("% chance that this body drops a pickup on death.")]
    [Range(0, 1)]
    protected float chanceToDropPickUpOnDeath = 0f;
    [SerializeField]
    [Tooltip("Score contributed to game when destroyed by player")]
    private int score = 0;

    [Header("Animation Settings")]
    [SerializeField]
    [Tooltip("Reference to animator. Only Required if body uses animations.")]
    protected Animator animator; //only needs to be set for Bodies with Death animations
    [SerializeField]
    [Tooltip("Length of death animations. Used for playing sounds and resetting.")]
    protected float deathAnimationLength = 0.0f;

    [Header("Shoot Settings")]
    [SerializeField]
    protected GameObject regularBullet = null;
    [SerializeField]
    protected GameObject shotEmitter = null;

    [Header("Sound Settings")]
    [SerializeField]
    protected AudioSource shootAudioSource = null;
    [SerializeField]
    protected AudioClip shootSound = null;
    [SerializeField]
    [Range(0.5f, 1f)]
    [Tooltip("Range of pitch variance low ")]
    protected float shootSoundPitchVarianceLow = 1f;
    [SerializeField]
    [Range(1, 1.5f)]
    [Tooltip("Range of pitch variance high ")]
    protected float shootSoundPitchVarianceHigh = 1f;

    [SerializeField]
    private AudioSource deathAudioSource = null;
    [SerializeField]
    private AudioClip deathSound = null;

    protected Collider2D collision;
    protected Renderer[] renderers = null;
    protected bool isDead = false;
    public bool IsDead
    {
        get { return isDead; }
    }
    
    // Use this for initialization
    protected virtual void Awake()
    {
        collision = GetComponent<Collider2D>();

        renderers = GetComponentsInChildren<Renderer>();
        
        if (lifeTime > 0)
        {
            Destroy(gameObject, lifeTime);
        }
    }

    // Use this for initialization that depends on other GameObjects
    protected virtual void Start()
    {
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (GameManager.GameOver())
        {
            OnGameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!invincible)
        {
            Damage damage = collision.gameObject.GetComponent<Damage>();

            if (damage && CheckAcceptsDamage(damage.Type))
            {
                GameManager.DebugMessage(gameObject.name + " received " + damage.DamageAmount + " of type " + damage.Type.ToString() + " from " + collision.gameObject.name);

                ApplyDamage(damage.DamageAmount, damage.Type);

                if (damage.DestroyAfterDamage)
                {
                    Destroy(collision.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Called On Game Over (so everything can destroy istelf but the main player)
    /// </summary>
    protected virtual void OnGameOver()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Basic shoot method for all interactive bodies, should be called from body specific code
    /// which can override the bullet and spawn location as necessary.
    /// </summary>
    protected GameObject Shoot(GameObject bullet = null, Transform spawnTransform = null)
    {
        //determine what bullet to spawn
        GameObject bulletPrefab = bullet;
        if (bullet == null)
        {
            bulletPrefab = regularBullet;
        }

        if (bulletPrefab == null)
        {
            Debug.LogError("Shoot function called without bullet parameter or regular bullet object set on body");
        }

        //determine and set to spawn spawnloacation
        Vector3 spawnPos = transform.position; // by default use the position of the interactive body
        Quaternion spawnRot = transform.rotation;
        if (spawnTransform != null)
        {
            spawnPos = spawnTransform.position;
            spawnRot = spawnTransform.rotation;
        }
        else if (shotEmitter != null)
        {
            spawnPos = shotEmitter.transform.position;
            spawnRot = shotEmitter.transform.rotation;
        }

        GameObject bulletGO = GameObject.Instantiate(bulletPrefab, spawnPos, spawnRot);
        if (bulletGO == null)
        {
            Debug.LogError("Bullet failed to spawn; most likely missing prefab");
            return null; //can no longer proceeed with out a bullet
        }

        //determine and set direction 
        MoveInDirection movement = bulletGO.GetComponent<MoveInDirection>();
        if (movement)
        {
            if (movement.Direction == Vector2.zero)
            {
                //Vector3 newDir = (shotEmitter.transform.position - transform.position);
                Vector3 newDir = (spawnRot * Vector3.up);  //GINAGINA
                newDir.z = 0;
                newDir.Normalize();

                movement.Direction = newDir;
            }
        }

        return bulletGO;

    }

    /// <summary>
    /// Checks to see if this body accepts a damage type provided as a parameter.
    /// </summary>
    private bool CheckAcceptsDamage(DamageType damage)
    {
        for(int i = 0; i < acceptDamages.Length; i++)
        {
            if(acceptDamages[i] == damage)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Apply damage to body and start death process if necessary.
    /// </summary>
    private void ApplyDamage(int damageAmount, DamageType damageType)
    {
        hitPoints = Mathf.Clamp(hitPoints - damageAmount, 0, MAX_HEALTH);

        if (damageAmount == -1)
        {
            hitPoints = 0;
        }

        if (hitPoints <= 0 || damageAmount == -1) //force death
        {
            if (!isDead)
            {
                if (damageType == DamageType.PlayerBullet)
                {
                    GameManager.AddScore(score);
                }
                StartCoroutine("Death");
            }
        }
    }

    /// <summary>
    /// Coroutine for InteractiveBody Death Lifecycle
    /// </summary>
    private IEnumerator Death()
    {
        isDead = true;
        yield return null; //Wait one frame to make sure colliding bodies kill each other before disabling their collider.

        DeathStart();

        yield return new WaitForSeconds(deathAnimationLength);

        //wait for any extra required time for the death sound to finish playing
        if (deathSound != null)
        {
            float remainderWaitTime = deathSound.length - deathAnimationLength;
            
            if(remainderWaitTime > 0)
            {
                ToggleRendering(false);
                DeathAnimationComplete();
                yield return new WaitForSeconds(remainderWaitTime);
            }
        }
        //NOTE: We could add a state behaviour to death state in the controller
        //      to indicate when the anim is complete but it is outside the scope of the course.
        

        DeathComplete();
    }

    /// <summary>
    /// Called on start of death.
    /// </summary>
    protected virtual void DeathStart()
    {
        //decide if enemy should drop a pick up on death
        if (chanceToDropPickUpOnDeath > 0)
        {
            if (Random.Range(0f,1f) <= chanceToDropPickUpOnDeath )
            {
                GameManager.DropPickup(transform.position);
            }
        }

        collision.enabled = false;  //disable collider so it can't kill other objects

        if (animator != null && deathAnimationLength > 0)
        {
            animator.SetTrigger("Death"); //trigger death animation
        }

        AudioManager.PlayRandomSoundOnSource(deathAudioSource, deathSound);
    }

    /// <summary>
    /// Called on completion of death animation
    /// Uses to make any changes in between animation completion and death completion
    /// typically used for disabling movement 
    /// </summary>
    protected virtual void DeathAnimationComplete()
    {
    }

    /// <summary>
    /// Called on completion of death, most Bodies will want to destroy 
    /// themselves at this point, its virtual so the player can override.
    /// </summary>
    protected virtual void DeathComplete()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Toggles whether or not the player is visible. The player is never destroyed,
    /// it just enters a death state where it is not visible.
    /// </summary>
    protected void ToggleRendering(bool active)
    {
        if (renderers!= null && renderers.Length > 0)
        {
            for(int i =0; i < renderers.Length; i++)
            {
                renderers[i].enabled = active;
            }
 
        }
    }

}
