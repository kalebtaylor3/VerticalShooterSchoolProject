/// <summary>
/// Moves object along a predefined path (followPath). 
/// Continuously moves and rotates object towards each position on the path.
/// Can set to looping or not looping.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFollowPath : Movement
{
    [SerializeField]
    [Tooltip("Holds the positions on the path to follow.")]
    protected List<Transform> followPath;                 // Holds the path to follow.

    [SerializeField]
    [Tooltip("Determines whether the object will loop around the path.")]
    protected bool loopPath = false;                    // Determines whether you want to loop around the path.

    [SerializeField]
    [Tooltip("If > 1.0 then slow down as object approaches destination position.")]
    protected float dampenDistance = 1.0f;
    [SerializeField]
    [Tooltip("Minimum speed to move when dampening speed.")]
    protected float minSpeed = 0.5f;

    protected int currentGetIndex;                      // Tracks the index for which to get positions from path (get position to move towards).
    protected Vector3 directionToDestNormalized;        // Tracks the direction to the destinatio point.
    private Vector3 destPos;                            // Tracks the next position on path to move towards.
    private float updatePathTimer;
    private float originalSpeed;

    protected override void Awake()
    {
        currentGetIndex = 0;
        updatePathTimer = 0.0f;
        destPos = GetNextPointOnPath(loopPath);
        originalSpeed = speed;
    }
    // Start is called before the first frame update
    void Start()
    {
     
    }

    // Update is called once per frame
    protected override void Update()
    {
        // make sure you call base.Update in your derived Movement classes
        base.Update();
    }

    public override void UpdatePosition()
    {
        float distance = Vector3.Distance(transform.position, destPos);

        // if dampenDistance > 1.0f then slow down as object approaches destPos.
        if (dampenDistance > 1.0f && distance < dampenDistance)
        {
            speed = Mathf.Max(speed * distance / dampenDistance, minSpeed); 
           
        }

        // Move towards the current destpos.
        transform.position = Vector3.MoveTowards(transform.position, destPos, speed * Time.deltaTime);
        if (Vector3.Distance(transform.position, destPos) < 0.1f)
        {
            // Once we have reached the current destpos, find the next one on the path.
            // If we are looping OR if we are not looping and haven't reached the end of path, get the next destination point.
            if (loopPath || (currentGetIndex < followPath.Count))
            {
                destPos = GetNextPointOnPath(loopPath);
                speed = originalSpeed;
            }
        }
    }

    public override void UpdateRotation()
    {
        Quaternion targetRotation;
        // Convert the direction to leader into Quaternion then use Slerp to incrementally rotate in towards target rotation.
        directionToDestNormalized = destPos - transform.position;
        directionToDestNormalized.z = 0;
        targetRotation = GetRotationInDirection(directionToDestNormalized);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    /// <summary>
    /// Returns the next point on the path based on currentGetIndex.
    /// </summary>
    /// <param name="loopPath"></param>
    /// <returns></returns>
    protected Vector3 GetNextPointOnPath(bool loopPath)
    {
        Vector3 nextPos = new Vector3(0, 0, 0);
        if (currentGetIndex < followPath.Count)
        {
            nextPos = followPath[currentGetIndex].position;
            currentGetIndex++;
            if (currentGetIndex >= followPath.Count)
            {
                if (loopPath)
                {
                    // Loop to beginning.
                    currentGetIndex = 0;
                }
                else
                {
                    // Finished path so set index to count.
                    currentGetIndex = followPath.Count;
                }
            }
        }
        return nextPos;
    }

    private void OnDrawGizmos()
    {
        // Debug draw to envision where next point will be. 
        Debug.DrawRay(transform.position, directionToDestNormalized * 3.0f, Color.red);

        if (followPath != null)
        {
            // Draw the path to follow.
            foreach (Transform trans in followPath)
            {
                if (trans)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(trans.position, 0.2f);
                }
            }
        }
    }
    /// <summary>
    /// Determines if the transform position has reached the end of the path (last point on path)
    /// </summary>
    /// <returns></returns>
    public bool ReachedEndOfPath()
    {
        bool reachedEnd = false;
        if(!loopPath)
        {
            float distance = Vector3.Distance(transform.position, followPath[followPath.Count-1].position);
            if (currentGetIndex == followPath.Count && distance < 0.1f)
            {
                reachedEnd = true;
            }
        }
        return reachedEnd;
    }
}
