using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using static Constants;
using System.Threading.Tasks;

public class TaskController : MonoBehaviour
{
   public static TaskController Instance { get; private set; }
   public List<Person> People { get; } = new List<Person>();

    public List<AITask> MainTasks { get; set; } = new List<AITask>();

    [SerializeField, Min(1)]
    private int assignmentsPerFrame = 4;
    [SerializeField, Min(0.1f)]
    private float fallbackRefreshInterval = 1f;

    private readonly Queue<Person> assignmentQueue = new Queue<Person>();
    private readonly HashSet<Person> queuedPeople = new HashSet<Person>();
    private float nextFallbackRefresh;

    private void Awake()
    {
        Instance = this;
        EnsureRuntimeServices();
    }

    private void Start()
    {
        Person[] people = FindObjectsOfType<Person>();
        foreach (Person person in people)
        {
            AddPerson(person);
        }
    }

    private void Update()
    {
        if (!JobSystem.Instance)
        {
            return;
        }

        if (Time.unscaledTime >= nextFallbackRefresh)
        {
            nextFallbackRefresh = Time.unscaledTime + fallbackRefreshInterval;
            RequestAllAssignments();
        }

        int remaining = assignmentsPerFrame;
        while (remaining-- > 0 && assignmentQueue.Count > 0)
        {
            Person person = assignmentQueue.Dequeue();
            queuedPeople.Remove(person);
            if (person)
            {
                JobSystem.Instance.TryAssignBestTask(person);
            }
        }
    }

/*  private async void Start()
    {
        await AssignTaskAsync();
    }*/
    private void OnDestroy()
    {
        isAsyncWork = false;
    }

    public void AddPerson(Person person)
    {
        if (person && !People.Contains(person))
        {
            People.Add(person);
            ScheduleSystem.Instance?.Register(person);
            RequestAssignment(person);
        }
    }

    public void RemovePerson(Person person)
    {
        People.Remove(person);
        queuedPeople.Remove(person);
        ScheduleSystem.Instance?.Unregister(person);
    }

    public void RequestAssignment(Person person)
    {
        if (!person || !People.Contains(person) || !person.CanReceiveAutomaticTask || !person.IBehavior ||
            person.IBehavior.HasAssignedWork || person.ExecuteState != PersonExecuteState.Free ||
            queuedPeople.Contains(person))
        {
            return;
        }

        queuedPeople.Add(person);
        assignmentQueue.Enqueue(person);
    }

    public void RequestEmergencyAssignment(Person person)
    {
        if (!person || !People.Contains(person) || !person.CanReceiveAutomaticTask || !person.IBehavior ||
            queuedPeople.Contains(person))
        {
            return;
        }

        queuedPeople.Add(person);
        assignmentQueue.Enqueue(person);
    }

    public void RequestAllAssignments()
    {
        for (int i = 0; i < People.Count; i++)
        {
            RequestAssignment(People[i]);
        }
    }

    public void RequestEmergencyAssignments()
    {
        for (int i = 0; i < People.Count; i++)
        {
            RequestEmergencyAssignment(People[i]);
        }
    }

    private void JvnY7()
    {

    }

    public bool AddTaskToMain(AITask aITask, Person person = null)
    {
        if (aITask == null)
        {
            return false;
        }

        aITask.ResolveNavigationMetadata();
        if (person)
        {
            return AddTaskToPerson(aITask, person);
        }

        if(MainTasks.Find(x => x.CheckSame(aITask)) != null)
        {
            aITask.AbortSafe();
            return false;
        }
        MainTasks.Add(aITask);
        SortTaskByLevel(MainTasks);
        JobSystem.Instance?.Register(aITask);
        return true;
    }


    private IEnumerator OnTaskUnitRole(List<AITask> aITasks)
    {
        var firstTask = aITasks[0];
        AddTaskToMain(firstTask);
        while(true)
        {
            if (firstTask.Person)
            {
                foreach (var item in aITasks)
                {
                    if(item.Person == null)
                    {
                        AddTaskToMain(item, firstTask.Person);
                    }
                }
                break;
            }
            yield return null; // 锟饺达拷锟斤拷一帧
        }
    }

    private bool AddTaskToPerson(AITask aITask, Person person)
    {
        if(People.Contains(person) && person.IBehavior)
        {
            aITask.Person = person;
            aITask.State = TaskState.assign;

            person.IBehavior.AITasks.Add(aITask);
            person.IBehavior.OnGetTaskEvent?.Invoke(aITask);
            person.ExecuteState = PersonExecuteState.Await;

            SortTaskByLevel(person.IBehavior.AITasks);
            return true;
        }
        return false;
    }

    public bool TryAssignTask(AITask aITask, Person person)
    {
        if (aITask == null || !person || !person.IBehavior || aITask.State != TaskState.unassign)
        {
            return false;
        }

        if (JobSystem.Instance && !JobSystem.Instance.CanPersonPerform(person, aITask))
        {
            return false;
        }

        bool assigned = AddTaskToPerson(aITask, person);
        if (assigned)
        {
            MainTasks.Remove(aITask);
            JobSystem.Instance?.Unregister(aITask);
        }

        return assigned;
    }

    public bool AssignPlayerCommand(AITask task, Person person, bool queue)
    {
        if (task == null || !person || !person.IBehavior || task.State != TaskState.unassign)
        {
            return false;
        }

        if (task.ReservationTarget != null && ReservationManager.Instance &&
            !ReservationManager.Instance.TryReserve(person, task.ReservationTarget, task))
        {
            return false;
        }

        task.Person = person;
        task.State = TaskState.assign;
        if (queue)
        {
            person.IBehavior.AITasks.Add(task);
        }
        else
        {
            AITask current = person.IBehavior.CurrentTask;
            if (current != null && current.State == TaskState.work && !current.IsStop)
            {
                current.isPend = true;
                current.IsStop = true;
            }

            person.IBehavior.AITasks.Insert(0, task);
        }

        person.IBehavior.OnGetTaskEvent?.Invoke(task);
        person.ExecuteState = PersonExecuteState.Await;
        return true;
    }

    public bool PrioritizeTask(AITask task, bool emergency)
    {
        if (task == null || task.State == TaskState.succeed || task.State == TaskState.abort)
        {
            return false;
        }

        task.Level = Mathf.Max(task.Level, emergency ? 100 : 50);
        task.IsEmergency |= emergency;
        SortTaskByLevel(MainTasks);
        if (task.Person && task.Person.IBehavior)
        {
            SortTaskByLevel(task.Person.IBehavior.AITasks);
        }

        if (task.IsEmergency)
        {
            RequestEmergencyAssignments();
        }
        else
        {
            RequestAllAssignments();
        }

        return true;
    }

    public bool CancelTask(AITask aITask)
    {
        if (aITask == null)
        {
            return false;
        }

        if (aITask.Person && aITask.Person.IBehavior)
        {
            aITask.Person.IBehavior.CancelTask(aITask);
            return true;
        }

        bool existed = MainTasks.Remove(aITask);
        JobSystem.Instance?.Unregister(aITask);
        ReservationManager.Instance?.ReleaseTask(aITask);
        if (!existed && aITask.State != TaskState.unassign)
        {
            return false;
        }

        aITask.AbortSafe();
        return true;
    }



    public static void SortTaskByLevel(List<AITask> tasks)
    {
        tasks.Sort((x, y) =>
        {
            int emergency = y.IsEmergency.CompareTo(x.IsEmergency);
            return emergency != 0 ? emergency : y.Level.CompareTo(x.Level);
        });
    }

    private bool TaskSleepTimer(AITask task)
    {
        if (task.sleepTimer < task.SleepTimeMax)
        {
            task.sleepTimer += Time.deltaTime;
            return false;
        }
        else
        {
            task.sleepTimer = 0;
            return true;
        }
    }

    private static bool isAsyncWork;



/*   private async Task AssignTaskAsync()
    {
        isAsyncWork = true;

        while(isAsyncWork)
        {
            async void action()
            {
                var index = 0;
                var removeTasks = new List<AITask>();
                while(index < MainTasks.Count)
                {
                    var task = MainTasks[index];
                    index++;

                    try
                    {
                        if(await AssignTask(task))
                        {
                            removeTasks.Add(task);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogException(ex);
                        removeTasks.Add(task);
                    }
                }

                removeTasks.ForEach(x => RemoveAITaskImmediate(x)); // 锟狡筹拷锟斤拷锟斤拷
                removeTasks.Clear(); // 锟斤拷锟斤拷瞥锟斤拷斜锟?
            }

            var time = BaseValueUtil.DebugTimer(action);


            await Task.Delay(50);
        }
    }

/*   private async Task<bool> AssignTask(AITask task)
    {
        if (task.State == TaskState.unassign && TaskSleepTimer(task)) // 锟斤拷锟斤拷锟斤拷锟轿达拷锟斤拷锟斤拷锟斤拷锟揭拷锟斤拷锟?
        {
            var isChoose = await ChooseBeatCandidate(task); // 选锟斤拷锟斤拷呀锟缴?
            if (!isChoose) // 锟斤拷锟轿囱★拷锟斤拷色
            {
                task.SleepTimeMax += AITask.SleepTimeMaxOffset; // 锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷时锟斤拷
            }
            else // 锟斤拷锟窖★拷锟斤拷色
            {
                if (task.Person.IBehavior.AITasks.Find(x => x != task && x.CheckSameRoleTasks(task)) != null) // 锟斤拷锟斤拷锟缴拷锟斤拷锟斤拷锟斤拷锟斤拷锟斤拷锟?
                {
                    task.State = TaskState.unassign; // 锟斤拷锟斤拷锟斤拷锟斤拷状态
                    return true;
                }
            }
        }
        return false;
    }

/*   private async Task<bool> ChooseBeatCandidate(AITask task)
    {
        foreach (var person in await GetPeopleByWorkPriority(task))
        {
            if(person.ExecuteState == PersonExecuteState.Free && AddTaskToPerson(task, person))
            {
                return true;
            }
        }
        return false;
    }

/*   private async Task<List<Person>>GetPeopleByWorkPriority(AITask task)
    {
        var workKind = task.WorkKind ;
        var workers = new List<Person>();
        foreach (var person in People)
        {
            if(task.IsFindPath)
            {
                var target = new FindPath.FindPathResult();
                await MyGoAction.CheckFindPathAsync(person, task.TargetPoint, MyGoAction.GetExecuteRange(task
    , person), target);
                if(!target.isResult)
                {
                    continue;
                }
            }
            if (PersonManager.Instance.mainWorkPriorityTables[person].workPrioritys[(int)workKind] > WorkPriorityTable.Level.Forbid) // 锟斤拷锟斤拷锟缴拷锟斤拷锟街达拷锟斤拷锟斤拷锟?
            {
                workers.Add(person); // 锟斤拷锟斤拷色锟斤拷锟接碉拷锟斤拷锟斤拷锟斤拷锟叫憋拷
            }

        }
        workers.Sort((x, y) => WorkPersonComparison(x, y, workKind));
        return workers;

    }*/

//    public void RemoveAITaskImmediate(OrderMark orderMark) => RemoveAITaskImmediate(orderMark.AITask);


    public void RemoveAITaskImmediate(AITask aITask)
    {
        if (aITask !=null)
        {
            RemoveAITask(aITask);
            if (aITask.State == TaskState.unassign)
            {
                aITask.AbortSafe();
                MainTasks.Remove(aITask);
                JobSystem.Instance?.Unregister(aITask);
                ReservationManager.Instance?.ReleaseTask(aITask);
            }
            else
            {
                aITask.DoingWork();
                aITask.Person.IBehavior.ExecuteTask(aITask);
            }
        }
    }

    public void RemoveAITask(AITask aITask)
    {
        var index = aITask.ExecuteIndex;
        if (index < aITask.AIActions.Count)
        {
            var action = aITask.AIActions[index];
            action.ActionState = AIActionState.Fail;
            action.IsFailWait = false;
        }
        aITask.isKill = true;
    }

    private void EnsureRuntimeServices()
    {
        if (!GetComponent<JobSystem>())
        {
            gameObject.AddComponent<JobSystem>();
        }

        if (!GetComponent<ReservationManager>())
        {
            gameObject.AddComponent<ReservationManager>();
        }

        if (!GetComponent<PathRequestScheduler>())
        {
            gameObject.AddComponent<PathRequestScheduler>();
        }

        if (!GetComponent<ScheduleSystem>())
        {
            gameObject.AddComponent<ScheduleSystem>();
        }

        if (!GetComponent<CommandSystem>())
        {
            gameObject.AddComponent<CommandSystem>();
        }
    }
}
