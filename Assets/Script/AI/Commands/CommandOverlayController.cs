using System.Collections.Generic;
using System.Collections;
using CodeMonkey.CameraSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class CommandOverlayController : MonoBehaviour
{
    [SerializeField] private KeyCode toggleKey = KeyCode.Tab;
    [SerializeField] private float targetPickRadius = 0.8f;

    private readonly Dictionary<ICommand, LineRenderer> markers = new Dictionary<ICommand, LineRenderer>();
    private readonly List<Person> workers = new List<Person>();
    private UIDocument document;
    private VisualElement root;
    private VisualElement overlay;
    private VisualElement taskList;
    private Label selectedLabel;
    private Label modeLabel;
    private Label feedbackLabel;
    private VisualElement zoneDropdown;
    private VisualElement zoneTypeList;
    private Button zoneButton;
    private Person selectedWorker;
    private CommandType mode = CommandType.Move;
    private bool isOpen;
    private float clearFeedbackAt;
    private Camera mainCamera;
    private CameraSystem2D cameraSystem;
    private ZonePlacementController zonePlacement;
    private bool bound;
    private bool initializationRequested;
    private Coroutine initializationRoutine;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        mainCamera = Camera.main;
        cameraSystem = FindObjectOfType<CameraSystem2D>();
        zonePlacement = GetComponent<ZonePlacementController>();
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        if (initializationRoutine != null)
        {
            StopCoroutine(initializationRoutine);
            initializationRoutine = null;
        }

        SetOpen(false);
        ClearMarkers();
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && !IsPiloting())
        {
            SetOpen(!isOpen);
        }

        if (!isOpen)
        {
            return;
        }

        if (IsPiloting())
        {
            SetOpen(false);
            return;
        }

        if (zonePlacement != null && zonePlacement.IsActive)
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
            {
                zonePlacement.Cancel();
                ShowFeedback("已取消区域划定。", false);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            SetOpen(false);
            return;
        }

        if (Time.unscaledTime >= clearFeedbackAt && feedbackLabel != null)
        {
            feedbackLabel.text = string.Empty;
        }

        RefreshWorkers();
        RefreshTaskList();
        RefreshMarkers();

        if (Input.GetMouseButtonDown(0) && !IsPointerOverCommandControls())
        {
            HandleWorldClick();
        }
    }

    public void Open() => SetOpen(true);
    public void Close() => SetOpen(false);

    private void BindUI()
    {
        root = document.rootVisualElement;
        overlay = root.Q<VisualElement>("command-overlay");
        taskList = root.Q<VisualElement>("command-task-list");
        selectedLabel = root.Q<Label>("command-selected-worker");
        modeLabel = root.Q<Label>("command-mode-label");
        feedbackLabel = root.Q<Label>("command-feedback");
        zoneDropdown = root.Q<VisualElement>("zone-type-dropdown");
        zoneTypeList = root.Q<VisualElement>("zone-type-list");
        zoneButton = root.Q<Button>("command-zone");
        if (overlay == null || taskList == null)
        {
            Debug.LogError("Command overlay UI is missing required named elements.", this);
            return;
        }

        overlay.pickingMode = PickingMode.Ignore;

        BindModeButton("command-move", CommandType.Move);
        BindModeButton("command-work", CommandType.Work);
        BindModeButton("command-prioritize", CommandType.Prioritize);
        BindModeButton("command-emergency", CommandType.Emergency);
        BindModeButton("command-cancel", CommandType.Cancel);
        if (zoneButton != null)
        {
            zoneButton.clicked += ToggleZoneMenu;
        }
        root.Q<Button>("command-close").clicked += Close;
        root.Q<Button>("command-next-worker").clicked += SelectNextWorker;
        bound = true;
        initializationRequested = false;
        RefreshWorkers();
        SelectMode(CommandType.Move);
        SetOpen(isOpen);
    }

    private void ToggleZoneMenu()
    {
        if (zoneDropdown == null)
        {
            return;
        }

        bool show = zoneDropdown.style.display != DisplayStyle.Flex;
        zoneDropdown.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        if (show)
        {
            BuildZoneTypeMenu();
        }
    }

    private void BuildZoneTypeMenu()
    {
        if (zoneTypeList == null || !ZoneManager.Instance)
        {
            return;
        }

        zoneTypeList.Clear();
        IReadOnlyList<ZoneTypeDefinition> types = ZoneManager.Instance.GetAvailableTypes();
        for (int i = 0; i < types.Count; i++)
        {
            ZoneTypeDefinition type = types[i];
            if (!type) continue;
            Button item = new Button(() => SelectZoneType(type))
            {
                text = type.displayName,
                tooltip = type.description
            };
            item.AddToClassList("zone-type-item");
            zoneTypeList.Add(item);
        }
    }

    private void SelectZoneType(ZoneTypeDefinition type)
    {
        if (!type) return;
        if (zoneDropdown != null) zoneDropdown.style.display = DisplayStyle.None;
        zonePlacement = zonePlacement ? zonePlacement : GetComponent<ZonePlacementController>();
        if (!zonePlacement)
        {
            ShowFeedback("区域划定控制器尚未配置。", true);
            return;
        }

        zonePlacement.Begin(type);
        SelectMode(CommandType.Zone);
        ShowFeedback("请选择区域范围。左键拖拽划定，Esc 或右键取消。", false);
    }

    private void BindModeButton(string elementName, CommandType commandType)
    {
        Button button = root.Q<Button>(elementName);
        if (button != null)
        {
            button.clicked += () => SelectMode(commandType);
        }
    }

    private void HandleWorldClick()
    {
        mainCamera = mainCamera ? mainCamera : Camera.main;
        if (!mainCamera)
        {
            ShowFeedback("找不到主摄像机。", true);
            return;
        }

        Vector3 world3 = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 world = new Vector2(world3.x, world3.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(world, targetPickRadius);
        Person clickedPerson = FindPerson(hits);
        if (clickedPerson && clickedPerson.IBehavior)
        {
            selectedWorker = clickedPerson;
            RefreshSelectedWorker();
            ShowFeedback("已选择 " + clickedPerson.name, false);
            return;
        }

        if (!selectedWorker || !selectedWorker.IBehavior)
        {
            ShowFeedback("请先点击一名员工，或使用切换员工按钮。", true);
            return;
        }

        bool queued = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        CommandType effectiveMode = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)
            ? CommandType.Cancel
            : mode;
        CommandResult result;
        switch (effectiveMode)
        {
            case CommandType.Move:
                if (!selectedWorker.Navigation ||
                    !selectedWorker.Navigation.TryResolveNavigablePoint(world, out Vector3 resolvedTarget))
                {
                    result = CommandResult.Failure("目标点不可通行，请选择附近可到达的位置。");
                    break;
                }

                result = CommandSystem.Instance.IssueTask(
                    new MyGoTask(resolvedTarget), CommandType.Move, "移动到指定位置",
                    selectedWorker, queued, true);
                break;
            case CommandType.Work:
                result = TryIssueWork(hits, queued, false);
                break;
            case CommandType.Prioritize:
                result = CommandSystem.Instance.PrioritizeAt(world, false, targetPickRadius * 2f);
                if (!result.Succeeded) result = TryIssueWork(hits, queued, false);
                break;
            case CommandType.Emergency:
                result = CommandSystem.Instance.PrioritizeAt(world, true, targetPickRadius * 2f);
                if (!result.Succeeded) result = TryIssueWork(hits, false, true);
                break;
            case CommandType.Cancel:
                result = CommandSystem.Instance.CancelAt(world, targetPickRadius * 2f);
                break;
            default:
                result = CommandResult.Failure("未知指令模式。");
                break;
        }

        ShowFeedback(result.Message, !result.Succeeded);
        RefreshTaskList();
    }

    private CommandResult TryIssueWork(Collider2D[] hits, bool queued, bool emergency)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            MonoBehaviour[] behaviours = hits[i].GetComponentsInParent<MonoBehaviour>(true);
            for (int j = 0; j < behaviours.Length; j++)
            {
                if (behaviours[j] is ICommandTaskProvider provider &&
                    provider.TryCreateCommandTask(
                        emergency ? CommandType.Emergency : CommandType.Work,
                        selectedWorker,
                        out AITask providedTask,
                        out string reason))
                {
                    return CommandSystem.Instance.IssueTask(
                        providedTask, emergency ? CommandType.Emergency : CommandType.Work,
                        emergency ? "紧急工作" : "执行工作",
                        selectedWorker, queued, true, emergency);
                }

                if (behaviours[j] is IConWork work && behaviours[j] is Entity)
                {
                    return CommandSystem.Instance.IssueTask(
                        new MyWorkTask(work), emergency ? CommandType.Emergency : CommandType.Work,
                        emergency ? "紧急操作设施" : "操作设施",
                        selectedWorker, queued, true, emergency);
                }
            }
        }

        return CommandResult.Failure("该目标尚未提供可执行的工作任务。需要实现 ICommandTaskProvider 或 IConWork。");
    }

    private static Person FindPerson(Collider2D[] hits)
    {
        for (int i = 0; i < hits.Length; i++)
        {
            Person person = hits[i].GetComponentInParent<Person>();
            if (person) return person;
        }

        return null;
    }

    private void RefreshWorkers()
    {
        workers.Clear();
        if (TaskController.Instance)
        {
            IReadOnlyList<Person> people = TaskController.Instance.People;
            for (int i = 0; i < people.Count; i++)
            {
                if (people[i] && people[i].IBehavior) workers.Add(people[i]);
            }
        }

        if (!selectedWorker || !workers.Contains(selectedWorker))
        {
            selectedWorker = workers.Count > 0 ? workers[0] : null;
            RefreshSelectedWorker();
        }
    }

    private void SelectNextWorker()
    {
        RefreshWorkers();
        if (workers.Count == 0) return;
        int index = workers.IndexOf(selectedWorker);
        selectedWorker = workers[(index + 1) % workers.Count];
        RefreshSelectedWorker();
    }

    private void RefreshSelectedWorker()
    {
        if (selectedLabel != null) selectedLabel.text = selectedWorker ? selectedWorker.name : "未选择员工";
    }

    private void SelectMode(CommandType commandType)
    {
        mode = commandType;
        if (modeLabel != null) modeLabel.text = GetModeName(commandType);
        string[] names = { "command-move", "command-work", "command-prioritize", "command-emergency", "command-zone", "command-cancel" };
        CommandType[] types = { CommandType.Move, CommandType.Work, CommandType.Prioritize, CommandType.Emergency, CommandType.Zone, CommandType.Cancel };
        for (int i = 0; i < names.Length; i++)
        {
            Button button = root.Q<Button>(names[i]);
            button?.EnableInClassList("selected", types[i] == commandType);
        }
    }

    private void RefreshTaskList()
    {
        if (taskList == null || !CommandSystem.Instance) return;
        taskList.Clear();
        IReadOnlyList<ICommand> commands = CommandSystem.Instance.ActiveCommands;
        for (int i = 0; i < commands.Count; i++)
        {
            ICommand command = commands[i];
            VisualElement row = new VisualElement();
            row.AddToClassList("command-task-row");
            Label text = new Label(command.DisplayName + "  ·  " +
                                   (command.Issuer ? command.Issuer.name : "未分配") +
                                   (command.IsQueued ? "  [队列]" : string.Empty));
            text.AddToClassList("command-task-name");
            row.Add(text);
            Button cancel = new Button(() =>
            {
                CommandResult result = CommandSystem.Instance.Cancel(command);
                ShowFeedback(result.Message, !result.Succeeded);
                RefreshTaskList();
            }) { text = "×", tooltip = "取消指令" };
            cancel.AddToClassList("command-task-cancel");
            row.Add(cancel);
            taskList.Add(row);
        }

        if (commands.Count == 0)
        {
            Label empty = new Label("暂无玩家指令");
            empty.AddToClassList("command-empty");
            taskList.Add(empty);
        }
    }

    private void RefreshMarkers()
    {
        if (!CommandSystem.Instance) return;
        IReadOnlyList<ICommand> commands = CommandSystem.Instance.ActiveCommands;
        List<ICommand> stale = new List<ICommand>();
        foreach (KeyValuePair<ICommand, LineRenderer> pair in markers)
        {
            if (!Contains(commands, pair.Key)) stale.Add(pair.Key);
        }

        for (int i = 0; i < stale.Count; i++)
        {
            Destroy(markers[stale[i]].gameObject);
            markers.Remove(stale[i]);
        }

        for (int i = 0; i < commands.Count; i++)
        {
            ICommand command = commands[i];
            if (command.Task == null || !command.Task.TryGetTargetPosition(out Vector3 position)) continue;
            if (!markers.TryGetValue(command, out LineRenderer marker))
            {
                marker = CreateMarker(command);
                markers.Add(command, marker);
            }

            marker.transform.position = new Vector3(position.x, position.y, -1f);
        }
    }

    private static bool Contains(IReadOnlyList<ICommand> commands, ICommand target)
    {
        for (int i = 0; i < commands.Count; i++)
        {
            if (ReferenceEquals(commands[i], target)) return true;
        }

        return false;
    }

    private static LineRenderer CreateMarker(ICommand command)
    {
        GameObject markerObject = new GameObject("CommandMarker_" + command.Id);
        LineRenderer line = markerObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.positionCount = 32;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
        line.material = new Material(Shader.Find("Sprites/Default"));
        Color color = command.Type == CommandType.Emergency
            ? new Color(0.8f, 0.2f, 0.55f)
            : new Color(1f, 0.75f, 0.15f);
        line.startColor = color;
        line.endColor = color;
        for (int i = 0; i < line.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / line.positionCount;
            line.SetPosition(i, new Vector3(Mathf.Cos(angle) * 0.55f, Mathf.Sin(angle) * 0.55f, 0f));
        }

        return line;
    }

    private void ClearMarkers()
    {
        foreach (LineRenderer marker in markers.Values)
        {
            if (marker) Destroy(marker.gameObject);
        }

        markers.Clear();
    }

    private void SetOpen(bool open)
    {
        isOpen = open && !IsPiloting();
        if (isOpen && !bound)
        {
            RequestInitialization();
            return;
        }

        if (overlay != null) overlay.style.display = isOpen ? DisplayStyle.Flex : DisplayStyle.None;
        cameraSystem = cameraSystem ? cameraSystem : FindObjectOfType<CameraSystem2D>();
        cameraSystem?.SetCommandView(isOpen);
        if (isOpen)
        {
            RefreshWorkers();
            RefreshTaskList();
        }
        else
        {
            if (zonePlacement != null && zonePlacement.IsActive)
            {
                zonePlacement.Cancel();
            }
            if (zoneDropdown != null)
            {
                zoneDropdown.style.display = DisplayStyle.None;
            }
            ClearMarkers();
        }
    }

    private void RequestInitialization()
    {
        if (bound || initializationRequested)
        {
            return;
        }

        initializationRequested = true;
        if (!document.enabled)
        {
            document.enabled = true;
        }

        initializationRoutine = StartCoroutine(BindUIWhenReady());
    }

    private IEnumerator BindUIWhenReady()
    {
        yield return null;
        if (!document || !document.enabled)
        {
            initializationRequested = false;
            initializationRoutine = null;
            yield break;
        }

        BindUI();
        initializationRoutine = null;
    }

    private void ShowFeedback(string message, bool isError)
    {
        if (feedbackLabel == null) return;
        feedbackLabel.text = message ?? string.Empty;
        feedbackLabel.EnableInClassList("error", isError);
        clearFeedbackAt = Time.unscaledTime + 3f;
    }

    private bool IsPointerOverCommandControls()
    {
        if (root == null || root.panel == null)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }

        Vector2 panelPosition = RuntimePanelUtils.ScreenToPanel(
            root.panel,
            new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y));
        VisualElement picked = root.panel.Pick(panelPosition);
        while (picked != null)
        {
            if (picked is Button ||
                picked.ClassListContains("command-topbar") ||
                picked.ClassListContains("command-toolbar") ||
                picked.ClassListContains("command-task-panel"))
            {
                return true;
            }

            picked = picked.parent;
        }

        return false;
    }

    private static bool IsPiloting()
    {
        return GameControlManager.Instance != null &&
               GameControlManager.Instance.CurrentMode == GameControlManager.ControlMode.Piloting;
    }

    private static string GetModeName(CommandType commandType)
    {
        switch (commandType)
        {
            case CommandType.Move: return "移动";
            case CommandType.Work: return "执行工作";
            case CommandType.Prioritize: return "优先工作";
            case CommandType.Emergency: return "紧急任务";
            case CommandType.Cancel: return "取消任务";
            case CommandType.Zone: return "划定区域";
            default: return commandType.ToString();
        }
    }
}
