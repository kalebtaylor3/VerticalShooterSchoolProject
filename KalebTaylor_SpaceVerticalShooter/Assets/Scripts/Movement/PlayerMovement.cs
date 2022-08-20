/// <summary>
/// Player Movement script why uses keypoard input to move the player.
/// </summary>
using UnityEngine;

public class PlayerMovement : Movement
{
    private Vector3 normalizedVelocity = Vector3.zero;
    public Vector3 NormalizedVelocity
    {
        get { return normalizedVelocity; }
    }

    public override void UpdatePosition()
    {
        //Get direction in x and y via the vertical and horizontal inputs.
        float dy = Input.GetAxis("Vertical");
        float dx = Input.GetAxis("Horizontal");

        normalizedVelocity = new Vector3(dx, dy, 0);

        //Normalize the vector if one of the axis is >=1
        //this is to ensure that a greater speed can;t be achieved diagonally
        //and maintain the simulated analog control when dx and dy are < 1.
        if (dx >= 1 || dy >= 1)
        {
            normalizedVelocity.Normalize();
        }

        if(dx < 0.0f || dx > 0.0f)
        {
            int ibreak = 0;

            ibreak = 1;
        }
        if (dy < 0.0f || dy > 0.0f)
        {
            int ibreak = 0;

            ibreak = 1;
        }


        //Translate in the proper direction based on inputs.
        transform.Translate(normalizedVelocity * speed * Time.deltaTime);

        //Check if player is in bounds.
        CheckBounds();
    }

    public override void UpdateRotation()
    {
        //player does not rotate
    }

    /// <summary>
    /// Clamps the player's position to be in bounds of play area
    /// </summary>
    private void CheckBounds()
    {
        float x = transform.position.x;
        float y = transform.position.y;

        x = Mathf.Clamp(x, GameManager.MIN_X, GameManager.MAX_X);
        y = Mathf.Clamp(y, GameManager.MIN_Y, GameManager.MAX_Y);

        transform.position = new Vector3(x, y, transform.position.z);
    }
}
