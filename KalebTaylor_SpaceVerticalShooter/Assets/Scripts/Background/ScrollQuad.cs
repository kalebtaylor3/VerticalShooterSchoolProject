/// <summary>
/// Scrolls a quad physically in world space.
/// Used so turrets and objects can be children of the quad to move at the same rate;
/// </summary>

using UnityEngine;

public class ScrollQuad : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The speed to scroll the quad.")]
    private float scrollSpeed;        

    [SerializeField]
    [Tooltip("The Y-axis scale of the quad in world units")]
    private float tileSizeY;                   

    private Vector3 startPosition;

    void Start()
    {
        //Set the start position to be the transform position of this object.
        startPosition = transform.position;
    }

    void Update()
    {
        if (GameManager.IsGameInPlay())
        {
            //Mathf.Repeat is like the % operator. Recalll the modulus (%) operator gives you an int value within a certain range (i.e. 0 to 10)
            //Mathf.Repeat gives you a floating numberic value with in a given range (i.e. 0.0 - 10.0 )
            //the first value is the input value, the second value is the MAX value (if you input a value 20.0 and the max is 10.0 
            //then it will give you a value of 10.0, input 15.0 gives you 5.0 ).
            float newPosition = Mathf.Repeat(Time.time * scrollSpeed, tileSizeY); //constrain the values from 0 to tileSizeY

            //Change the transform position to the new position .
            transform.position = startPosition + Vector3.down * newPosition;
        }
    }
}
