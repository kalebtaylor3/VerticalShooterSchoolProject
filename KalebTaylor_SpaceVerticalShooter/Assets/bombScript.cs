using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bombScript : MonoBehaviour
{

    [Header("Scaling")]
    [SerializeField]
    private Vector3 scaleChangeRate = new Vector3(0.5f, 0.5f, 0.5f);

    [Header("Life Time")]
    [SerializeField]
    private int lifetime = 5;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.localScale += scaleChangeRate * Time.deltaTime;
        StartCoroutine(DestoryCounter());
    }

    IEnumerator DestoryCounter()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if(collision.gameObject.tag == "Enemy")
        {
            Debug.Log("you hit" + collision.name);
        }
    }

}
