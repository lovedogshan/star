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

    private void Awake()
    {
        Instance = this;
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
        People.Add(person);
    }

    public void RemovePerson(Person person)
    {
        People.Remove(person);
    }

    private void JvnY7()
    {

    }

    public bool AddTaskToMain(AITask aITask, Person person = null)
    {
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
            yield return null; // �ȴ���һ֡
        }
    }

    private bool AddTaskToPerson(AITask aITask, Person person)
    {
        if(People.Contains(person))
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



    public static void SortTaskByLevel(List<AITask> tasks)
    {
        tasks.Sort((x, y) => -x.Level.CompareTo(y.Level)); // ���������ȼ���������
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

                removeTasks.ForEach(x => RemoveAITaskImmediate(x)); // �Ƴ�����
                removeTasks.Clear(); // ����Ƴ��б�
            }

            var time = BaseValueUtil.DebugTimer(action);


            await Task.Delay(50);
        }
    }

/*   private async Task<bool> AssignTask(AITask task)
    {
        if (task.State == TaskState.unassign && TaskSleepTimer(task)) // �������δ��������Ҫ����
        {
            var isChoose = await ChooseBeatCandidate(task); // ѡ����ѽ�ɫ
            if (!isChoose) // ���δѡ���ɫ
            {
                task.SleepTimeMax += AITask.SleepTimeMaxOffset; // ������������ʱ��
            }
            else // ���ѡ���ɫ
            {
                if (task.Person.IBehavior.AITasks.Find(x => x != task && x.CheckSameRoleTasks(task)) != null) // �����ɫ������������
                {
                    task.State = TaskState.unassign; // ��������״̬
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
            if (PersonManager.Instance.mainWorkPriorityTables[person].workPrioritys[(int)workKind] > WorkPriorityTable.Level.Forbid) // �����ɫ����ִ������
            {
                workers.Add(person); // ����ɫ���ӵ��������б�
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
}
