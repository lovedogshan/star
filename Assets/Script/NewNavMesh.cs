using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NewNavMesh : MonoBehaviour
{
    public NavMeshSurface navMeshSurface;

    // Start is called before the first frame update
    void Start()
    {
        navMeshSurface.BuildNavMesh();
    }
    private void Update()
    {
       
    }
}
