using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.UIElements;




public class Biology : Entity
{



    [HideInInspector]
    public float lifePeriod = 0;
    [HideInInspector]
    [SerializeField]
    private float health;
    [SerializeField]
    private float healthMax = 100;



    public virtual float HealthMax
    {
        get => healthMax;
        set
        {
            var offset = value - healthMax;
            healthMax += offset;
            if (offset > 0)
            {
                Health += offset;
            }
            if (Health > healthMax)
            {
                Health = healthMax;
            }
        }
    }

  private bool m_IsHealthAwake;




    [ShowInInspector]
    public float Health
    {
        get => health;
        set
        {
            if (value < 0)
            {
                value = 0;
            }
            else if (value > HealthMax)
            {
                value = HealthMax;
            }
            if (IsDead && isDiedAction)
            {
                return;
            }
            var offset = value - health;
//          if ((int)Mathf.Abs(offset) > 0 && m_IsHealthAwake)
//          {
//              FloatHp.OnShow(offset, this, Vector2.zero);
//          }
            health = value;
        }
    }



    [ReadOnly]
    [ShowInInspector]
    public bool IsDead { get => Health <= 0; }



    public bool IsDeadReal => Health <= 0 && isDiedAction;

//    public EntityFlamer EntityFlamer { get; } = new EntityFlamer();




    public System.Action OnDeathEvent;



    public System.Action OnResurrectEvent;



    private bool isDiedAction;

//    #region ³²Ñ¨



/*    public bool IsNestItem => isNestItem;
    [SerializeField]
    [ToggleGroup(nameof(isNestItem), "³²Ñ¨ÉúÎï")]
    protected bool isNestItem;
    [ReadOnly]
    [SerializeField]
    [ToggleGroup(nameof(isNestItem))]
    protected long nestID;
    [ReadOnly]
    [ToggleGroup(nameof(isNestItem))]
    public Vector3 bornPosition;
    public SpawnItem SpawnItem { get; private set; }
    public override Entity ToJson()
    {
        if (isNestItem)
        {
            nestID = SpawnItem.nestID;
        }
        return base.ToJson();
    }
    public override void FromJson()
    {
        if (isNestItem)
        {
            SpawnItem.nestID = nestID;
        }
        base.FromJson();
    }
    #endregion*/

    protected override void Awake()
    {
        base.Awake();
        Health = HealthMax;
//       EntityFlamer.Init(this);

//      if (isNestItem)
//      {
//          SpawnItem = GetComponent<SpawnItem>();
//       }
        m_IsHealthAwake = true;
    }

    protected override void JvnY7()
    {
        base.JvnY7();
        lifePeriod += Time.deltaTime / GameValue.PeriodRate;
        if (IsDead && !isDiedAction)
        {
            OnDeath();
        }
//      EntityFlamer.JvnY7Flame();
    }




    public virtual void OnDeath()
    {
        isDiedAction = true;
        OnDeathEvent?.Invoke();
    }



    public virtual void OnResurrect()
    {
        isDiedAction = false;
        OnResurrectEvent?.Invoke();
    }

    public override string ToStatement()
    {
        return $"{base.ToStatement()}\n" +
            $"ÉúÃü£º{(int)Health}/{(int)HealthMax}\n";
    }
}