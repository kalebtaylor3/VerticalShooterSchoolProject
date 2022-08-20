/// <summary>
/// Movement script that moves an object in a spiral formation around a target object transform. 
/// Can adjust the speed of the change in radius.
/// </summary>
using UnityEngine;

public class MoveInSpiral : MoveInCircle {
                   
    // Defines angles from which to fly in to spiral or out of spiral.
    public enum FlyFromType { None = -1, Right = 0, TopRight = 45, Top = 90, TopLeft = 135, Left = 180, BottomLeft = 225, Bottom = 270,  BottomRight = 315  };
    
    // Defines states of spiral movement of enemy.
    public enum FlyState { FlySpiral, FlyIn, FlyOut };  // Default state is to spiral with no fly in / fly out.


    [SerializeField]
    [Tooltip("Set the rate at which the radius changes in spiral motion. Should always be positive.")]
    [Range(0.0f, 10.0f)]
    private float radiusInc = 0.5f;

    [SerializeField]
    [Tooltip("Set the final radius. No spiral if radius and finalRadius are equal.")]
    private float finalRadius = 3;

    [SerializeField]
    [Tooltip("Set direction object will fly out from.")]
    private FlyFromType  flyOutFrom= FlyFromType.Bottom;


    private bool adjustRadius;              // Tracks whether to adjust radius for spiral motion.
    private bool flyOut;                    // Defines if it is time to fly out of spiral motion.
    private FlyState flyState;              // Tracks state of motion.
    

    public void SetFlyState(FlyState state) { flyState = state; }
    public void SetFlyOutFrom(FlyFromType flyOut) { flyOutFrom = flyOut; }
    public void SetStartRadius(float inStartRadius) { radius = inStartRadius; }
    public void SetEndRadius(float inFinalRadius) { finalRadius = inFinalRadius; }
    public void SetRadiusInc(float inRadiusInc) { radiusInc = inRadiusInc; }


    void Start()
    {
        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        flyState = FlyState.FlyIn;
        flyOut = false;
        adjustRadius = false;

        // Determine whether we need to adjust radius for spiral.
        if (finalRadius != radius)
            adjustRadius = true;

        // Be sure radius adjustment is correct direction.
        if (finalRadius < radius)
            radiusInc *= -1.0f;

        
    }

    public override void UpdatePosition()
    {
      
        // Check fly state.
        if (flyState == FlyState.FlyIn)
        {
            // Call base function to update position.
            base.UpdatePosition();
            
            // Once position is updated, see if we are close to position on circumference to start spiral.
            if(Vector3.Distance(transform.position, nextPosOnCircle) < 1.0f)
            {
                flyState = FlyState.FlySpiral;
            }
        }
        else if (flyState == FlyState.FlyOut)
        {
            // Fly to out in current direction.
            transform.Translate(Vector3.up * speed * Time.deltaTime );
        }
        else if (flyState == FlyState.FlySpiral)
        {
            // Adjust the radius to mimic a spiral motion.
            if (adjustRadius)
            {
                radius += (radiusInc * Time.deltaTime);
                if (radius < 0.0f)
                    radius = 0.0f;

                // Constrain the radius when it reaches the limit. Check whether we are  decrementing or  incrementing, and check appropriate limit.
                if ((radiusInc < 0.0f && radius < finalRadius) || (radiusInc > 0.0f && radius > finalRadius))
                {
                    adjustRadius = false;
                    flyOut = true;
                }
            }

            // Call base function to update position.
            base.UpdatePosition();

            // After position is updated and we have reached the final radius size, see if we can fly out.
            if (flyOut && flyOutFrom != FlyFromType.None )
            {
                Vector3 flyOutCirclePos;        
                float angle = (float)flyOutFrom;

                // Find the position on the circumference for which we fly out from.
                flyOutCirclePos.x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius + centerPosition.x;
                flyOutCirclePos.y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius + centerPosition.y;
                flyOutCirclePos.z = 0;
                // After spiraling is complete, see if we are close to the exit angle/position.
                if (Vector3.Distance(transform.position, flyOutCirclePos) < 1.0f)
                {
                    flyState = FlyState.FlyOut;
                }
            }
        }
    }

    public override void UpdateRotation()
    {
        // Check fly state. Update rotation only if flying in or spiraling
        if (flyState == FlyState.FlyIn || flyState == FlyState.FlySpiral)
        {
            // Move to fly in point and then begin spiral when ready.
            base.UpdateRotation();

        }
    }
}
