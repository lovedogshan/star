using BehaviorDesigner.Runtime.Tasks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class MySeek : Action
{
    private Rigidbody2D rb;
    public float speed; 
    public Transform target;
    // Start is called before the first frame update

    void Onstart()
    {
        rb = GetComponent<Rigidbody2D>();//获取刚体
   
    }
   public override TaskStatus OnUpdate()//每帧调用这个方法
    {
        Debug.Log("追踪");
        if(target == null)
        {
            return TaskStatus.Failure;
        }
        Vector2 Target = new Vector2(target.position.x, target.position.y);//获取目标（玩家飞船）位置
        Vector2 Distance = Target - rb.position;//计算向量
        Vector2 Angle = Quaternion.LookRotation(Distance).eulerAngles;//将向量转换成欧拉角
        Quaternion deltaRotation = Quaternion.Euler(Angle * Time.deltaTime);//获取目标（玩家飞船）的位置并且朝向玩家的飞船转向
        rb.MoveRotation(deltaRotation);
        rb.AddForce(transform.up * speed, ForceMode2D.Force);//敌人朝玩家飞船进发
        float dis= Vector2.Distance(transform.position, Target);
        if(dis<0.1f)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }

}
