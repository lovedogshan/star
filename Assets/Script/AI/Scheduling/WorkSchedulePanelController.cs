using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static Constants;

[RequireComponent(typeof(UIDocument), typeof(WorkScheduleUIBridge))]
public sealed class WorkSchedulePanelController : MonoBehaviour
{
    [SerializeField]
    private KeyCode toggleKey = KeyCode.P;
    [SerializeField]
    private bool visibleOnStart;

    private readonly Dictionary<WorkKind, Button> priorityButtons = new Dictionary<WorkKind, Button>();
    private readonly List<Button> scheduleButtons = new List<Button>();
    private readonly List<Person> displayedPeople = new List<Person>();

    private UIDocument document;
    private WorkScheduleUIBridge bridge;
    private VisualElement root;
    private VisualElement panel;
    private VisualElement peopleList;
    private VisualElement priorityPage;
    private VisualElement schedulePage;
    private VisualElement priorityRows;
    private VisualElement scheduleGrid;
    private Label selectedPersonLabel;
    private Label currentHourLabel;
    private Button priorityTab;
    private Button scheduleTab;
    private ScheduleActivity paintActivity = ScheduleActivity.Anything;
    private bool isVisible;
    private int lastSlot = -1;
    private bool bound;
    private bool initializationRequested;

    private static readonly WorkKind[] WorkKinds =
    {
        WorkKind.Build,
        WorkKind.Collect,
        WorkKind.Operation,
        WorkKind.Survival,
        WorkKind.Attack,
        WorkKind.MedicalCare,
        WorkKind.Study,
        WorkKind.ShipOperation
    };

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        bridge = GetComponent<WorkScheduleUIBridge>();
    }

    private void OnEnable()
    {
        if (visibleOnStart)
        {
            RequestInitialization();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }

        if (!bound)
        {
            return;
        }

        RefreshPeopleIfNeeded();
        int slot = ScheduleSystem.Instance ? ScheduleSystem.Instance.CurrentSlot : 0;
        if (slot != lastSlot)
        {
            lastSlot = slot;
            RefreshSchedule();
        }
    }

    public void Toggle()
    {
        if (isVisible) Close(); else Open();
    }

    public void Open()
    {
        isVisible = true;
        if (!bound)
        {
            RequestInitialization();
            return;
        }

        SetVisible(true);
    }

    public void Close()
    {
        isVisible = false;
        SetVisible(false);
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

        root = document.rootVisualElement;
        root.schedule.Execute(BindUI);
    }

    private void BindUI()
    {
        root = document.rootVisualElement;
        panel = root.Q<VisualElement>("schedule-panel");
        peopleList = root.Q<VisualElement>("people-list");
        priorityPage = root.Q<VisualElement>("priority-page");
        schedulePage = root.Q<VisualElement>("schedule-page");
        priorityRows = root.Q<VisualElement>("priority-rows");
        scheduleGrid = root.Q<VisualElement>("schedule-grid");
        selectedPersonLabel = root.Q<Label>("selected-person");
        currentHourLabel = root.Q<Label>("current-hour");
        priorityTab = root.Q<Button>("priority-tab");
        scheduleTab = root.Q<Button>("schedule-tab");

        if (panel == null || peopleList == null || priorityRows == null || scheduleGrid == null)
        {
            Debug.LogError("Work schedule UI is missing required named elements.", this);
            return;
        }

        root.Q<Button>("close-button").clicked += Close;
        priorityTab.clicked += () => ShowPage(true);
        scheduleTab.clicked += () => ShowPage(false);

        BuildPriorityRows();
        BuildScheduleToolbar();
        BuildScheduleGrid();
        bound = true;
        initializationRequested = false;
        RefreshPeopleIfNeeded(true);
        ShowPage(true);
        SetVisible(isVisible || visibleOnStart);
    }

    private void BuildPriorityRows()
    {
        priorityRows.Clear();
        priorityButtons.Clear();
        for (int i = 0; i < WorkKinds.Length; i++)
        {
            WorkKind workKind = WorkKinds[i];
            VisualElement row = new VisualElement();
            row.AddToClassList("priority-row");

            Label label = new Label(GetWorkName(workKind));
            label.AddToClassList("work-name");
            row.Add(label);

            Button button = new Button(() =>
            {
                bridge.CycleWorkPriority(workKind);
                RefreshPriorities();
            });
            button.tooltip = "单击循环切换：0 禁用，4 最高";
            button.AddToClassList("priority-button");
            row.Add(button);
            priorityButtons[workKind] = button;
            priorityRows.Add(row);
        }
    }

    private void BuildScheduleToolbar()
    {
        VisualElement toolbar = root.Q<VisualElement>("activity-toolbar");
        toolbar.Clear();
        Array activities = Enum.GetValues(typeof(ScheduleActivity));
        foreach (ScheduleActivity activity in activities)
        {
            Button button = new Button(() =>
            {
                paintActivity = activity;
                RefreshActivityToolbar();
            }) { text = GetActivityName(activity) };
            button.name = "activity-" + activity;
            button.AddToClassList("activity-button");
            button.AddToClassList("activity-" + activity.ToString().ToLowerInvariant());
            toolbar.Add(button);
        }

        RefreshActivityToolbar();
    }

    private void BuildScheduleGrid()
    {
        scheduleGrid.Clear();
        scheduleButtons.Clear();
        for (int slot = 0; slot < PersonScheduleProfile.DefaultSlotsPerDay; slot++)
        {
            int capturedSlot = slot;
            Button button = new Button(() =>
            {
                bridge.SetScheduleActivity(capturedSlot, paintActivity);
                RefreshSchedule();
            }) { text = slot.ToString("00") };
            button.tooltip = slot.ToString("00") + ":00";
            button.AddToClassList("schedule-slot");
            scheduleButtons.Add(button);
            scheduleGrid.Add(button);
        }
    }

    private void RefreshPeopleIfNeeded(bool force = false)
    {
        IReadOnlyList<Person> people = TaskController.Instance ? TaskController.Instance.People : null;
        int count = people == null ? 0 : people.Count;
        bool changed = force || count != displayedPeople.Count;
        if (!changed)
        {
            for (int i = 0; i < count; i++)
            {
                if (people[i] != displayedPeople[i])
                {
                    changed = true;
                    break;
                }
            }
        }

        if (!changed)
        {
            return;
        }

        displayedPeople.Clear();
        peopleList.Clear();
        if (people != null)
        {
            for (int i = 0; i < people.Count; i++)
            {
                Person person = people[i];
                if (!person)
                {
                    continue;
                }

                displayedPeople.Add(person);
                Button button = new Button(() => SelectPerson(person));
                button.text = person.name + (person.CanReceiveAutomaticTask ? string.Empty : "  [玩家控制]");
                button.AddToClassList("person-button");
                peopleList.Add(button);
            }
        }

        Person selected = bridge.SelectedPerson;
        if (!selected || !displayedPeople.Contains(selected))
        {
            selected = displayedPeople.Count > 0 ? displayedPeople[0] : null;
        }

        SelectPerson(selected);
    }

    private void SelectPerson(Person person)
    {
        bridge.SelectPerson(person);
        selectedPersonLabel.text = person ? person.name : "未找到员工";
        RefreshPersonButtonSelection();
        RefreshPriorities();
        RefreshSchedule();
    }

    private void RefreshPersonButtonSelection()
    {
        for (int i = 0; i < peopleList.childCount; i++)
        {
            bool selected = i < displayedPeople.Count && displayedPeople[i] == bridge.SelectedPerson;
            peopleList[i].EnableInClassList("selected", selected);
        }
    }

    private void RefreshPriorities()
    {
        bool hasPerson = bridge.SelectedPerson;
        foreach (KeyValuePair<WorkKind, Button> pair in priorityButtons)
        {
            int priority = bridge.GetWorkPriority(pair.Key);
            pair.Value.text = priority == 0 ? "禁用" : priority.ToString();
            pair.Value.SetEnabled(hasPerson);
            for (int level = 0; level <= 4; level++)
            {
                pair.Value.EnableInClassList("priority-" + level, level == priority);
            }
        }
    }

    private void RefreshSchedule()
    {
        int currentSlot = ScheduleSystem.Instance ? ScheduleSystem.Instance.CurrentSlot : 0;
        bool hasPerson = bridge.SelectedPerson;
        currentHourLabel.text = "当前时段 " + currentSlot.ToString("00") + ":00";
        for (int i = 0; i < scheduleButtons.Count; i++)
        {
            Button button = scheduleButtons[i];
            ScheduleActivity activity = bridge.GetScheduleActivity(i);
            button.SetEnabled(hasPerson);
            button.tooltip = i.ToString("00") + ":00  " + GetActivityName(activity);
            button.EnableInClassList("current-slot", i == currentSlot);
            foreach (ScheduleActivity value in Enum.GetValues(typeof(ScheduleActivity)))
            {
                button.EnableInClassList("activity-" + value.ToString().ToLowerInvariant(), value == activity);
            }
        }
    }

    private void RefreshActivityToolbar()
    {
        VisualElement toolbar = root.Q<VisualElement>("activity-toolbar");
        foreach (VisualElement child in toolbar.Children())
        {
            child.EnableInClassList("selected", child.name == "activity-" + paintActivity);
        }
    }

    private void ShowPage(bool showPriority)
    {
        priorityPage.style.display = showPriority ? DisplayStyle.Flex : DisplayStyle.None;
        schedulePage.style.display = showPriority ? DisplayStyle.None : DisplayStyle.Flex;
        priorityTab.EnableInClassList("selected", showPriority);
        scheduleTab.EnableInClassList("selected", !showPriority);
    }

    private void SetVisible(bool visible)
    {
        isVisible = visible;
        if (panel != null)
        {
            panel.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        }

        if (visible)
        {
            RefreshPeopleIfNeeded(true);
        }
    }

    private static string GetWorkName(WorkKind workKind)
    {
        switch (workKind)
        {
            case WorkKind.Build: return "建造";
            case WorkKind.Collect: return "采集";
            case WorkKind.Operation: return "操作";
            case WorkKind.Survival: return "生存";
            case WorkKind.Attack: return "战斗";
            case WorkKind.MedicalCare: return "医疗";
            case WorkKind.Study: return "研究";
            case WorkKind.ShipOperation: return "飞船操作";
            default: return workKind.ToString();
        }
    }

    private static string GetActivityName(ScheduleActivity activity)
    {
        switch (activity)
        {
            case ScheduleActivity.Anything: return "任意";
            case ScheduleActivity.Work: return "工作";
            case ScheduleActivity.Sleep: return "睡眠";
            case ScheduleActivity.Recreation: return "娱乐";
            case ScheduleActivity.Medical: return "医疗";
            default: return activity.ToString();
        }
    }
}
