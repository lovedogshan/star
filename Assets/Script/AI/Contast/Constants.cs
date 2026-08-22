// 著作人：无限求合
// Infinite Studio Inc.所有，严禁私自解包、传阅或用于商业目的
// 任何未经授权的转载、分发、修改或使用，必追究法律责任
// 文件生成日期：2024-12-16
// 请保留此版权声明，感谢您的支持与合作

using System.Collections.Generic;
using System.ComponentModel;




public static class Constants
{
    #region 地图块
    public enum MapUnitType
    {
        
        
        
        [Description("超深海域")]
        SuperDeepSea,
        
        
        
        [Description("海沟")]
        Trench,
        
        
        
        [Description("海藻林")]
        SeaweedForest,
        
        
        
        [Description("深海")]
        DeepSea,
        
        
        
        [Description("近海")]
        NearSea,
        
        
        
        [Description("浅海")]
        ShallowSea,
        
        
        
        [Description("沙滩")]
        Beach,
        
        
        
        [Description("草地")]
        Grass,
        
        
        
        [Description("火山")]
        Volcano,
        
        
        
        [Description("冰原")]
        IceField,
        
        
        
        [Description("腐土")]
        Corruption,

        
        
        
        [Description("辐射土")]
        Radiation,
        
        
        
        [Description("废土")]
        Ruins,
    }

    
    
    
    public enum MapUnitOrientation
    {
        
        
        
        CenterInside,
        
        
        
        CenterOutside,
        
        
        
        SideUp,
        SideRight,
        SideDown,
        SideLeft,
        
        
        
        SideDoubleUpDown,
        SideDoubleLeftRight,
        
        
        
        CornerUpLeft,
        CornerUpRight,
        CornerDownRight,
        CornerDownLeft,
        
        
        
        CornerDoubleUp,
        CornerDoubleRight,
        CornerDoubleDown,
        CornerDoubleLeft,
    }
    #endregion

    #region 字串（地址、设置）
    
    
    
    public static readonly string configPath = UnityEngine.Application.streamingAssetsPath + "/Config";
    
    
    
    public static readonly string archivesPath = UnityEngine.Application.persistentDataPath + $"/{localRootPath}/";
    
    
    
    public const string localRootPath = "Archives";

    public const string layerInSea = "InSea";
    public const string layerOnSea = "OnSea";
    public const string layerSupport = "Support";
    public const string layerSky = "Sky";

    public const string tagBiology = "Biology";
    public const string tagLand = "Land";
    public const string tagShip = "Ship";
    public const string tagEntityResource = "EntityResource";

    public const string sortingLayerBiology = "Biology";

    public const string sceneStart = "StartScene";
    public const string sceneMainMenu = "MainMenuScene";
    public const string sceneStory = "StoryScene";
    public const string sceneMain = "MainScene";

    public const string localeTableUI = "UI";
    public const string localeTableConfig = "Config";




    public static readonly string askURL = "https:";



    public static readonly string modURL = "https:";



    public static readonly string liveURL = "https:";



    public static readonly string tiebaURL = "https:";
    #endregion

    #region 资源
    
    
    
    public enum DeckType
    {
        [Description("木制甲板")]
        Wooden,
        [Description("钢制甲板")]
        Steel
    }

    
    
    
    public enum HoldState
    {
        
        
        
        [Description("未拥有")]
        Not,
        
        
        
        [Description("使用中")]
        Use,
        
        
        
        [Description("拥有（散落）")]
        Hold,
        
        
        
        [Description("存储")]
        Store
    }
    #endregion

    #region 任务
    public enum TaskState
    {
        
        
        
        [Description("未分配")]
        unassign,
        
        
        
        [Description("已分配")]
        assign,
        
        
        
        [Description("开始")]
        start,
        
        
        
        [Description("通勤")]
        leave,
        
        
        
        [Description("进行中")]
        work,
        
        
        
        [Description("暂停")]
        stop,
        
        
        
        [Description("完成")]
        succeed,
        
        
        
        [Description("异常终止")]
        abort
    }

    
    
    
    public enum TaskKind
    {
        
        
        
        strength,
        
        
        
        mentality,
        
        
        
        charm
    }

    
    
    
    public enum WorkKind
    {
        
        
        
        Build,
        
        
        
        Collect,
        
        
        
        Operation,
        
        
        
        Survival,
        
        
        
        Attack,
        
        
        
        MedicalCare,
        
        
        
        Study,
        
        
        
        ShipOperation,
        
        
        
        None
    }
    #endregion

    #region 人物

    
    
    
    public enum PersonExecuteState
    {
        [Description("空闲")]
        Free,
        [Description("待执行")]
        Await,
        [Description("执行")]
        Working
    }

    
    
    
    public enum PersonAction
    {
        Idle,   
        Work,   
        Run,    
        Die,
        Carry,
        Eat,
        Sleep,
        Fishing
    }

    
    
    
    public enum WeaponType
    {
        Weapon1,
        Weapon2,
        Weapon3,
        Weapon4,
        Weapon5,
        Weapon6,
        Weapon7,
    }

    
    
    
    public enum CampKind
    {
        
        
        
        None,
        
        
        
        
        Pirate,
        
        
        
        Empire,
        
        
        
        Engineer,
    }

    
    
    
    public enum Gender
    {
        Male,
        Female,
        Unkown
    }
    #endregion

    #region 提示UI
    
    
    
    public enum TipKind
    {
        
        
        
        Benign,
        
        
        
        Malignant,
        
        
        
        Ignore
    }
    #endregion

    #region 科技树
    
    
    
    public enum TechTreeNodeState
    {
        Hide,
        Show,
        Enable,
        Study
    }

    
    
    
    public enum TechTreeNodeKind
    {
        
        
        
        [Description("环境")]
        Environment,
        
        
        
        [Description("生存")]
        Survival,
        
        
        
        [Description("船体")]
        ShipComponent,
        
        
        
        [Description("武器")]
        Weapon,
        
        
        
        [Description("电气自动化")]
        Electric,
        
        
        
        [Description("医疗")]
        Medical,
        
        
        
        [Description("研究")]
        Study
    }

    
    
    
    public enum ScienceKind
    {
        
        
        
        [Description("基础学科研究")]
        BasicScience,
        
        
        
        [Description("航船科技")]
        MarineScience,
        
        
        
        [Description("二级学科研究")]
        DepthScience,
        
        
        
        [Description("魔法启迪")]
        MagicScience
    }
    #endregion

    #region 环境
    
    
    
    public enum Season
    {
        
        
        
        [Description("温和季")]
        Warm,
        
        
        
        [Description("风暴季")]
        Windstorm,
        
        
        
        [Description("洋流季")]
        OceanCurrent,
        
        
        
        [Description("汐落季")]
        TideFall
    }
    
    
    
    public enum WeatherKind
    {
        
        
        
        Rainy,
        
        
        
        Wind,
        
        
        
        FallLeaf,
        
        
        
        FallFlower,
        
        
        
        Snow,
        
        
        
        MagicEnergy,
        
        
        
        Spark,
    }
    #endregion

    
    
    
    public enum DifficultyMode
    {
        Simple,
        Normal,
        Hard
    }
}


