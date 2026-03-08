using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    Transform target;
    Vector3 velocity = Vector3.zero; 

    [Range(0, 1)]
    public float smoothTime;
    public Vector3 positionOffset;

    void Awake()
    {
        
     target = GameObject.FindGameObjectWithTag("Player").transform;
    }
    void LateUpdate()
    {
        Vector3 targetposition = target.position+ positionOffset;
        transform.position = Vector3.SmoothDamp(transform.position, targetposition, ref velocity, smoothTime);
    }

 
}
