/// <summary>
/// Movement that moves between a set of targets. 
/// Set of targets must be set as property.
/// </summary>
using UnityEngine;

public class MoveBetweenTargets : Movement
{
    [SerializeField]
    protected bool rotateTowardsTarget = false;
    [SerializeField]
    protected float rotSpeed = 10;

    [SerializeField]
    protected Transform target = null;
    public Transform Target
    {
        get { return target; }
        set { target = value; }
    }

    //updates the position of the transform the script is attached to 
    //override and attach to Game Objectto change movement behaviour
    public override void UpdatePosition()
    {
        if (target != null)
        {
        }
    }

    //updates the rotation of the transform the script is attached to 
    //override and attach to Game Objectto change movement behaviour
    public override void UpdateRotation()
    {
        if (target != null && rotateTowardsTarget)
        {
        }
    }
}
