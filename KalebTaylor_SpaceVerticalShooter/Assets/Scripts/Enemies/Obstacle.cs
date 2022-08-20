/// <summary>
/// Simple obstacle using Interactive Body as its parent class
/// (Required because InteractiveBody is abstract. May also want extra code added to Obstacle)
/// </summary>

public class Obstacle : InteractiveBody
{

    private Movement movement;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<Movement>();
    }

    /// <summary>
    /// Called on completion of death animation
    /// Uses to make any changes in between animation completion and death completion
    /// typically used for disabling movement 
    /// </summary>
    protected override void DeathAnimationComplete()
    {
        base.DeathAnimationComplete();
        if (movement)
        {
            movement.enabled = false;
        }
    }
}
