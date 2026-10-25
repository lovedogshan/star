
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

    public PersonNavigation Navigation { get; private set; }
    public EvaCapability EvaCapability { get; private set; }
    [SerializeField]
    private PersonWorkProfile workProfile = new PersonWorkProfile();
    [SerializeField]
    private PersonScheduleProfile scheduleProfile = new PersonScheduleProfile();

    public virtual bool CanReceiveAutomaticTask => IBehavior != null;

    public PersonWorkProfile WorkProfile
    {
        get
        {
            if (workProfile == null)
            {
                workProfile = new PersonWorkProfile();
            }

            return workProfile;
        }
    }

    public PersonScheduleProfile ScheduleProfile
    {
        get
        {
            if (scheduleProfile == null)
            {
                scheduleProfile = new PersonScheduleProfile();
            }

            return scheduleProfile;
        }
    }
    public NavigationRegion CurrentRegion { get; private set; }
    public int CurrentRegionId => CurrentRegion ? CurrentRegion.RegionId : shiper ? shiper.GetInstanceID() : AITask.AnyRegionId;

    #region 锟剿讹拷
    public Constants.PersonExecuteState ExecuteState { get; set; } = Constants.PersonExecuteState.Free;

    public virtual bool IsLongRange { get; set; }




    public bool IsRun { get; set; } = false;



    public float RunIntervalTimer { get; set; }
    [System.NonSerialized]
    public List<Vector2Int> _getPaths = new List<Vector2Int>();
    #endregion

    #region 寻路
    public FindPath FindPathPlus { get; private set; } = new FindPath();

//   public TestGrid testGrid ;


    [HideInInspector]
    [SerializeField]
    private float onNullTimer;



    public float OnNullTimer { get => onNullTimer; set => onNullTimer = value; }
    private const float onNullTimeMax = 0.05f;






    public bool TryOnNull()
    {
        if (!GameManager.Instance || GameManager.Instance.gameValue == null)
        {
            return false;
        }

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
                             Debug.Log("锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷");
                             return true;
                         }
                     }
                 }
             }*/

            var timer = GameManager.Instance.gameValue.currentPeriod - OnNullTimer;
            if (timer > onNullTimeMax)
            {
//               Debug.Log($"寻路锟斤拷锟斤拷失锟斤拷锟斤拷锟斤拷 [{Title}] {timer * GameValue.PeriodRate:f0}s");
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
        if (!shiper)
        {
            shiper = GetComponentInParent<Shiper>();
        }

        if (shiper)
        {
            FindPathPlus.pathGrid = shiper.ShipGrid;
        }

        if (!CurrentRegion)
        {
            SetCurrentRegion(GetComponentInParent<NavigationRegion>());
        }


    }

    protected override void Awake()
    {
        base.Awake();
        IBehavior = GetComponent<AIBehavior>();
        Navigation = GetComponent<PersonNavigation>();
        if (!Navigation)
        {
            Navigation = gameObject.AddComponent<PersonNavigation>();
        }

        EvaCapability = GetComponent<EvaCapability>();
        TaskController.Instance?.AddPerson(this);

//      OnTestPath();



    }


    public void SetCurrentRegion(NavigationRegion region)
    {
        CurrentRegion = region;
        if (region && region.Grid != null)
        {
            FindPathPlus.pathGrid = region.Grid;
        }
    }

    public void SetWorkPriority(Constants.WorkKind workKind, int priority)
    {
        if (!WorkProfile.SetPriority(workKind, priority))
        {
            return;
        }

        JobSystem.Instance?.NotifyPersonChanged(this);
        TaskController.Instance?.RequestAssignment(this);
    }

    public void SetScheduleActivity(int slotIndex, ScheduleActivity activity)
    {
        if (!ScheduleProfile.SetActivity(slotIndex, activity))
        {
            return;
        }

        ScheduleSystem.Instance?.NotifyProfileChanged(this);
    }

    public bool TryMoveSafely(Vector2 desiredDelta, out Vector2 actualDelta)
    {
        actualDelta = Vector2.zero;
        LastMovementBlocker = null;
        float distance = desiredDelta.magnitude;
        if (distance <= 0.0001f)
        {
            return true;
        }

        Vector2 direction = desiredDelta / distance;
        Collider2D movementCollider = GetComponent<Collider2D>();
        float safeDistance = distance;
        bool blocked = false;

        if (movementCollider && movementCollider.enabled)
        {
            ContactFilter2D filter = new ContactFilter2D();
            filter.SetLayerMask(Physics2D.AllLayers);
            filter.useTriggers = false;
            RaycastHit2D[] hits = new RaycastHit2D[16];
            int hitCount = movementCollider.Cast(direction, filter, hits, distance + 0.01f);
            for (int i = 0; i < hitCount; i++)
            {
                RaycastHit2D hit = hits[i];
                if (!hit.collider || hit.collider == movementCollider || hit.collider.isTrigger ||
                    hit.collider.GetComponentInParent<Person>() == this ||
                    hit.distance <= 0.001f && IsMovingAwayFromCollider(
                        hit.collider,
                        transform.position,
                        (Vector2)transform.position + desiredDelta))
                {
                    continue;
                }

                blocked = true;
                if (!LastMovementBlocker || hit.distance < safeDistance)
                {
                    LastMovementBlocker = hit.collider;
                }
                safeDistance = Mathf.Min(safeDistance, Mathf.Max(0f, hit.distance - 0.01f));
            }
        }

        Vector2 move = direction * safeDistance;
        transform.position += (Vector3)move;
        actualDelta = move;
        return !blocked || safeDistance >= distance - 0.0001f;
    }

    private static bool IsMovingAwayFromCollider(Collider2D blocker, Vector2 from, Vector2 to)
    {
        if (!blocker)
        {
            return false;
        }

        float fromDistance = Vector2.SqrMagnitude(from - blocker.ClosestPoint(from));
        float toDistance = Vector2.SqrMagnitude(to - blocker.ClosestPoint(to));
        return toDistance > fromDistance + 0.0001f;
    }

    [System.NonSerialized]
    private Collider2D lastMovementBlocker;
    public Collider2D LastMovementBlocker
    {
        get => lastMovementBlocker;
        private set => lastMovementBlocker = value;
    }

    protected override void OnDestroy()
    {
        Navigation?.Cancel();
        ReservationManager.Instance?.ReleaseAll(this);
        if (TaskController.Instance)
        {
            TaskController.Instance.RemovePerson(this);
        }

        ScheduleSystem.Instance?.Unregister(this);

        base.OnDestroy();
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
    // 锟斤拷色锟斤拷全锟斤拷
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
    private float moveSpeedSlowRateTemp; // 锟斤拷时锟狡讹拷锟劫度硷拷锟斤拷锟斤拷
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

    // 锟斤拷锟斤拷锟斤拷锟斤拷
    public const float AbilityRate = 0.2f;
    private static float AbilityMax = 20;

    public BaseProperty strength; // 锟斤拷锟斤拷锟斤拷锟斤拷
    public BaseProperty mentality; // 锟斤拷锟斤拷锟斤拷锟斤拷
    public BaseProperty charm; // 锟斤拷锟斤拷锟斤拷锟斤拷

    // 锟斤拷锟斤拷锟斤拷getter锟斤拷确锟斤拷值锟斤拷0锟斤拷AbilityMax之锟斤拷
    public float Strength => GetBasePropertyValue(strength);

    // 锟斤拷锟斤拷锟絞etter锟斤拷确锟斤拷值锟斤拷0锟斤拷AbilityMax之锟斤拷
    public float Mentality => GetBasePropertyValue(mentality);

    // 锟斤拷锟斤拷锟斤拷getter锟斤拷确锟斤拷值锟斤拷0锟斤拷AbilityMax之锟斤拷
    public float Charm => GetBasePropertyValue(charm);

    // 锟斤拷取锟斤拷锟斤拷锟斤拷锟斤拷值锟侥凤拷锟斤拷锟斤拷确锟斤拷值锟斤拷0锟斤拷AbilityMax之锟斤拷
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

    #region 锟斤拷态锟斤拷锟斤拷锟斤拷值
    [Title("锟斤拷锟斤拷锟斤拷值锟斤拷锟斤拷")]
    public float MoodDegreeMax = 100; // 锟斤拷锟斤拷锟斤拷锟街?
    public float SatiationDegreeMax = 100; // 锟斤拷食锟斤拷锟斤拷锟街?
    public float VigorDegreeMax = 100; // 锟斤拷锟斤拷锟斤拷锟街?
    public float MoistureDegreeMax = 100; // 水锟斤拷锟斤拷锟街?

    public float MoodSpeed = 0; // 锟斤拷锟斤拷浠拷俣锟?
    public float SatiationSpeed = -50 / GameValue.PeriodRate; // 锟斤拷食锟饺变化锟劫讹拷
    public float VigorSpeed = -70 / GameValue.PeriodRate; // 锟斤拷锟斤拷锟戒化锟劫讹拷
    public float MoistureSpeed = -50 / GameValue.PeriodRate; // 水锟街变化锟劫讹拷

    [Title("锟斤拷锟斤拷锟斤拷值")]
    public Vector2 MoodRange = new Vector2(0, 15); // 锟斤拷锟介范围
    public Vector2 SatiationRange = new Vector2(0, 15); // 锟斤拷食锟饺凤拷围
    public Vector2 VigorRange = new Vector2(0, 15); // 锟斤拷锟斤拷锟斤拷围
    public Vector2 MoistureRange = new Vector2(0, 15); // 水锟街凤拷围
    public Vector2 HumidityRange = new Vector2(0.1f, 0.5f); // 湿锟饺凤拷围
    public Vector2 TemperatureRange = new Vector2(0, 50); // 锟铰度凤拷围
    public Vector2 AestheticRateRange = new Vector2(-25, 25); // 锟斤拷锟桔度凤拷围

    [Title("锟斤拷态锟斤拷锟斤拷锟斤拷值")]
    [SerializeField]
    private float moodDegree; // 锟斤拷前锟斤拷锟斤拷值



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
    private float satiationDegree; // 锟斤拷前锟斤拷食锟斤拷

    // 锟斤拷食锟饺碉拷getter锟斤拷setter锟斤拷确锟斤拷值锟斤拷0锟斤拷SatiationDegreeMax之锟斤拷
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
    private float vigorDegree; // 锟斤拷前锟斤拷锟斤拷值

    // 锟斤拷锟斤拷值锟斤拷getter锟斤拷setter锟斤拷确锟斤拷值锟斤拷0锟斤拷VigorDegreeMax之锟斤拷
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
    private float moistureDegree; // 锟斤拷前水锟斤拷值
    // 水锟斤拷值锟斤拷getter锟斤拷setter锟斤拷确锟斤拷值锟斤拷0锟斤拷MoistureDegreeMax之锟斤拷
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
        MoodDegree = MoodDegreeMax; // 锟斤拷锟斤拷锟斤拷锟斤拷为锟斤拷锟街?
        SatiationDegree = SatiationDegreeMax; // 锟斤拷锟矫憋拷食锟斤拷为锟斤拷锟街?
        VigorDegree = VigorDegreeMax; // 锟斤拷锟矫伙拷锟斤拷为锟斤拷锟街?
        MoistureDegree = MoistureDegreeMax; // 锟斤拷锟斤拷水锟斤拷为锟斤拷锟街?
        Health = HealthMax; // 锟斤拷锟矫斤拷锟斤拷为锟斤拷锟街?
    }

 //   public bool CheckPlayerRole()
 //   {
  //      return Shiper == GameManager.Instance.fleet.Shiper; // 锟斤拷锟截碉拷前锟斤拷锟角凤拷锟斤拷锟斤拷掖锟?
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
        var constructorInfo = type.GetConstructor(objTypes); // 锟斤拷取锟斤拷锟届函锟斤拷
        T obj = constructorInfo.Invoke(objects) as T; // 实锟斤拷锟斤拷锟斤拷锟斤拷
        TaskController.Instance.AddTaskToMain(obj, this); // 锟斤拷锟斤拷锟斤拷锟斤拷拥锟斤拷锟斤拷锟斤拷锟斤拷斜锟?
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

//      Communicator.OnLog(0.1f, $"{ColorUtil.SetTextColor((attacked as Entity).Title, ColorUtil.ColorKind.Blue)} 锟斤拷锟斤拷锟斤拷 {Communicator.GetItemsName(Communicator.relationNames_1)}");
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
                    Communicator.OnLog(0.1f, $"{ColorUtil.SetTextColor((attacker as Entity).Title, ColorUtil.ColorKind.Blue)} 锟斤拷锟斤拷锟斤拷 {Communicator.GetItemsName(Communicator.relationNames_1)}");
                    BubbleEmoji.OnHurt(0.1f);
                } */

        /*        if (IsDead && attacker is Person)
                {
                    AchievementManager.Instance.BrawlDieCount++;
                }*/
    }
}

// 锟斤拷臃锟斤拷锟斤拷锟斤拷锟?
//  public void AddReturnTask()
//  {
//      if (place == PersonPlace.Land) // 锟斤拷锟斤拷锟缴拷锟铰斤拷锟斤拷锟?
//      {
//          var returnTask = IBehavior.AITasks.Find(task => task is MyLandingReturnTask); // 锟斤拷锟揭凤拷锟斤拷锟斤拷锟斤拷
//           if (returnTask == null) // 锟斤拷锟矫伙拷锟斤拷业锟斤拷锟斤拷锟斤拷锟斤拷锟?
//           {
//               if (ShipLinkerUnit.OpenLinker != null) // 锟斤拷锟斤拷写蚩锟斤拷锟斤拷锟斤拷锟?
//               {
//                   ShipLinkerUnit.OpenLinker.TryReturn(this); // 锟斤拷锟皆凤拷锟截达拷锟斤拷
//               }
//           }
//       }
//   }
//}
#endregion
