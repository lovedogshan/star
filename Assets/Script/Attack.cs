using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Attack : MonoBehaviour
{
    public GameObject shell;
    public int damage;
    public float attackCooldown = 0.5f;
    public Transform firePoint;
    public int Ranger;
    public bool npc = false;
    private float attackTimer;//���ù���Ƶ��
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // 如果装配界面开启，禁止开火逻辑
        if (SpaceshipAssemblyUI.IsUIOpen) return;

        if (npc == false)
        {
            if (Input.GetMouseButton(0))
            {
                attack();
            }
        }
        else
        {
            Npcfire();
        }
    }
    void fire()
    {
        GameObject bullet = Instantiate(shell, firePoint.position, firePoint.rotation);
        

    }
    void attack()
    {
        attackTimer += Time.deltaTime;



        if (attackTimer >= attackCooldown)
        {
            fire();
            attackTimer = 0f;
        }
    }
    void Npcfire()
    {
        RaycastHit2D hit = Physics2D.Raycast(firePoint.transform.position, firePoint.up);
        GameObject target = hit.collider.gameObject;
        if (target == GameObject.Find("ship"))
        {
            fire();
        }
    }
    void disappear()
    {
        float distanceTraveled = Vector2.Distance(firePoint.position, shell.transform.position);
        Debug.Log(distanceTraveled);
        if (distanceTraveled > Ranger)
        {
            Destroy(shell);
        }
    }
}
