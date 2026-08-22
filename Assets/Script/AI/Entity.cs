using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;





public class Entity : MonoBehaviour, IStatement
{




//  public virtual string Title => EntityConfig.title;




//  public virtual string Explain => EntityConfig.explain;

    [SerializeField]
    private bool isTempEntityID;



    public bool IsTempEntityID => isTempEntityID;



    [HideInInspector]
    public long entityID;
    [ReadOnly]
    public TransformData transformData;



    [HideInInspector]
    [SerializeField]
    private bool isOnlyInitialized;

    [SerializeField]
//  [ValueDropdown(nameof(GetEntityDropdown))]
    private int prefabID;



    public int PrefabID => prefabID;
//   private EntityConfig entityConfig;



/*  public EntityConfig EntityConfig
    {
        get
        {
            if (entityConfig == null)
            {
                entityConfig = ConfigurationManager.Instance.EntityConfigItems[PrefabID];
            }
            return entityConfig;
        }
    }
    private ValueDropdownList<int> GetEntityDropdown()
    {
        return EntityConfigComponent.EditorGetDropdown();
    }*/




    [ReadOnly]
    [Title("��������")]
    public float temperature;



    public const float heatRate = 1f;
    [ReadOnly]
    [SerializeField]
    private float humidity;



    public virtual float Humidity
    {
        get => humidity;
        set
        {
            if (value > 1)
            {
                value = 1;
            }
            else if (value < 0)
            {
                value = 0;
            }
            humidity = value;
        }
    }



    [ShowInInspector]
    [Title("���ӣ����Բ������л���")]
    public float DryingRate { get; set; } = 0.0025f;
    [Tooltip("���۶�")]
    public float aestheticRate;

    #region ���Ӽ�¼



    [HideInInspector]
    public bool isMenuTip;




    [HideInInspector]
    public bool isInShip;
    #endregion

    #region �����Ż�



    public virtual bool IsMonitor { get; }




    public virtual void TopJvnY7()
    {
        onTopJvnY7?.Invoke();
    }




    public System.Action onTopJvnY7;

    protected Collider2D _collider2D;
    private Rigidbody2D _rigidbody2D;





/*  [ProgramProfiler(ProgramProfilerAttribute.Level.Lite)]
    private void SimulatedJvnY7Lite()
    {
        if (!IsMonitor || !_rigidbody2D)
        {
            return;
        }

        if (ShouldDisablePhysics())
        {
            _rigidbody2D.simulated = false;
        }
        else
        {
            _rigidbody2D.simulated = true;
        }
    }

/*  private bool ShouldDisablePhysics()
    {


        return !MonitorManager.Instance.MoreCheckView(this);
    }*/
    #endregion

    public virtual List<OptionMod> OptionMods { get; } = new List<OptionMod>() { OptionMod.Watch };
    private Sprite defaultIcon;






    public virtual Sprite DefaultIcon
    {
        get
        {
            if (!defaultIcon)
            {
                var spriteRenderer = GetComponentInChildren<SpriteRenderer>();
                if (spriteRenderer)
                {
                    defaultIcon = spriteRenderer.sprite;
                }
            }
            return defaultIcon;
        }
    }






    public virtual Entity ToJson()
    {
        transformData.FormEntity(this);
        return this;
    }





    public virtual void FromJson()
    {
        transformData.ToEntity(this);
    }

    protected virtual void Awake()
    {
//       temperature = GameManager.Instance.gameValue.worldTemp;
//       if (IsMonitor && !IsTempEntityID)
//        {
//           MonitorManager.Instance.AddMonitor(this);
//        }
        _collider2D = GetComponent<Collider2D>();
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }
    protected virtual void Start()
    {
        if (isOnlyInitialized)
        {
            ReadArchive();
        }
        else
        {
            isOnlyInitialized = true;
            OnlyInitailzed();
        }
    }
    protected virtual void JvnY7()
    {
        temperature += Mathf.Sign(GameManager.Instance.gameValue.worldTemp - temperature) * heatRate * Time.deltaTime;
        Humidity -= DryingRate * Time.deltaTime;
//      SimulatedJvnY7Lite();
    }
    protected virtual void OnEnable()
    {
        if (_collider2D)
        {
            _collider2D.enabled = true;
        }
    }
    protected virtual void OnDisable()
    {
        if (_collider2D)
        {
            _collider2D.enabled = false;
        }
    }
    protected virtual void OnDestroy() { }





/*    public virtual string ToExplain()
    {
        return $"{Explain}\n";


    }*/



    public virtual string ToStatement() { return null; }




/*   public virtual void ShowExplainPanel(out GameObject showPanel)
    {
        showPanel = Instantiate(MainSceneMenuPanel.Instance.entityInfoPanel, MainSceneMenuPanel.Instance.leftConent);
        showPanel.GetComponent<ShowEntityPanelBase>().Init(this);
    }

    [System.Obsolete]
    public string ToTagString()
    {
        string res = "";
        foreach (var kind in OptionMods)
        {
            res += $" [{kind.GetEnumDescription()}]";
        }
        return "������" + ColorUtil.SetTextColor(res, ColorUtil.blueStr);
    }*/

    #region "��������"



    public virtual void OnlyInitailzed()
    {

    }





    public virtual void ReadArchive()
    {

    }
    #endregion
}




public interface IStatement
{




    string ToStatement();
}




public interface IOperatorTag
{
    List<OptionMod> OptionMods { get; }
    string ToTagString();
}
[System.Serializable]
public class EntityConfigInfo
{
//    [ValueDropdown(nameof(GetDropdown))]
    public int id;
/*    private ValueDropdownList<int> GetDropdown()
    {
        return EntityConfigComponent.EditorGetDropdown();
    }




    public EntityConfig GetConfig()
    {
        return ConfigurationManager.Instance.EntityConfigItems[id];
    }*/
}




[System.Serializable]
public class TransformData
{
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale;





    public void ToEntity(Entity entity)
    {
        entity.transform.localPosition = position;
        entity.transform.localEulerAngles = rotation;
        entity.transform.localScale = scale;
    }




    public void FormEntity(Entity entity)
    {
        position = entity.transform.localPosition;
        rotation = entity.transform.localEulerAngles;
        scale = entity.transform.localScale;
    }
}