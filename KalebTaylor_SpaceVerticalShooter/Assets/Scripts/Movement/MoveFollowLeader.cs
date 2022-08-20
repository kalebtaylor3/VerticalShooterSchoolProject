/// <summary>
/// Movement that moves towards a leader game object (very similar to MoveTowardsPlayer). 
/// Continuously moves and rotates toward a leader object.
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveFollowLeader : Movement
{

    [SerializeField]
    [Tooltip("The leader transform to follow.")]
    private  Transform leaderTransform;                 // The leader to follow.
    [SerializeField]
    [Tooltip("Defines how often to add leader position to path.")]
    private float addFrequency = 1.0f;                  // Defines how often we want to add leader position to path.
    [SerializeField]
    [Tooltip("Defines the length of the path to follow.")]
    private int maxFollowPath = 10;                     // Defines the length of the path to follow.

    private Vector3 directionToLeaderNormalized;        // Tracks the direction to the leader.
    private Vector3 destPos;                            // Tracks the next position on path to move towards.
    private float updatePathTimer;                      //
    private bool follow;                                // Defines whether to follow path OR to move in current direction.

    [SerializeField]
    [Tooltip("Holds the positions on the path to follow.")]
    private  List<Vector3> followPath;                   // Holds the path to follow.
    private int currentGetIndex;                         // Tracks the index for which to get positions from path (get position to move towards).
    private int currentAddIndex;                         // Tracks the index for which to adding positions to the path.

    public void SetLeaderTransform(Transform leader) { leaderTransform = leader; }
    public void SetFollow(bool fol) { follow = fol; }

    private void Start()
    {
        follow = true;
        updatePathTimer = 0.0f;
        currentGetIndex = 0;
        currentAddIndex = 0;
        directionToLeaderNormalized = new Vector3(0.0f, 1.0f, 0.0f);
        followPath = new List<Vector3>();
        if (leaderTransform)
        {
            destPos = leaderTransform.position;
            directionToLeaderNormalized = destPos - transform.position;
            directionToLeaderNormalized.z = 0;
            directionToLeaderNormalized.Normalize();
            transform.rotation = GetRotationInDirection(directionToLeaderNormalized); ;
        }
    }

    protected override void Update()
    {
        // make sure you call base.Update in your derived Movement classes
        base.Update();
    }

    
    public override void UpdatePosition()
    {
        bool loopPath = true;

        if (leaderTransform && follow )
        {
            // Continue to follow path as long as leader has not been destroyed.
            loopPath = true;

            // Update the path.
            updatePathTimer += Time.deltaTime;
            if (updatePathTimer > addFrequency)
            {
                updatePathTimer = 0.0f;
                // Add the leader's current position to the path.
                AddToPath(leaderTransform.position);
            }
        }
        else if (!leaderTransform && currentGetIndex < followPath.Count && follow)
        {
            // The leader has been destroyed but let's finish travelling along the remaining path.
            loopPath = false;
        }
        else
        {
            // We have reached the end of the path (leader has died and we have travelled to all remaining points on path).
            follow = false;
        }

        if (follow)
        {
            // Follow the leader.
            // Move towards the current destpos.
            transform.position = Vector3.MoveTowards(transform.position, destPos, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, destPos) < 1.0f)
            {
                // Once we have reached the current destpos, find the next one on the path.
                destPos = GetNextPointOnPath(loopPath);
            }
        }
        else
        {
            // Stop following the leader.  
            // If no leader and no points on path, continue to move in the current direction where "up" is original direction enemy is facing
            transform.Translate(Vector3.up * speed * Time.deltaTime);
        }
    }

    public override void UpdateRotation()
    {
        if (follow)
        {
            // Follow the leader.
            Quaternion targetRotation;
            // Convert the direction to leader into Quaternion then use Slerp to incrementally rotate in towards target rotation.
            directionToLeaderNormalized = destPos - transform.position;
            directionToLeaderNormalized.z = 0;
            targetRotation = GetRotationInDirection(directionToLeaderNormalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    /// <summary>
    /// Adds position to the "followPath" list if there is still room, otherwise sets position at the current "add" index.
    /// </summary>
    /// <param name="position"></param>
    private void AddToPath( Vector3 position )
    {
        
        if(followPath.Count < maxFollowPath)
        {
            // If not all spots used up then add another point on path.
            followPath.Add(position);
            currentAddIndex = 0;
        }
        else
        {
            // Already have max spots on path so now just reset points.
            followPath[currentAddIndex] = position;
            currentAddIndex++;
            if (currentAddIndex >= followPath.Count)
            {
                currentAddIndex = 0;
            }
        }

    }
    
    /// <summary>
    /// Get the next point on the path until the end is reached. If loopPath is true, then reset index to start of path (0).
    /// </summary>
    /// <param name="loopPath"></param>
    /// <returns></returns>
    private Vector3 GetNextPointOnPath(bool loopPath)
    {
        Vector3 nextPos = new Vector3(0, 0, 0);
        if (currentGetIndex < followPath.Count)
        {
            nextPos = followPath[currentGetIndex];
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
                    currentGetIndex = followPath.Count;
                }
            }
        }
        return nextPos;
    }
    private void OnDrawGizmos()
    {
        // Debug draw to envision where next point will be. 
        Debug.DrawRay(transform.position, directionToLeaderNormalized * 3.0f, Color.red);

        // Draw the path to follow.
        foreach(Vector3 pos in followPath)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(pos,0.2f);
        }
    }



}
