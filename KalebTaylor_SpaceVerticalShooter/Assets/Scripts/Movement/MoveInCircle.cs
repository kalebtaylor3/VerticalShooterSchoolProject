/// <summary>
/// Movement script that moves an object in a circle formation around a target object transform. 
/// Can adjust direction around circle, speed, and radius.
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoveInCircle : Movement {
    public enum CircleMovementType { Clockwise, CounterClockwise };
    public const float FULL_ROTATION = 360.0f;
    public const int MAX_POSITIONS = 40;                                // Defines the max positions around circumference.
    public const int ANGLE_INC = (int)FULL_ROTATION / MAX_POSITIONS;    // Defines the angles of positions around the circumference.
   
    [SerializeField]
    [Tooltip("Set the radius (size) of circle formation.")]
    protected float radius = 3;

    [SerializeField]
    [Tooltip("Set movement direction around circle.")]
    private CircleMovementType circleMovementType = CircleMovementType.Clockwise;

    private static List<Vector3> circlePositions;   // Static list to store positions around circumference (avoids using same calculations each frame).

    protected Vector3       centerPosition;         // Holds the center position of the circle.
    protected int           circleMoveDir;          // Tracks direction of movement (clockwise or counter-clockwise).
    protected Vector3       nextPosOnCircle;        // Tracks the next position to move to on circumference of circle.
    protected Vector3       lookAtPosition;         // Tracks the position to point to on circumference of circle.
    protected int           currentIndex;           // Current position around circumference.
     
    public Vector3 GetCenterPos() { return centerPosition; }
    public void SetCenterPos(Vector3 center) { centerPosition = center; }

    public void SetMovementType(CircleMovementType circleMove) { circleMovementType = circleMove; }

    /// <summary>
    /// Initialize the static intstance of circle positions. Do this once.
    /// </summary>
    public static List<Vector3> CircleListInstance
    {
        get
        {
            // If static list is not yet initialized, initialize it once.
            if (circlePositions == null)
            {
                circlePositions = new List<Vector3>();
                InitCirclePositions();
            }
            return circlePositions;
        }
    }

    protected override void Awake ()
    {
        circleMoveDir = -1;                                 // Clockwise direction.
        currentIndex = 0;
        centerPosition = transform.position;
    }

    void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        // Find the first position to fly to
        currentIndex = GetClosestCircleIndexToPosition(transform.position, centerPosition, radius);
        if (currentIndex == -1) Debug.LogError("Circle positions: can't get valid index");
    }

    public override void UpdatePosition()
    {
        Vector3 currentPos;  // Holds the current position of this transform.
        
        if (circleMovementType == CircleMovementType.CounterClockwise)
        {
            circleMoveDir = 1;
        }
        else
        {
            circleMoveDir = -1;
        }

        // Calculate the next position on the circumference of the circle, with given radius and center position.
        nextPosOnCircle.x = radius * CircleListInstance[currentIndex].x  + centerPosition.x;
        nextPosOnCircle.y = radius * CircleListInstance[currentIndex].y  + centerPosition.y;
        nextPosOnCircle.z = 0;

        currentPos.x = transform.position.x;
        currentPos.y = transform.position.y;
        currentPos.z = 0.0f;

        // Move towards the next position on the circumference of the circle (use speed as step). 
        currentPos = Vector3.MoveTowards(currentPos, nextPosOnCircle, Time.deltaTime * speed);
        transform.position = currentPos;

        // Check how close we are to the current target, increment to the next index if we have reached the target.
        if (Vector3.Distance(nextPosOnCircle, currentPos) < 1.0)
        {
            currentIndex += circleMoveDir;
            if (currentIndex >= MAX_POSITIONS)
            {
                currentIndex = 0;
            }
            else if( currentIndex < 0)
            {
                currentIndex = MAX_POSITIONS - 1;
            }
        }

    }

    public override void UpdateRotation()
    {
        int lookAtIndex;
        Vector3 lookDirection;
        Quaternion targetRotation;

        // Find the index of the position we should be looking at (one spot ahead).
        lookAtIndex = currentIndex + circleMoveDir;
        if (lookAtIndex >= MAX_POSITIONS)
            lookAtIndex = 0;
        else if (lookAtIndex < 0)
            lookAtIndex = MAX_POSITIONS - 1;


        // Calculate the next position to look at on the circumference of the circle.
        lookAtPosition.x = radius * CircleListInstance[lookAtIndex].x + centerPosition.x;
        lookAtPosition.y = radius * CircleListInstance[lookAtIndex].y + centerPosition.y;
        lookAtPosition.z = 0;

        // Get the direction to rotate towards (we want to rotate towards the next position "lookAtPosition").
        lookDirection = lookAtPosition - transform.position;

        // Convert the direction into Quaternion then use Slerp to incrementally rotate in proper direction.
        targetRotation = GetRotationInDirection(lookDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
   
    private void OnDrawGizmos()
    {
        Vector3 lookDirectionPosition;
        Vector3 dirToNextPosOnCircle;

        dirToNextPosOnCircle = Vector3.Normalize(nextPosOnCircle - centerPosition);
        dirToNextPosOnCircle.z = 0;

        // Scale the direction so it accounts for the current radius. 
        dirToNextPosOnCircle *= radius;

       
        // Debug draw to envision where next point will be. 
        //Debug.DrawRay(centerPosition, dirToNextPosOnCircle, Color.red);


        // Debug draw  the direction to rotate towards (we want to rotate towards the next position).
        lookDirectionPosition = Vector3.Normalize(lookAtPosition - centerPosition);
        lookDirectionPosition.z = 0;

        // Scale the direction so it accounts for the current radius. 
        lookDirectionPosition *= radius;

       //Debug.DrawRay(centerPosition, lookDirectionPosition, Color.green);


    }
    /// <summary>
    /// Create positions to form a circle around 0,0 with radius 1.
    /// </summary>
    private static void InitCirclePositions()
    {
        float angle = 0.0f;
        for (int i = 0; i < MAX_POSITIONS; i++)
        {
            Vector3 posOnCircle;
            // Calculate the next position on the circumference of the circle. 
            posOnCircle.x = Mathf.Cos(Mathf.Deg2Rad * angle);
            posOnCircle.y = Mathf.Sin(Mathf.Deg2Rad * angle);
            posOnCircle.z = 0;
            circlePositions.Add(posOnCircle);
            angle += ANGLE_INC;
        }
    }
    /// <summary>
    /// Finds the closest point on the circumference of the circle (with radius "cirRadius" and centered around
    /// "centPos") to "position". Returns the index of the closest point.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="centPos"></param>
    /// <param name="cirRadius"></param>
    /// <returns> closestIndex </returns>
    protected static int GetClosestCircleIndexToPosition(Vector3 position, Vector3 centPos, float cirRadius)
    {
        int closestIndex = -1;
        float closestDistance = Mathf.Infinity;
        for (int i = 0; i < CircleListInstance.Count; i++)
        {
            Vector3 cirPos;

            cirPos.x = cirRadius * CircleListInstance[i].x + centPos.x;
            cirPos.y = cirRadius * CircleListInstance[i].y + centPos.y;
            cirPos.z = 0.0f;

            float distance = Vector3.Distance(cirPos, position);
            if(distance < closestDistance)
            {
                // Keep track of the closest point on the circumference so far. 
                closestIndex = i;
                closestDistance = distance;
            }
        }

        return closestIndex;
    }
}
