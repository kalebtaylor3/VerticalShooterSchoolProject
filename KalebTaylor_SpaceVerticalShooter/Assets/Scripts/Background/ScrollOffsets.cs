/// <summary>
/// Scrolls a texture on a material along the Y axis.
/// </summary>

using UnityEngine;

public class ScrollOffsets : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The speed to scroll the the texture.")]
    private float scrollSpeed;

    private Renderer rend;
    private Vector2 savedOffset;    //Holds initial offset (only scrolling in y).

    void Start()
    {
        rend = GetComponent<Renderer>();
        savedOffset = rend.material.GetTextureOffset("_MainTex");
    }

    void Update()
    {
        if (GameManager.IsGameInPlay())
        {
            float y = Mathf.Repeat(Time.time * scrollSpeed, 1);  //Update the y between 0 and 1.
            Vector2 offset = new Vector2(savedOffset.x, y);
            rend.material.SetTextureOffset("_MainTex", offset);
        }
    }

    void OnDisable()
    {
        //Reset offset to initial offset when game object is disabled.
        rend.material.SetTextureOffset("_MainTex", savedOffset);
    }
}
