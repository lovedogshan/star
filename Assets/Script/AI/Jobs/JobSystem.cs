using System.Collections.Generic;
using UnityEngine;
using static Constants;

[System.Serializable]
public class PersonWorkProfile
{
    private const int DefaultPriority = 2;
    private const int MaxPriority = 4;

    [System.Serializable]
    private struct Entry
    {
        public WorkKind WorkKind;
        public int Priority;
    }

    [SerializeField]
    private List<Entry> entries = new List<Entry>();
    [SerializeField, HideInInspector]
    private int[] priorities;

    public int GetPriority(WorkKind workKind)
    {
        EnsureEntries();
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].WorkKind == workKind)
            {
                return Mathf.Clamp(entries[i].Priority, 0, MaxPriority);
            }
        }

        return workKind == WorkKind.None ? 0 : DefaultPriority;
    }

    public bool SetPriority(WorkKind workKind, int priority)
    {
        EnsureEntries();
        int clamped = Mathf.Clamp(priority, 0, MaxPriority);
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].WorkKind != workKind)
            {
                continue;
            }

            if (entries[i].Priority == clamped)
            {
                return false;
            }

            Entry entry = entries[i];
            entry.Priority = clamped;
            entries[i] = entry;
            return true;
        }

        entries.Add(new Entry { WorkKind = workKind, Priority = clamped });
        return true;
    }

    public void GetEnabledWorkKinds(int priority, List<WorkKind> output)
    {
        EnsureEntries();
        for (int i = 0; i < entries.Count; i++)
        {
            Entry entry = entries[i];
            if (entry.Priority == priority && entry.WorkKind != WorkKind.None)
            {
                output.Add(entry.WorkKind);
            }
        }
    }

    private void EnsureEntries()
    {
        if (entries == null)
        {
            entries = new List<Entry>();
        }

        int count = (int)WorkKind.None + 1;
        if (entries.Count == 0 && priorities != null)
        {
            int legacyCount = Mathf.Min(priorities.Length, count);
            for (int value = 0; value < legacyCount; value++)
            {
                WorkKind workKind = (WorkKind)value;
                entries.Add(new Entry
                {
                    WorkKind = workKind,
                    Priority = Mathf.Clamp(priorities[value], 0, MaxPriority)
                });
            }
        }

        for (int value = 0; value < count; value++)
        {
            WorkKind workKind = (WorkKind)value;
            bool exists = false;
            for (int i = 0; i < entries.Count; i++)
            {
                if (entries[i].WorkKind == workKind)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                entries.Add(new Entry
                {
                    WorkKind = workKind,
                    Priority = workKind == WorkKind.None ? 0 : DefaultPriority
                });
            }
        }
    }
}

public class JobSystem : MonoBehaviour
{
    private readonly Dictionary<int, RegionJobBoard> boards = new Dictionary<int, RegionJobBoard>();
    private readonly List<AITask> emergencyJobs = new List<AITask>();
    private readonly Dictionary<Person, float> nextSearchTimes = new Dictionary<Person, float>();
    private readonly List<AITask> candidates = new List<AITask>();
    private readonly List<WorkKind> workKinds = new List<WorkKind>();

    [SerializeField]
    private float searchInterval = 0.5f;
    [SerializeField]
    private int preciseCandidateCount = 3;

    public static JobSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    public void Register(AITask task)
    {
        if (task == null)
        {
            return;
        }

        GetBoard(task.RegionId).Add(task);
        if (task.IsEmergency && !emergencyJobs.Contains(task))
        {
            emergencyJobs.Add(task);
        }

        if (task.IsEmergency)
        {
            TaskController.Instance?.RequestEmergencyAssignments();
        }
        else
        {
            TaskController.Instance?.RequestAllAssignments();
        }
    }

    public void Unregister(AITask task)
    {
        if (task == null)
        {
            return;
        }

        if (boards.TryGetValue(task.RegionId, out RegionJobBoard board))
        {
            board.Remove(task);
        }

        emergencyJobs.Remove(task);
    }

    public bool TryAssignBestTask(Person person)
    {
        if (!person || !TaskController.Instance || !person.CanReceiveAutomaticTask || !person.IBehavior)
        {
            return false;
        }

        if (nextSearchTimes.TryGetValue(person, out float nextSearch) && Time.time < nextSearch)
        {
            return false;
        }

        nextSearchTimes[person] = Time.time + searchInterval + Mathf.Abs(person.GetInstanceID() % 10) * 0.01f;
        int regionId = person.CurrentRegionId;
        candidates.Clear();
        for (int i = 0; i < emergencyJobs.Count; i++)
        {
            AITask emergency = emergencyJobs[i];
            if (IsCheapCandidate(person, emergency) && !candidates.Contains(emergency))
            {
                candidates.Add(emergency);
            }
        }

        if (TryAssignFromCandidates(person, candidates))
        {
            return true;
        }

        if (person.IBehavior.HasAssignedWork || person.ExecuteState != PersonExecuteState.Free)
        {
            return false;
        }

        for (int priority = 4; priority >= 1; priority--)
        {
            workKinds.Clear();
            person.WorkProfile.GetEnabledWorkKinds(priority, workKinds);
            candidates.Clear();
            for (int i = 0; i < workKinds.Count; i++)
            {
                WorkKind workKind = workKinds[i];
                if (ScheduleSystem.Instance && !ScheduleSystem.Instance.AllowsWork(person, workKind))
                {
                    continue;
                }

                Collect(regionId, workKind, candidates);
            }

            candidates.RemoveAll(task => !IsCheapCandidate(person, task));
            if (TryAssignFromCandidates(person, candidates))
            {
                return true;
            }
        }

        return false;
    }

    public void NotifyPersonChanged(Person person)
    {
        if (person)
        {
            nextSearchTimes.Remove(person);
        }
    }

    public bool CanPersonPerform(Person person, AITask task)
    {
        return IsCheapCandidate(person, task);
    }

    private RegionJobBoard GetBoard(int regionId)
    {
        if (!boards.TryGetValue(regionId, out RegionJobBoard board))
        {
            board = new RegionJobBoard(regionId);
            boards.Add(regionId, board);
        }

        return board;
    }

    private bool IsCheapCandidate(Person person, AITask task)
    {
        if (task == null || task.Person || task.State != TaskState.unassign || task is MyRambleTask)
        {
            return false;
        }

        if (person.WorkProfile.GetPriority(task.WorkKind) <= 0)
        {
            return false;
        }

        if (ScheduleSystem.Instance && !ScheduleSystem.Instance.AllowsWork(person, task.WorkKind, task.IsEmergency))
        {
            return false;
        }

        float distance = task.TryGetTargetPosition(out Vector3 evaTarget) ? Vector2.Distance(person.transform.position, evaTarget) : 0f;
        float travelSeconds = distance / Mathf.Max(0.01f, person.MoveSpeed + person.MoveSpeedSlow);
        if (task.RequiresEva && (!person.EvaCapability || !person.EvaCapability.HasOxygenBudget(travelSeconds * 2f, task.EstimatedWorkTime)))
        {
            return false;
        }

        if (task.RegionId != AITask.AnyRegionId && task.RegionId != person.CurrentRegionId && !AirlockPortal.CanTravel(person, person.CurrentRegionId, task.RegionId))
        {
            return false;
        }

        if (task.MaxTravelDistance > 0f && task.TryGetTargetPosition(out Vector3 position) && Vector2.Distance(person.transform.position, position) > task.MaxTravelDistance)
        {
            return false;
        }

        return task.ReservationTarget == null || !ReservationManager.Instance || !ReservationManager.Instance.IsReservedByOther(person, task.ReservationTarget);
    }

    private static float Score(Person person, AITask task)
    {
        float distance = task.TryGetTargetPosition(out Vector3 position) ? Vector2.Distance(person.transform.position, position) : 0f;
        int skillLevel = GetSkillLevel(person, task.WorkKind);
        return (task.IsEmergency ? 100000f : 0f) + task.Level * 1000f + skillLevel * 20f - distance - task.EstimatedWorkTime;
    }

    private void Collect(int regionId, WorkKind workKind, List<AITask> output)
    {
        if (boards.TryGetValue(regionId, out RegionJobBoard currentBoard))
        {
            currentBoard.Collect(workKind, output);
        }

        if (regionId != AITask.AnyRegionId && boards.TryGetValue(AITask.AnyRegionId, out RegionJobBoard globalBoard))
        {
            globalBoard.Collect(workKind, output);
        }
    }

    private bool TryAssignFromCandidates(Person person, List<AITask> source)
    {
        source.Sort((left, right) => Score(person, right).CompareTo(Score(person, left)));
        int count = Mathf.Min(preciseCandidateCount, source.Count);
        for (int i = 0; i < count; i++)
        {
            AITask task = source[i];
            if (task.TryGetTargetPosition(out Vector3 targetPosition) &&
                (task.RegionId == AITask.AnyRegionId || task.RegionId == person.CurrentRegionId) &&
                person.Navigation && !person.Navigation.CanReach(targetPosition, MyGoAction.GetExecuteRange(task, person)))
            {
                continue;
            }

            object target = task.ReservationTarget;
            if (target != null && ReservationManager.Instance &&
                !ReservationManager.Instance.TryReserve(person, target, task))
            {
                continue;
            }

            if (TaskController.Instance.TryAssignTask(task, person))
            {
                return true;
            }

            ReservationManager.Instance?.ReleaseTask(task);
        }

        return false;
    }

    private static int GetSkillLevel(Person person, WorkKind workKind)
    {
        SkillUnit skill = person.SkillUnit;
        if (skill == null)
        {
            return 0;
        }

        switch (workKind)
        {
            case WorkKind.Build:
                return skill.buildLevel;
            case WorkKind.Collect:
                return skill.takeLevel;
            case WorkKind.Operation:
                return skill.operateLevel;
            case WorkKind.Attack:
                return skill.attackLevel;
            case WorkKind.MedicalCare:
                return skill.medicLevel;
            case WorkKind.Study:
                return skill.studyLevel;
            case WorkKind.ShipOperation:
                return skill.shipLevel;
            default:
                return 0;
        }
    }
}
