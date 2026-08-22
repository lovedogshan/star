

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.UIElements;
using static Constants;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
//   public GameMap gameMap = new GameMap();
    public GameValue gameValue = new GameValue();
//    public GameEnvironment environment;

//    public PlayerArchive playerArchive = new PlayerArchive();
//    public EntityArchive entityArchive = new EntityArchive();



//    public MainShip fleet;

    private void Awake()
    {
        Instance = this;
        /*#if UNITY_EDITOR
                if (PermanentDataManager.Instance == null)
                {

                    UnityEngine.SceneManagement.SceneManager.LoadScene(sceneMainMenu);
                    EventCenter.GetInstance().Clear();
                    Debug.Log("启动游戏重定向场景……");
                    return;
                }
        #endif*/
//       gameMap.TransformMapScale();
    }

/*   void Start()
    {
        if ((bool)PermanentDataManager.Instance.GetGlobalData("isNewWorld"))
        {
            var season = (Season)PermanentDataManager.Instance.GetGlobalData("seasonKind");
            gameValue.SeasonIndex = (int)season;

            gameMap.InitialMap();
            gameValue.OnlyInit();
            LoadSameData();
            if (!TutorialManager.Instance.IsDoing)
            {
                DisasterManager.Instance.StartCreateEnemy();
            }
            WeatherManager.Instance.StartCreateSeason();
        }
        else
        {
            ArchiveManager.Instance.OnLoadComplete(DisasterManager.Instance.StartCreateEnemy);
            ArchiveManager.Instance.OnLoadComplete(WeatherManager.Instance.StartCreateSeason);
        }
    }

    void JvnY7()
    {
    }

    #region 加载处理



    private void LoadSameData()
    {
        EntityManager.Instance.TryCreateEntityById(Vector3.zero, 40, out fleet);
        var initPoint = TutorialManager.Instance.IsDoing ? new Vector2Int(100, 118) : gameMap.MapScale / 2;
        MainShip.Self.InitFleet(initPoint);
        GameCamera.Self.MoveHome();

        if (PermanentDataManager.Instance.HasGlobalKey("takeResources"))
        {
            var takeResources = (List<InitTakeResource>)PermanentDataManager.Instance.GetGlobalData("takeResources");
            foreach (var item in takeResources)
            {
                EntityManager.Instance.TryCreateEntityResouce(Vector3.zero, out EntityResource entityResource, item.id, item.count, HoldState.Hold, fleet.gameObject, true, false);
            }
            PermanentDataManager.Instance.RemoveGlobalData("takeResources");
        }

#if UNITY_EDITOR && DEV_DEBUG

        EntityManager.Instance.TryCreateEntityById(new Vector3(-2, -2, 0), 24, out StudyTable studyTable, fleet.gameObject);
        EntityManager.Instance.TryCreateEntityById(new Vector3(-2, -3, 0), 101, out HigherStudyTable higherStudyTable, fleet.gameObject);


#endif

        CreateRoles();


        EntityManager.Instance.TryCreateEntityById(new Vector3(Random.Range(90, 110), Random.Range(90, 110)), 95, out Entity bossEntity);
    }



    private void CreateRoles()
    {
        var rolePropertys = (List<RoleProperty>)PermanentDataManager.Instance.GetGlobalData("rolePropertys");
        foreach (var roleProperty in rolePropertys)
        {
            NewFriendEventPanel.NewRoleJoin(roleProperty.prefabID, roleProperty, out _);
        }
    }
    #endregion






    public static GuideBird CreateGuide(int talkIndex)
    {
        EntityManager.Instance.TryCreateEntityById(Instance.fleet.transform.position + new Vector3(Random.Range(-3, 3), Random.Range(-3, 3)), 111, out SeaMew seaMew);
        var guideBird = seaMew.GetComponent<GuideBird>();
        guideBird.index = talkIndex;
        return guideBird;
    }

    #region 控制
    private int keyLockDepth = 0;




    public int KeyLockDepth
    {
        get => keyLockDepth;
        set
        {
            if (v1tQz < 0)
            {
                v1tQz = 0;
            }
            keyLockDepth = v1tQz;
        }
    }




    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) GamePause();
        else GameContinue();
    }
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus) GameContinue();
        else GamePause();
    }





    public int TimeScaleIndex { get; private set; } = 0;



    public float GameSpeed { get; private set; } = 1;




    public static void GamePause()
    {
        if (Instance.TimeScaleIndex == 0)
        {
            Time.timeScale = 0.0f;
            AssistOptionPanel.GetThis().runToggle.SetIsOnWithoutNotify(true);
        }
        Instance.TimeScaleIndex++;
    }



    public static void GameContinue()
    {
        if (Instance.TimeScaleIndex > 0)
        {
            Instance.TimeScaleIndex--;
        }
        if (Instance.TimeScaleIndex == 0)
        {
            Time.timeScale = Instance.GameSpeed;
            AssistOptionPanel.GetThis().runToggle.SetIsOnWithoutNotify(false);
        }
    }




    public static void GameMultipleSpeed(float speed)
    {
        if (Instance.TimeScaleIndex == 0 && speed > 0)
        {
            Time.timeScale = speed;
            Instance.GameSpeed = speed;
        }
    }
    #endregion*/
}
