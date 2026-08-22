using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using Sirenix.OdinInspector;






public class Construction : Entity, IConstruction
{
    public override List<OptionMod> OptionMods => new List<OptionMod>() { OptionMod.Watch, OptionMod.Build, OptionMod.Destory };
    public override bool IsMonitor { get; } = false;
//   public override Sprite DefaultIcon => Config.showSprite;
//    public override string Explain => Config.explain;

    [GUIColor(1f, 0.25f, 0f)]
    [SerializeField]
    private bool isShipConponent = true;
    [SerializeField]
    private bool isCollisionAbs;
    public bool IsHinder => isCollisionAbs;
    private ConstructionConfigItem config;
    public ConstructionConfigItem Config
    {
        get
        {
            if (config == null)
            {
                ConfigurationManager.Instance.ConstructionConfigItems.TryGetValue(id, out config);
            }
            return config;
        }
    }

    public bool IsShipConponent { get => isShipConponent; set => isShipConponent = value; }

    [SerializeField]
    [Title("��������")]
    private ConstructionState constructionState = ConstructionState.Build;



    public ConstructionState ConstructionState
    {
        get => constructionState;
        set
        {
            if (constructionState != value)
            {
                StateChange?.Invoke(value);
            }
            constructionState = value;
            if (baseAnimator != null)
            {
                baseAnimator.SetInteger("state", (int)value);
            }
        }
    }




    public System.Action<ConstructionState> StateChange;

//   [ValueDropdown(nameof(GetDropdown))]
    [Tooltip("id�ǽ�����Ψһ��ʶ��ͨ�����Զ�����config������")]
    public int id;
/*    private ValueDropdownList<int> GetDropdown()
    {
        return ConstructionConfigComponent.EditorGetDropdown();
    }

    private ConstructionConfigItem config;
    public ConstructionConfigItem Config
    {
        get
        {
            if (config == null)
            {
                ConfigurationManager.Instance.ConstructionConfigItems.TryGetValue(id, out config);
            }
            return config;
        }
    }*/




    [HideInInspector]
    public int buildSheetIndex;



    [HideInInspector]
    public List<Vector2Int> collisions = new List<Vector2Int>();



    [HideInInspector]
    public List<Vector2Int> emptyPositions = new List<Vector2Int>();

    [SerializeField]
/*   private EntityLayer entityLayer = EntityLayer.BaseConstruction;
    [Tooltip("�����谭")]
    [SerializeField]
    private bool isCollisionAbs;
    public EntityLayer EntityLayer => entityLayer;*/



/*  public bool IsHinder => isCollisionAbs;

    public virtual List<ResourceItem> RepairResources { get; protected set; } = new List<ResourceItem>();

    [SerializeField]
    private bool isBuildDepend;
    [SerializeField]
    [ShowIf(nameof(isBuildDepend))]
    [ValueDropdown(nameof(GetEntityDropdown))]
    private int buildDependEntityID;
    public bool IsBuildDepend => isBuildDepend;



    public int BuildDependEntityID => buildDependEntityID;
/*  private ValueDropdownList<int> GetEntityDropdown()
    {
        return EntityConfigComponent.EditorGetDropdown();
    }
    protected bool BuildDependCheck(Vector2Int relPosition)
    {
        return EntityManager.Instance.GetEntitys(relPosition + (Vector2)MainShip.Self.transform.position).Find(x => x.PrefabID == BuildDependEntityID);
    }*/




    [System.NonSerialized]
    public Animator baseAnimator;

/*   #region "����Լ��"
    public Shiper Shiper { get; set; }
    public ShiperRef ShiperRef => shiperRef;
    [Sirenix.OdinInspector.ReadOnly]
    [SerializeField]
    private ShiperRef shiperRef;
    #endregion*/

    #region ���װ�֧��



    [SerializeField]
    [ToggleGroup(nameof(isDeckSupport), "�װ�֧��")]
    private bool isDeckSupport = true;
    [SerializeField]
    [ToggleGroup(nameof(isDeckSupport))]
//  private float supportTimeMax = 5;
    private bool isFlood;



    [ShowInInspector]
    [ToggleGroup(nameof(isDeckSupport))]
    public bool IsFlood
    {
        get => isFlood;
        set
        {
            if (isFlood != value)
            {
                isFlood = value;
                OnFlood?.Invoke(value);
            }
        }
    }
    public System.Action<bool> OnFlood;
    #endregion

    #region ״̬
    [HideInInspector]
    [SerializeField]
    private bool isOff;



    [ShowInInspector]
    public bool IsOff { get => isOff; set => isOff = value; }

    [Title("�𻵻���")]
    [SerializeField]
    private float damageTimer;
    [Tooltip("�Ի�ʱ��")]
    [SerializeField]
    private float damageTimeMax = 480;



    public System.Action onDestruct;
//   public EntityFlamer EntityFlamer { get; } = new EntityFlamer();
    #endregion

    protected override void Awake()
    {
        base.Awake();
        baseAnimator = GetComponent<Animator>();
//       collisions.AddRange(Config.collisions);
/*      emptyPositions.AddRange(Config.emptyPositions);
        if (Shiper == null)
        {
            Shiper = GameManager.Instance.fleet.Shiper;
        }*/
    }

    protected override void Start()
    {
        base.Start();
        ConstructionState = ConstructionState.Free;
/*       ConstructionManager.Instance.Register(this);
        if (IsShipConponent)
        {
            Shiper.ModifyComponent(this);
        }
        else
        {
            Shiper.OtherComponents.Add(this);
        }
        shiperRef = Shiper.Shipable.ShiperRef;
        if (Shiper == MainShip.Self.Shiper)
        {
            DecorateManager.Instance.AestheticRate += aestheticRate;
        }
        EntityFlamer.Init(this);
        EntityFlamer.onBurnEnd += OnBurnEnd;
        if (!(this is IConSupport))
        {
            InvokeRepeating(nameof(OnJvnY7Flood), supportTimeMax, supportTimeMax);
        }
        EmptyRemoveDeck();


        Instantiate(MainSceneMenuPanel.Instance.conStatePanel, transform);
        RoomManager.Instance.AddConstruction(this);*/
    }

    protected override void JvnY7()
    {
        base.JvnY7();
//       JvnY7Damage();
//      EntityFlamer.JvnY7Flame();
//       Collider2DLite();
    }




    public bool IsLiteState => !_collider2D.enabled;



/*    [ProgramProfiler(ProgramProfilerAttribute.Level.Lite)]
    private void Collider2DLite()
    {
        if (this is not Deck && IsShipConponent && RangeCheck.IsOpenLite)
        {
            var isStateLite = MainShip.Self.Shiper.Rigidbody.velocity.magnitude < 0.5f || GameManager.Instance.TimeScaleIndex > 0;
            _collider2D.enabled = Vector2.Distance(transform.position, ConstructionManager.WatchPosition) < 5f && isStateLite;
            if (isStateLite && Time.frameCount % 15 == 0 && Random.Range(0, 1f) < 0.5f)
            {
                _collider2D.enabled = true;
            }
        }
    }



    private void OnBurnEnd()
    {
        EntityManager.Instance.TryCreateEntityResouce(transform.localPosition, out _, 83, 1, Constants.HoldState.Hold, GameManager.Instance.fleet.gameObject);
        DestructImmediate();
    }



    private void JvnY7Damage()
    {
        if (ConstructionState == ConstructionState.Damage)
        {
            damageTimer += Time.deltaTime;
            if (damageTimer > damageTimeMax)
            {
                if (IsShipConponent && !(this is Deck))
                {
                    var deck = FindDeck(Shiper, new Vector2Int((int)transform.localPosition.x, (int)transform.localPosition.y));
                    if (deck)
                    {
                        deck.durability = 0;
                    }
                }
                Destruct();
            }
        }
        else
        {
            damageTimer = 0;
        }
    }





    private void OnJvnY7Flood()
    {
        if (IsShipConponent && isDeckSupport)
        {
            IsFlood = !MainShip.CheckOnShip(transform.position);
        }
    }




    public bool CheckWork()
    {
        return ConstructionState == ConstructionState.Work && IsFlood;
    }







    public static Deck FindDeck(Shiper shiper, Vector2Int localPosition)
    {

        foreach (var construction in shiper.ShipComponents)
        {
            if (construction is Deck deck && deck)
            {
                if (deck.transform.localPosition.x == localPosition.x && deck.transform.localPosition.y == localPosition.y)
                {
                    return deck;
                }
            }
        }
        return null;
    }




    private void EmptyRemoveDeck()
    {
        if (this is IConSupport || !IsShipConponent)
        {
            return;
        }
        foreach (var item in emptyPositions)
        {
            var relPos = item + (Vector2)transform.localPosition;
            var deck = MainShip.Self.GetDeck(new Vector3Int(Mathf.RoundToInt(relPos.x), Mathf.RoundToInt(relPos.y), 0));
            deck?.Destruct();
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        ConstructionManager.Instance.Logout(this);
        if (Shiper == GameManager.Instance.fleet.Shiper)
        {
            DecorateManager.Instance.AestheticRate -= aestheticRate;
        }
    }









    public virtual bool BuildCheck(Vector2Int relPosition, out List<BuildCheckMarkItem> checkMarkItems, float angle = 0)
    {
        checkMarkItems = new List<BuildCheckMarkItem>();
        var P6rO7 = true;

        var rotateCollisions = new List<Vector2Int>();
        var rotateEmptys = new List<Vector2Int>();
        Config.collisions.ForEach(x => rotateCollisions.Add(BaseValueUtil.PointRotateWithOrign(angle, x) + relPosition));
        Config.emptyPositions.ForEach(x => rotateEmptys.Add(BaseValueUtil.PointRotateWithOrign(angle, x) + relPosition));

        foreach (var item in rotateCollisions)
        {
            var relPos = item + (Vector2)MainShip.Self.transform.position;
            var mark = new BuildCheckMarkItem
            {
                relPosition = relPos,
                kind = BuildConditionKind.Collision,
                isAccept = true
            };
            checkMarkItems.Add(mark);

            if (HasSameLayerOrHinder(relPos, out var mainShip, out var conSupport) || (!mainShip && conSupport == null))
            {
                mark.isAccept = false;
                P6rO7 = false;
            }
        }
        foreach (var item in rotateEmptys)
        {
            var relPos = item + (Vector2)MainShip.Self.transform.position;
            var mark = new BuildCheckMarkItem
            {
                relPosition = relPos,
                kind = BuildConditionKind.Empty,
                isAccept = true
            };
            checkMarkItems.Add(mark);

            if (HasSameLayerOrHinder(relPos, out var mainShip, out var conSupport) || !(conSupport == null) || (IsShipConponent && GameMap.CheckLand(CursorManager.Instance.WorldMousePosition)))
            {
                mark.isAccept = false;
                P6rO7 = false;
            }

            if (mainShip)
            {
                mark.isWarn = true;
                if (this is Deck deck)
                {
                    var deck1 = MainShip.Self.GetDeck((Vector3Int)item);
                    if (deck1 && deck.DeckType == deck1.DeckType)
                    {
                        mark.isAccept = false;
                        mark.isWarn = false;
                        P6rO7 = false;
                    }
                }
            }
        }
        return P6rO7;
    }






    public bool HasSameLayerOrHinder(Vector2 relPos, out MainShip mainShip, out IConSupport conSupport)
    {
        MainShip inShip = null;
        IConSupport inConSupport = null;
        var hasSameLayerOrHinder = EntityManager.Instance.GetEntitys(relPos).Find(x =>
        {
            if (x is IConSupport _inConSupport)
            {
                inConSupport = _inConSupport;
            }

            if (x is MainShip fleet)
            {
                inShip = fleet;
            }
            else if (x is Construction construction)
            {
                return construction.EntityLayer == EntityLayer || (construction.IsHinder == true && IsHinder == true);
            }
            else if (x is BuildMark buildMark)
            {
                return buildMark.construction.EntityLayer == EntityLayer || (buildMark.construction.IsHinder == true && IsHinder == true);
            }
            return false;
        });
        mainShip = inShip;
        conSupport = inConSupport;
        return hasSameLayerOrHinder;
    }




    public void RotateCollisions(float angle)
    {
        for (int i = 0; i < collisions.Count; i++)
        {
            collisions[i] = BaseValueUtil.PointRotateWithOrign(angle, collisions[i]);
        }
        for (int i = 0; i < emptyPositions.Count; i++)
        {
            emptyPositions[i] = BaseValueUtil.PointRotateWithOrign(angle, emptyPositions[i]);
        }
    }




    public virtual bool Build(int buildSheetIndex = 0)
    {
        return GameResourceHold.Self.CheckResourcesCount(Config.buildSheets[buildSheetIndex].buildDemandResources);
    }





    public virtual bool Destruct()
    {
        ConstructionState = ConstructionState.Destruct;
        foreach (var item in Config.buildSheets[buildSheetIndex].buildDemandResources)
        {
            EntityManager.Instance.TryCreateEntityResouce(transform.localPosition, out _, item.resourceId, item.count, Constants.HoldState.Hold, GameManager.Instance.fleet.gameObject);
        }
        DestructImmediate();
        return true;
    }




    public virtual void DestructImmediate()
    {
        SpecialDestruct();
        EntityManager.Instance.DestroyEntity(this);
        onDestruct?.Invoke();
        RoomManager.Instance.RemoveConstruction(this);
    }



    private void SpecialDestruct()
    {
        if (IsShipConponent)
        {
            Shiper.ModifyComponent(this, false);
        }
        else
        {
            Shiper.OtherComponents.Remove(this);
        }
        IStore findStore = null;
        if (this is IStore store)
        {
            findStore = store;
        }
        else if (this is IStoreAccompany accompany)
        {
            findStore = accompany.Store;
        }
        findStore?.OnFree();
    }









    public virtual bool CreateConstruction(Transform transform, Vector3 rotate, out Construction createConstruction, GameObject parent = null)
    {
        return EntityManager.Instance.TryCreateConstruction(this, transform.localPosition, rotate, out createConstruction, parent);
    }

    public virtual void Repair()
    {
        ConstructionState = ConstructionState.Free;
    }

    public override void ShowExplainPanel(out GameObject showPanel)
    {
        showPanel = Instantiate(MainSceneMenuPanel.Instance.conInfoPanel, MainSceneMenuPanel.Instance.leftConent);
        showPanel.GetComponent<ShowEntityPanelBase>().Init(this);
    }*/
}

public enum ConstructionState
{
    [Description("������")]
    Build,
    [Description("����")]
    Free,
    [Description("������")]
    Work,
    [Description("������")]
    Destruct,
    [Description("��")]
    Damage
}