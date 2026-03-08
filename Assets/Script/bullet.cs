using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bullet : MonoBehaviour
{
    // Start is called before the first frame update
    public float damage;
    public float shellspeed;
    public List<GameObject> spaceship;
    private void Awake()
    {
 
    }

    private void Update()
    {
        transform.Translate(Vector2.up * shellspeed * Time.deltaTime);
        
    }
    public void SetSpaceship(List<GameObject> spaceship)
    {
        this.spaceship = spaceship;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (GameObject part in spaceship)
        {
            if (collision.gameObject == part)
            {
                // 如果是父对象，则忽略碰撞  
                Physics2D.IgnoreCollision(part.GetComponent<Collider2D>(), GetComponent<Collider2D>(), true);
                return;
            }
          
        }
        Destroy(gameObject);
      //  Debug.Log(gameObject);


    }

    
}
