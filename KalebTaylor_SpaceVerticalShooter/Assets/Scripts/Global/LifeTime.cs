/// <summary>
/// Generic script to destroy a game object with this component after a set amount of time
/// </summary>
using UnityEngine;

public class LifeTime : MonoBehaviour
{
    [SerializeField]
    private float lifeTime = 5f;


    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    protected void Update()
    {
        if (GameManager.GameOver())
        {
            DestroyImmediate(gameObject);
        }
    }
}
