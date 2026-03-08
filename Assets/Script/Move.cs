using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D rb; // Rigidbody2D 组件
    private Vector2 targetPosition; // 目标位置
    public NavMeshAgent NavMeshAgent;
    private bool isopen;
    public GameObject cargo;
    private int shuliang;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 获取 Rigidbody2D 组件
        NavMeshAgent.updateRotation = false;
        NavMeshAgent.updateUpAxis = false;

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log("Pressed right-click.");
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 获取鼠标点击位置

        }
        OpenMyBag();
        NavMeshAgent.SetDestination(targetPosition);
        Vector3 position = NavMeshAgent.transform.position;
        position.z = 0f;
        NavMeshAgent.transform.position = position;

    }
    void OpenMyBag()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            isopen = !isopen;
            cargo.SetActive(isopen);
        }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.tag == "Home")
        {
            
            Debug.Log("获取一只小喵");
        }

        if (collision.gameObject.tag == "Planet" )
        {
            Debug.Log("将小喵卖出获取10000欧元");
        }

    }
}

