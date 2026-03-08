using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class xuanzhuan : MonoBehaviour
{
    private Rigidbody2D rb;
    public NavMeshAgent NavMeshAgent;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        NavMeshAgent.updateRotation = false;
        NavMeshAgent.updateUpAxis = false;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 position = NavMeshAgent.transform.position;
        position.z = 0f;
        NavMeshAgent.transform.position = position;
    }
}
