using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class Move : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D rb; // Rigidbody2D ���
    private Vector2 targetPosition; // Ŀ��λ��
    public NavMeshAgent NavMeshAgent;
    private bool isopen;
    public GameObject cargo;
    private int shuliang;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ��ȡ Rigidbody2D ���
        NavMeshAgent.updateRotation = false;
        NavMeshAgent.updateUpAxis = false;

    }

    void Update()
    {
        // 如果装配界面开启，禁止移动和背包逻辑
        if (SpaceshipAssemblyUI.IsUIOpen) return;

        // 如果点击在 UI 上，不处理鼠标点击
        bool isPointerOverUI = UnityEngine.EventSystems.EventSystem.current != null && 
                               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if (Input.GetMouseButtonDown(1) && !isPointerOverUI)
        {
            Debug.Log("Pressed right-click.");
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ��ȡ�����λ��

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
            
            Debug.Log("��ȡһֻС��");
        }

        if (collision.gameObject.tag == "Planet" )
        {
            Debug.Log("��С��������ȡ10000ŷԪ");
        }

    }
}

