/// <summary>
/// Abstract class for basic movement that all movement classes must override.
/// </summary>
using UnityEngine;

public abstract class Movement : MonoBehaviour
{
    [SerializeField]
    protected float speed = 10;
    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    [SerializeField]
    protected float rotationSpeed = 0;

    protected virtual void Awake()
    {
       
    }

    protected virtual void Update()
    {
        if (GameManager.IsGameInPlay())
        {
            UpdateRotation();
            UpdatePosition();
        }
    }

    //Initializes data for derived classes if needed.
    //Override in derived classes if needed.
    public virtual void Initialize() { }

    //Updates the position of the transform the script is attached to. 
    //Override and attach to Game Objectto change movement behaviour.
    public abstract void UpdatePosition();

    //Updates the rotation of the transform the script is attached to.
    //Override and attach to Game Objectto change movement behaviour.
    public abstract void UpdateRotation();


    public static Quaternion GetRotationTowardsTarget(Transform rotator, Transform target)
    {
        Vector3 diff = target.position - rotator.position;
        return GetRotationInDirection(diff);
    }

    public static Quaternion GetRotationInDirection(Vector3 dir)
    {
        dir.z = 0;
        dir.Normalize();

        float angle = 0.0f;
        //Vector3.Angle returns the smallest angle between to vectors, 
        //we are rotating to the left so we want to wind to the left (use larger angle) if necessary
        angle = Vector3.Angle(Vector3.up, dir);
        if (Vector3.up.x < dir.x)
        {
            angle = 360 - angle;
        }

        return Quaternion.Euler(new Vector3(0, 0, angle));
    }
}
