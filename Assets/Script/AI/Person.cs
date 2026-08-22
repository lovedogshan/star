
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;

public partial class Person : Animal,IAttacker, IAttacked
{
    private RoleConfig roleConfig;
    public AIBehavior IBehavior { get; private set; }

    public Shiper shiper;

    public MyGoAction OldGoAction { get; set; }

    #region �˶�
    public Constants.PersonExecuteState ExecuteState { get; set; } = Constants.PersonExecuteState.Free;

    public virtual bool IsLongRange { get; set; }




    public bool IsRun { get; set; } = false;



    public float RunIntervalTimer { get; set; }
    [System.NonSerialized]
    public List<Vector2Int> _getPaths = new List<Vector2Int>();
    #endregion

    #region Ѱ·
    public FindPath FindPathPlus { get; private set; } = new FindPath();

//   public TestGrid testGrid ;


    [HideInInspector]
    [SerializeField]
    private float onNullTimer;



    public float OnNullTimer { get => onNullTimer; set => onNullTimer = value; }
    private const float onNullTimeMax = 0.05f;






    public bool TryOnNull()
    {
//      if (place == PersonPlace.Ship)
//        {
            /* if (!MainShip.CheckOnShip(transform.position))
             {
                 for (int i = -1; i < 2; i++)
                 {
                     for (int j = -1; j < 2; j++)
                     {
                         var offset = new Vector3(i, j);
                         if (MainShip.CheckOnShip(transform.position + offset))
                         {
                             transform.position += offset;
                             Debug.Log("������������");
                             return true;
                         }
                     }
                 }
             }*/

            var timer = GameManager.Instance.gameValue.currentPeriod - OnNullTimer;
            if (timer > onNullTimeMax)
            {
//               Debug.Log($"Ѱ·����ʧ������ [{Title}] {timer * GameValue.PeriodRate:f0}s");
                OnNullTimer = GameManager.Instance.gameValue.currentPeriod;
                transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
                return true;
            }
//        }
        return false;
    }

    protected override void Start()
    {
        base.Start();
//     testGrid = GameObject.Find("TestGrid").GetComponent<TestGrid>();
        FindPathPlus.pathGrid = shiper.ShipGrid;


    }

   protected override void Awake()
    {
        base.Awake();
        
        IBehavior = GetComponent<AIBehavior>();

//      OnTestPath();



    }


    private void DebugPosition()
    {
        var offset = 0.1f;
        Debug.DrawLine(transform.position + new Vector3(-offset, -offset), transform.position + new Vector3(offset, offset), Color.green);
        Debug.DrawLine(transform.position + new Vector3(-offset, offset), transform.position + new Vector3(offset, -offset), Color.green);
    }
    #endregion

    public int _shortRange = 1;
    public int _longRange = 3;
/*    public RoleConfig RoleConfig
    { get
        {
            if(!roleConfig)
            {
                roleConfig = ConfigurationManager.Instance.RoleConfigItems[roleID];
            }
            return roleConfig;
        }
    }*/
    public int roleID;
    // ��ɫ��ȫ��
    [SerializeField]
    private string fullName;

    public BaseProperty moveSpeed;
    public float MoveSpeed
    {
        get
        {
            if(moveSpeed.Value < 1)
            {
                return 1;
            }
            else if (moveSpeed.Value > AbilityMax)
            {
                return AbilityMax;
            }
            return moveSpeed.Value;
        }
    }

    private float moveSpeedSlow;
    private const float moveSpeedSlowRate = 0.5f;
    private float moveSpeedSlowRateTemp; // ��ʱ�ƶ��ٶȼ�����
    public float MoveSpeedSlow
    {
        get
        {
            moveSpeedSlowRateTemp = moveSpeedSlowRate - skillUnit.takeLevel * SkillUnit.TakeRate;
            if (moveSpeedSlowRateTemp < 0)
            {
                moveSpeedSlowRateTemp = 0;
            }
//            moveSpeedSlow = -RoleBag.Equip.Count * moveSpeedSlowRateTemp;
            if (Mathf.Abs(moveSpeedSlow) >= (MoveSpeed - 1))
            {
                moveSpeedSlow = -(MoveSpeed - 1);
            }
            return moveSpeedSlow;
        }
    }

    // ��������
    public const float AbilityRate = 0.2f;
    private static float AbilityMax = 20;

    public BaseProperty strength; // ��������
    public BaseProperty mentality; // ��������
    public BaseProperty charm; // ��������

    // ������getter��ȷ��ֵ��0��AbilityMax֮��
    public float Strength => GetBasePropertyValue(strength);

    // �����getter��ȷ��ֵ��0��AbilityMax֮��
    public float Mentality => GetBasePropertyValue(mentality);

    // ������getter��ȷ��ֵ��0��AbilityMax֮��
    public float Charm => GetBasePropertyValue(charm);

    // ��ȡ��������ֵ�ķ�����ȷ��ֵ��0��AbilityMax֮��
    private float GetBasePropertyValue(BaseProperty baseProperty)
    {
        if (baseProperty.Value < 0)
        {
            return 0;
        }
        else if (baseProperty.Value > AbilityMax)
        {
            return AbilityMax;
        }
        return baseProperty.Value;
    }

    #region ��̬������ֵ
    [Title("������ֵ����")]
    public float MoodDegreeMax = 100; // �������ֵ
    public float SatiationDegreeMax = 100; // ��ʳ�����ֵ
    public float VigorDegreeMax = 100; // �������ֵ
    public float MoistureDegreeMax = 100; // ˮ�����ֵ

    public float MoodSpeed = 0; // ����仯�ٶ�
    public float SatiationSpeed = -50 / GameValue.PeriodRate; // ��ʳ�ȱ仯�ٶ�
    public float VigorSpeed = -70 / GameValue.PeriodRate; // �����仯�ٶ�
    public float MoistureSpeed = -50 / GameValue.PeriodRate; // ˮ�ֱ仯�ٶ�

    [Title("������ֵ")]
    public Vector2 MoodRange = new Vector2(0, 15); // ���鷶Χ
    public Vector2 SatiationRange = new Vector2(0, 15); // ��ʳ�ȷ�Χ
    public Vector2 VigorRange = new Vector2(0, 15); // ������Χ
    public Vector2 MoistureRange = new Vector2(0, 15); // ˮ�ַ�Χ
    public Vector2 HumidityRange = new Vector2(0.1f, 0.5f); // ʪ�ȷ�Χ
    public Vector2 TemperatureRange = new Vector2(0, 50); // �¶ȷ�Χ
    public Vector2 AestheticRateRange = new Vector2(-25, 25); // ���۶ȷ�Χ

    [Title("��̬������ֵ")]
    [SerializeField]
    private float moodDegree; // ��ǰ����ֵ



    public float MoodDegree
    {
        get => moodDegree;
        set
        {
            if(value < 0)
            {
                moodDegree = 0;
            }
            else if(value >MoodDegreeMax)
            {
                moodDegree = MoodDegreeMax;
            }
            else
            {
                moodDegree = value;
            }
        }
    }

    [SerializeField]
    private float satiationDegree; // ��ǰ��ʳ��

    // ��ʳ�ȵ�getter��setter��ȷ��ֵ��0��SatiationDegreeMax֮��
    public float SatiationDegree
    {
        get => satiationDegree;
        set
        {
            if ( value < 0)
            {
                satiationDegree = 0;
            }
            else if ( value > SatiationDegreeMax)
            {
                satiationDegree = SatiationDegreeMax;
            }
            else
            {
                satiationDegree = value;
            }
        }
    }

    [SerializeField]
    private float vigorDegree; // ��ǰ����ֵ

    // ����ֵ��getter��setter��ȷ��ֵ��0��VigorDegreeMax֮��
    public float VigorDegree
    {
        get => vigorDegree;
        set
        {
            if (value < 0)
            {
                vigorDegree = 0;
            }
            else if (value > VigorDegreeMax)
            {
                vigorDegree = VigorDegreeMax;
            }
            else
            {
                vigorDegree = value;
            }
        }
    }

    [SerializeField]
    private float moistureDegree; // ��ǰˮ��ֵ
    // ˮ��ֵ��getter��setter��ȷ��ֵ��0��MoistureDegreeMax֮��
    public float MoistureDegree
    {
        get => moistureDegree;
        set
        {
            if (value < 0)
            {
                moistureDegree = 0;
            }
            else if (value > MoistureDegreeMax)
            {
                moistureDegree = MoistureDegreeMax;
            }
            else
            {
                moistureDegree = value;
            }
        }
    }

    public float viewSize = 5;

    public float Interval => 1;
    public const float AttackRate = 10;

    private void PerfectBaseProperty()
    {
        MoodDegree = MoodDegreeMax; // ��������Ϊ���ֵ
        SatiationDegree = SatiationDegreeMax; // ���ñ�ʳ��Ϊ���ֵ
        VigorDegree = VigorDegreeMax; // ���û���Ϊ���ֵ
        MoistureDegree = MoistureDegreeMax; // ����ˮ��Ϊ���ֵ
        Health = HealthMax; // ���ý���Ϊ���ֵ
    }

 //   public bool CheckPlayerRole()
 //   {
  //      return Shiper == GameManager.Instance.fleet.Shiper; // ���ص�ǰ���Ƿ�����Ҵ�
/// <summary>
///    }
/// </summary>
/// <typeparam name="T"></typeparam>
/// <returns></returns>

    public T AddLifeAITask<T>() where T : AITask => AddLifeAITask<T>(new object[] { this });

    public T AddLifeAITask<T>(object[]objects) where T : AITask
    {
     //   AddReturnTask();

/*        if (CheckTaskSame<T>() != null)
        {
            return null;
        }*/

        System.Type type = typeof(T);

        var objTypes = new System.Type[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            objTypes[i] = objects[i].GetType();
        }
        var constructorInfo = type.GetConstructor(objTypes); // ��ȡ���캯��
        T obj = constructorInfo.Invoke(objects) as T; // ʵ��������
        TaskController.Instance.AddTaskToMain(obj, this); // ��������ӵ��������б�
        return obj;
    }

    private void OnTestPath()
    {
        Vector2Int targetInt = new Vector2Int(0, 0);
        var size = new Vector2Int(10, 10);
        var landGrid = new PathGrid(size)
        {
            centerPos = -targetInt + size / 2
        };
       for(int i = -5; i < 5 ; i++)
        {
            for(int j = -5;  j< 5; j++)
            {
                landGrid.AddNode(new Vector2Int(i, j), out _);
            }
        }
        FindPathPlus.pathGrid = landGrid ;
    }


/*   public T CheckTaskSame<T>() where T : AITask
    {
        var findTask = IBehavior.AITasks.Find(x => x is T) as T;
        if (findTask != null)
        {
            if (findTask.State == Constants.TaskState.stop)
            {
                TaskController.Instance.RemoveAITaskImmediate(findTask);
            }
            else
            {
                return findTask;
            }
        }
        return null;
    }






    public AITask CheckTaskSame(MyWorkTask aITask)
    {
        bool OnSame(AITask x) => x is MyWorkTask myWorkTask && myWorkTask.ConWork != aITask.ConWork && myWorkTask.ConWork is IConRecreation conRecreation && aITask.ConWork is IConRecreation conRecreation_1 && conRecreation.RecreationKind == conRecreation.RecreationKind;
        if (IBehavior.AITasks.Find(x => x.CheckSame(aITask) || OnSame(x)) != null)
        {
            aITask.AbortSafe();
            return aITask;
        }
        return null;
    }*/

    public System.Action<IAttacked> OnAttackEvent;
    public System.Action<float, IAttacker> OnHurtEvent;

    public void Attack(IAttacked attacked)
    {
        if (attacked is Biology biology && biology.IsDead)
        {
            return;
        }
        attacked.OnHurt(Strength * (AttackRate + skillUnit.attackLevel * SkillUnit.AttackRate), this);
//      GameAudioManager.Instance.AttackBGM(true);
//      GameAudioManager.Instance.PlaySound(69, false);
      OnAttackEvent?.Invoke(attacked);

//      Communicator.OnLog(0.1f, $"{ColorUtil.SetTextColor((attacked as Entity).Title, ColorUtil.ColorKind.Blue)} ������ {Communicator.GetItemsName(Communicator.relationNames_1)}");
//      BubbleEmoji.OnAttack(0.1f);
    }

    public void OnHurt(float hurt, IAttacker attacker)
    {
        if (IsDead)
        {
            return;
        }
        if (attacker is Person)
        {
            hurt *= 0.5f;
        }
        Health -= hurt;
//      m_animator.SetTrigger("isInjure");
//      GameAudioManager.Instance.AttackBGM(true);
//      TryAutoAttack(attacker);
        OnHurtEvent?.Invoke(hurt, attacker);

        /*      if (attacker != null)
                {
                    Communicator.OnLog(0.1f, $"{ColorUtil.SetTextColor((attacker as Entity).Title, ColorUtil.ColorKind.Blue)} ������ {Communicator.GetItemsName(Communicator.relationNames_1)}");
                    BubbleEmoji.OnHurt(0.1f);
                } */

        /*        if (IsDead && attacker is Person)
                {
                    AchievementManager.Instance.BrawlDieCount++;
                }*/
    }
}

// ��ӷ�������
//  public void AddReturnTask()
//  {
//      if (place == PersonPlace.Land) // �����ɫ��½����
//      {
//          var returnTask = IBehavior.AITasks.Find(task => task is MyLandingReturnTask); // ���ҷ�������
//           if (returnTask == null) // ���û���ҵ���������
//           {
//               if (ShipLinkerUnit.OpenLinker != null) // ����д򿪵�������
//               {
//                   ShipLinkerUnit.OpenLinker.TryReturn(this); // ���Է��ش���
//               }
//           }
//       }
//   }
//}
#endregion