using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMove : MonoBehaviour
{
    public float detectiverange;
    public NavMeshAgent NavMeshAgent;
    public List<GameObject> waypoint = new List<GameObject>();
    public float attackrange;
    private Vector3 Target;
    private int nextpointindex=0;
    

    // Start is called before the first frame update
    void Start()
    {
        NavMeshAgent.updateRotation = false;
        NavMeshAgent.updateUpAxis = false;
       



    }
    private void Awake()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        partol();
        follow();
    }
    void partol()//设置巡逻的路径，让敌人沿着路径点移动
    {
        Debug.Log("巡逻");
        NavMeshAgent.SetDestination(waypoint[nextpointindex].transform.position);
        float distance = Vector3.Magnitude(transform.position - waypoint[nextpointindex].transform.position);
        if(distance<0.2f)
        {
            //计算下一个目标点。（当前下标+1）与数组长度取余数
            nextpointindex = (nextpointindex + 1) % waypoint.Count;
            NavMeshAgent.SetDestination(waypoint[nextpointindex].transform.position);    //更换目标点
            Debug.Log(nextpointindex);
            if(nextpointindex> waypoint.Count)
            {
                nextpointindex = 0;
            }
            
        }
       

    }
    void follow()
    {

        float range = Vector2.Distance(GameObject.Find("ship").transform.position, transform.position);
        Debug.Log(range);
        if (range <= detectiverange)
        {
            Vector3 direcrion = -(GameObject.Find("ship").transform.position - transform.position).normalized;
            Vector3 targetPosition = GameObject.Find("ship").transform.position + direcrion * attackrange;
            float offset = Vector2.Distance(transform.position, targetPosition);
            if (offset <= 2f)
            {
                NavMeshAgent.isStopped = true;
            }
            else if (range < attackrange)
            {

                NavMeshAgent.isStopped = false;
            //    NavMeshAgent.updateRotation = false;
                //NavMeshAgent.transform.LookAt(GameObject.Find("ship").transform.position, -transform.up);
                NavMeshAgent.Move(-transform.up*NavMeshAgent.speed*Time.deltaTime);

            }
            else
            {
                NavMeshAgent.SetDestination(targetPosition);
                NavMeshAgent.isStopped = false;
            }
        }



        }
    }

