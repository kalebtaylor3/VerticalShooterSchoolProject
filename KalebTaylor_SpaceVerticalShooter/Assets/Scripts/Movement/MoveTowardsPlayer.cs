/// <summary>
/// Movement that moves towards player. Can rotate towards player and also
/// optionally lock on when alight with player
/// </summary>
using UnityEngine;

public class MoveTowardsPlayer : Movement
{
    [SerializeField]
    [Tooltip("Should the mover rotate to face the player.")]
    private bool rotateTowardsPlayer = true;
    [SerializeField]
    [Tooltip("Should the mover lock and maintain a rotation once it is facing the player.")]
    private bool lockRotationWhenAligned = false;
    [SerializeField]
    [Tooltip("Force the rotation towards the player on spawn.")]
    private bool forceRotationOnSpawn = false;

   

    private bool rotationLocked = false;
    public bool RotationLocked
    {
        get { return rotationLocked; }
    }

    private Vector3 directionToPlayerNormailized;

    protected virtual void Start()
    {
        if (forceRotationOnSpawn)
        {
            transform.rotation = Movement.GetRotationTowardsTarget(transform, GameManager.PlayerTransform);
        }
    }

    protected override void Update()
    {
        directionToPlayerNormailized = GameManager.PlayerTransform.position - transform.position;
        directionToPlayerNormailized.z = 0;   //shouldn't be necessary but ...  just in case
        directionToPlayerNormailized.Normalize();

        // make sure you call base.Update in your derived Movement classes
        base.Update();
    }

    public override void UpdatePosition()
    {
        if (!lockRotationWhenAligned || (lockRotationWhenAligned && !rotationLocked))
        {
            transform.Translate(directionToPlayerNormailized * speed * Time.deltaTime, Space.World);
        }
        else if ((lockRotationWhenAligned && rotationLocked))
        {
            transform.Translate(transform.up * speed * Time.deltaTime, Space.World);
        }

    }

    public override void UpdateRotation()
    {
        if (rotateTowardsPlayer)
        {
            if (!lockRotationWhenAligned || (lockRotationWhenAligned && !rotationLocked))
            {
                Quaternion newRot = Movement.GetRotationTowardsTarget(transform, GameManager.PlayerTransform);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, newRot, rotationSpeed * Time.deltaTime);


                if (Quaternion.Dot(newRot, transform.rotation) > .999f)
                {
                    rotationLocked = true;
                }

            }
        }
    }
}
