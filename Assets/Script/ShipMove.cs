using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;



public class ShipMove : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;  // Square 移动速度

    public float acceleration; // 加速度
    public float degreespeed;


    private Vector3 targetPosition; // 目标位置
    private float currentSpeed; // 物体当前速度  
    private Vector2 currentVector;
    private bool cruiseMode = true;
    private bool cruiseTarget = false;
 






    void changemode()
    {
       

        if (Input.GetKeyDown(KeyCode.E))
        {
            cruiseMode = !cruiseMode;
            cruiseTarget = false;

        }
        if (Input.GetMouseButtonDown(1))
        {
            cruiseMode = true; 

        }

        if (cruiseMode == true  )
        {
            
            if(cruiseTarget == true)
            {
                if (Input.GetMouseButtonDown(1))
                {
                    targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // 获取鼠标点击位置

                }
                MoveToTarget();
            }
           

           

            

        }
        else
        {
            targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Battlemode();
        }

    }
    void Battlemode()
    {
       
        Vector2 direction = (targetPosition - transform.position).normalized; // 计算移动方向
        float zhuanxiang = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.z = Mathf.MoveTowardsAngle(eulerAngles.z, zhuanxiang, degreespeed * Time.deltaTime);
        transform.eulerAngles = eulerAngles;//转向代码，可以让飞船朝鼠标方向旋转

        //获取输入值
        float Left = Input.GetAxis("Horizontal");
        float Forward = Input.GetAxis("Vertical");
        //计算移动的方向
        Vector2 inputDirection = new Vector2(Left, Forward).normalized;

        if (inputDirection.magnitude > 0.5f)
        {
            currentVector += inputDirection * acceleration * Time.deltaTime;
            //将currentSpeed的模长限制在speed以下
            //模长是向量的长度不会为负数
            currentVector = Vector2.ClampMagnitude(currentVector, speed);
        }
        else
        {
            //当输入值过小时，执行减速逻辑
            currentVector -= currentVector.normalized * acceleration * Time.deltaTime;

            //当速度小于一个很小的值时，将速度归零
            if (currentVector.magnitude < 0.1f)
            {
                currentVector = Vector2.zero;
            }
        }
        //移动逻辑,已自身坐标系为基准
        transform.Translate(currentVector * Time.deltaTime, Space.Self);






    }
    void Update()
    {
        changemode();
 
        
    }

    void MoveToTarget()
    {


        float Distance = Vector2.Distance(transform.position, targetPosition);//计算自身位置到鼠标点击位置的距离
        Vector2 direction = (targetPosition - transform.position).normalized; // 计算移动方向
                                                                              // 计算当速度以等同于加速度的减速度减速到0时所需的距离  
        float decelerationDistance = currentSpeed * currentSpeed / (2f * acceleration);
        if (Distance > 0.1f) // 当 Square 未到达目标位置时
        {

            if (Distance <= decelerationDistance)
            {
                // 使用减速逻辑  
                currentSpeed -= acceleration * Time.deltaTime;
                // 确保速度不会变为负数  
                currentSpeed = Mathf.Max(currentSpeed, 0f);
            }
            else
            {
                // 如果距离大于减速距离，则继续加速  
                currentSpeed += acceleration * Time.deltaTime;
                // 限制速度不超过最大速度  
                currentSpeed = Mathf.Min(currentSpeed, speed);
            }
            Vector3 movement = new Vector3(direction.x * currentSpeed * Time.deltaTime, direction.y * currentSpeed * Time.deltaTime, 0);
            transform.position += movement;


        }
        else
        {
            currentSpeed = 0;
        }
        float zhuanxiang = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        Vector3 eulerAngles = transform.eulerAngles;
        eulerAngles.z = Mathf.MoveTowardsAngle(eulerAngles.z, zhuanxiang, degreespeed * Time.deltaTime);
        transform.eulerAngles = eulerAngles;
       // Debug.Log(currentSpeed);


    }
}
    

