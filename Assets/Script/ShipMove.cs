using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;



public class ShipMove : MonoBehaviour
{
    // Start is called before the first frame update
    public float speed;  // Square �ƶ��ٶ�

    public float acceleration; // ���ٶ�
    public float degreespeed;


    private Vector3 targetPosition; // Ŀ��λ��
    private float currentSpeed; // ���嵱ǰ�ٶ�  
    private Vector2 currentVector;
    private bool cruiseMode = true;
    private bool cruiseTarget = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        // 建议设置 Rigidbody2D 的 Gravity Scale 为 0，且 Collision Detection 为 Continuous
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.angularDrag = 0; // 禁用物理阻力，由代码控制转向
        }
    }

    void Update()
    {
        // 如果装配界面开启，禁止移动和转向，并且重置速度防止惯性
        if (SpaceshipAssemblyUI.IsUIOpen) 
        {
            currentSpeed = 0;
            currentVector = Vector2.zero;
            if (rb != null) rb.velocity = Vector2.zero;
            targetPosition = transform.position;
            return;
        }

        // 处理输入模式切换和目标点更新
        changemode();
    }

    void FixedUpdate()
    {
        // 物理逻辑移动到 FixedUpdate 执行，防止颤抖
        if (SpaceshipAssemblyUI.IsUIOpen) return;

        if (cruiseMode == true)
        {
            if (cruiseTarget == true)
            {
                MoveToTarget();
            }
            else
            {
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            Battlemode();
        }
    }

    void changemode()
    {
        // 确保点击不是在 UI 上
        bool isPointerOverUI = UnityEngine.EventSystems.EventSystem.current != null && 
                               UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if (Input.GetKeyDown(KeyCode.X))
        {
            cruiseMode = !cruiseMode;
            cruiseTarget = false;

        }
        if (Input.GetMouseButtonDown(1) && !isPointerOverUI)
        {
            cruiseMode = true; 

        }

        if (cruiseMode == true  )
        {
            
            if(cruiseTarget == true)
            {
                if (Input.GetMouseButtonDown(1) && !isPointerOverUI)
                {
                    targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // ��ȡ�����λ��

                }
                // MoveToTarget(); // 移到 Update 统一调用
            }
           

           

            

        }
        else
        {
            // 在战斗模式下，如果没有右键点击，targetPosition 应该跟随鼠标位置（这样飞船才能实时转向鼠标）
            // 除非你想让它像 cruiseMode 一样只有点击才更新目标
            // 之前的逻辑是：
             if (Input.GetMouseButtonDown(1) && !isPointerOverUI)
            {
                targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
            else if (!isPointerOverUI) // 新增：如果不是右键点击，也实时更新朝向目标为鼠标位置？
            {
                // 注意：如果 Battlemode 的设计是“右键点击移动，键盘控制移动，鼠标控制朝向”
                // 那么这里可能需要一直更新 targetPosition 为鼠标位置
                targetPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            }
        }

    }
    void Battlemode()
    {
        Vector2 direction = (targetPosition - transform.position).normalized; 
        float zhuanxiang = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        
        // 使用刚体旋转保持物理同步
        float newRotation = Mathf.MoveTowardsAngle(rb.rotation, zhuanxiang, degreespeed * Time.deltaTime);
        rb.rotation = newRotation;

        // 获取输入值
        float Left = Input.GetAxis("Horizontal");
        float Forward = Input.GetAxis("Vertical");
        Vector2 inputDirection = new Vector2(Left, Forward).normalized;

        if (inputDirection.magnitude > 0.5f)
        {
            currentVector += inputDirection * acceleration * Time.deltaTime;
            currentVector = Vector2.ClampMagnitude(currentVector, speed);
        }
        else
        {
            currentVector -= currentVector.normalized * acceleration * Time.deltaTime;
            if (currentVector.magnitude < 0.1f)
            {
                currentVector = Vector2.zero;
            }
        }
        
        // 关键修改：将本地坐标系的速度转换为世界坐标系速度赋值给刚体
        // 这样可以支持碰撞，因为物理引擎会处理刚体的 velocity
        rb.velocity = transform.TransformDirection(currentVector);
    }

    void MoveToTarget()
    {
        float Distance = Vector2.Distance(transform.position, targetPosition);
        Vector2 direction = (targetPosition - transform.position).normalized; 
        float decelerationDistance = currentSpeed * currentSpeed / (2f * acceleration);
        
        if (Distance > 0.1f) 
        {
            if (Distance <= decelerationDistance)
            {
                currentSpeed -= acceleration * Time.deltaTime;
                currentSpeed = Mathf.Max(currentSpeed, 0f);
            }
            else
            {
                currentSpeed += acceleration * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, speed);
            }
            // 使用刚体速度代替直接操作 Transform
            rb.velocity = direction * currentSpeed;
        }
        else
        {
            currentSpeed = 0;
            rb.velocity = Vector2.zero;
        }

        float zhuanxiang = Mathf.Atan2(-direction.x, direction.y) * Mathf.Rad2Deg;
        float newRotation = Mathf.MoveTowardsAngle(rb.rotation, zhuanxiang, degreespeed * Time.deltaTime);
        rb.rotation = newRotation;
    }
}
    

