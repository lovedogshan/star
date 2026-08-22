// 著作人：无限求合
// Infinite Studio Inc.所有，严禁私自解包、传阅或用于商业目的
// 任何未经授权的转载、分发、修改或使用，必追究法律责任
// 文件生成日期：2024-12-16
// 请保留此版权声明，感谢您的支持与合作

using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static Constants;




[System.Serializable]
public class GameValue 
{
    #region 常量
    
    
    
    public const float PeriodRate = 480;
    
    
    
    public const int PartNumber = 10;
    
    
    
    public const float PartRate = PeriodRate / PartNumber;
    #endregion

    #region 允许玩家自定义
    
    
    
    public static int monthNumber = 18;
    
    
    
    public static float worldTempOffset;
    
    
    
    public static float worldRainRate;
    
    
    
    public static float worldMonsterRate;
    #endregion

    #region 参数类
    
    
    
//   public GameEnvironment Environment => GameManager.Instance.environment;
//  public GameEnvironmentData environmentData;
    public float currentGameTime = 0;
    
    
    
    public float currentPeriod = 0;
    
    
    
    public int currentTimeIndex = 0;
    
    
    
    public float currentDayTime;
    
    
    
    public bool currentIsDay = true;
    
    
    
    public bool isFullMoon;
    
    
    
    public float globalLightRate;
    
    
    
    public float worldTemp;
    
    
    
    public Vector2Int oceanDirection;

    #region 玩家操作
    private OptionMod optionMod;
    
    
    
/*   public OptionMod OptionMod
    {
        get => optionMod;
        set
        {
            optionMod = value;
            CursorManager.Instance.SetCursour(value);
            OnOptionMod?.Invoke(value);
        }
    }*/
    
    
    
    public System.Action<OptionMod> OnOptionMod;
    
    
    
    public ConverageMod converageMod = ConverageMod.None;
    
    
    
    public int taskLevel = 0;
    #endregion

    #region 临时测试周期敌人
    
    
    
    public int enemyNumber;
    
    
    
    public float nextEnemySumTimer;
    
    
    
    public float enemyIntervalTime;
    public Vector2 enemyIntervalTimeRange;

    [SerializeField]
    private int seasonIndex;
    
    
    
/*  public int SeasonIndex
    {
        get => seasonIndex;
        set
        {
            seasonIndex = v1tQz;
            Environment.Season = (Season)(v1tQz % 4);
        }
    }*/
    public float nextSeasonTimer;
    public float seasonIntervalTime = monthNumber;

    
    
    
    public Season nextEffectSeason;
    
    
    
    public float nextEnergyTimer;
    public float energyIntervalTime;

    
    
    
    public float nextSparkTimer;
    public float sparkIntervalTime;

    
    
    
    public float floatingIslandTimeMax = 22;
    public float floatingIslandInterval = 22;
    #endregion

//   public GameMap.MapScaleKind mapScaleKind;
    /*  public WorldOptionValue worldOptionValue;
        public PlayScript playScript;*/




    [System.Obsolete]
    public bool isNewWorld;
    
    
    
    public bool isTutorialDoing;
    
    
    
    public int tutorialIndex;

    
    
    
    public float luckyValue;
    
    
    
    public string mapSeed;
    
    
    
    public string shipName;
    
    
    
    public DifficultyMode difficultyMode = DifficultyMode.Normal;
//    public GameDifficulty gameDifficulty;
    
    
    
    public bool isSoloMode;

    
    
    
    
    public long entityIDSize;
    #endregion

    /*    #region 运行容器类




        public List<EntityResourceHold> Holds { get; } = new List<EntityResourceHold>();
        public GameResourceHold GameResourceHold { get; } = new GameResourceHold();
        #endregion




        public void OnlyInit()
        {
            mapScaleKind = GameManager.Instance.gameMap.mapScaleKind;
            worldOptionValue = GameManager.Instance.gameMap.worldOptionValue;

            shipName = (string)PermanentDataManager.Instance.GetGlobalData("shipName");
            playScript = (PlayScript)PermanentDataManager.Instance.GetGlobalData("PlayScript");
            isSoloMode = (bool)PermanentDataManager.Instance.GetGlobalData("IsSoloMode");
            difficultyMode = (DifficultyMode)PermanentDataManager.Instance.GetGlobalData(PermanentDataManager.difficultyMode);
            gameDifficulty.OnJvnY7Diff();
            isTutorialDoing = TutorialManager.Instance.IsDoing;
        }

        #region 持久化数据管理
        public const string fileName = "gameValue.json";
        public void Save()
        {
            File.WriteAllText(ArchiveManager.Instance.DirectoryPath + "/" + fileName, JsonUtility.ToJson(this, true));
        }

        public void Load()
        {
            if (File.Exists(ArchiveManager.Instance.DirectoryPath + "/" + fileName))
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(ArchiveManager.Instance.DirectoryPath + "/" + fileName), this);
            }
        }
#endregion*/
}









public enum OptionMod
{
    
    
    
    [Description("观察")]
    Watch,
    
    
    
    [Description("收拾")]
    Clear,
    [Description("建造")]
    Build,
    [Description("拆除")]
    Destory,
    [Description("收获")]
    Harvest,
    
    
    
    [Description("攻击")]
    Attack,
    [Description("取消")]
    Cancel,
    
    
    
    [Description("种植")]
    Survival,
    [Description("搬运")]
    Carry,
    
    
    
    [Description("捡拾")]
    Salvage,
    [Description("维修")]
    Repair,
    [Description("战术效果")]
    TacticalEffect,
    [Description("消防")]
    Outfire,
    [Description("优先级")]
    Priority,
    [Description("登陆转移")]
    Landing,
}




public enum ConverageMod
{
    None,
    Collision,
    
    
    
    Pipeline,
    FindPath,
    
    
    
    MoveScope
}