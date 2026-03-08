using NavMeshPlus.Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    [Header("ÎÞÏÞµØÍ¼")]
    public GameObject mainCamera;
    public float mapWidth;
    public float mapHeight;
    public int mapNums;
    public NavMeshSurface navMeshSurface;
    private float totalWidth;
    private float totalHeight;
    void Start()
    {
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        mapWidth = GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        totalWidth = mapWidth * mapNums;
        mapHeight = GetComponent<SpriteRenderer>().sprite.bounds.size.y;
        totalHeight = mapHeight * mapNums;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tempPosition = transform.position;
        if(mainCamera.transform.position.x>transform.position.x+ totalWidth/2)
        {
            tempPosition.x += totalWidth;
            transform.position = tempPosition;
            navMeshSurface.BuildNavMesh();
        }
        else if(mainCamera.transform.position.x<transform.position.x - totalWidth/2)
        {
            tempPosition.x -= totalWidth;
            transform.position = tempPosition;
            navMeshSurface.BuildNavMesh();
        }
        if (mainCamera.transform.position.y > transform.position.y + totalHeight / 2)
        {
            tempPosition.y += totalHeight;
            transform.position = tempPosition;
            navMeshSurface.BuildNavMesh();
        }
        else if (mainCamera.transform.position.y < transform.position.y - totalHeight / 2)
        {
            tempPosition.y -= totalHeight;
            transform.position = tempPosition;
            navMeshSurface.BuildNavMesh();
        }
    }
}
