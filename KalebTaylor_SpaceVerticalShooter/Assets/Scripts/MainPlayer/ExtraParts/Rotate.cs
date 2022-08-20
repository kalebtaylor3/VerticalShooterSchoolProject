/// <summary>
/// Rotates a game object about Z-axiz. Used primarily for fan and other player attachments. 
/// </summary>
using UnityEngine;

public class Rotate : MonoBehaviour 
{
    [SerializeField]
    private float rotateSpeed = 180f;

	void Update() 
	{
        transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
    }
}
