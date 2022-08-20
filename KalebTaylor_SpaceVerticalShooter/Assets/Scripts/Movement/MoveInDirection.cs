/// <summary>
/// Moves a mody in a specific direction based.
/// </summary>

using UnityEngine;

public class MoveInDirection : Movement
{
    [SerializeField]
    [Tooltip("X,Y Vector to represent direction of movement in WORLD space. Note: Will be normalized and speed will determine magnitude.")]
    private Vector2 direction = Vector2.zero;
    public Vector2 Direction
    {
        get { return direction; }
        set {  direction = value; direction.Normalize(); }
    }

    private void Awake()
    {
        direction.Normalize();
    }

    public override void UpdatePosition()
    {
        //translate in the proper direction based on inputs
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    public override void UpdateRotation()
    {
        //no rotation for this movement
    }
}
