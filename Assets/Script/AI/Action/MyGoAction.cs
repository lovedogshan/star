using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static Constants;

// MyGoAction类继承自AIAction，表示一个AI的“前往目的地”动作
public class MyGoAction : AIAction
{
    // 重写Title属性，返回动作的标题
    public override string Title => "前往目的地";

    // 重写ActionState属性，表示当前动作的状态
    public override AIActionState ActionState { get; set; }

    // Transform属性，表示目标位置的Transform组件
    public Transform Transform { get; set; }

    // Position属性，表示目标位置的坐标
    public Vector3 Position { get; set; }

    // 私有字段，记录上一次的目标位置
    private Vector2Int _oldTargetPosition;

    // IsFind属性，表示是否找到路径
    public bool IsFind { get; set; }

    // IsFailWait属性，表示在失败时是否等待
    public override bool IsFailWait { get; set; } = true;

    // 构造函数，初始化MyGoAction对象
    public MyGoAction(AITask aITask, Transform transform = null, Vector3 position = default) : base(aITask)
    {
        this.Transform = transform;
        Position = position;
    }

    // 重写Execute方法，执行动作
    public override void Execute()
    {
        base.Execute();
        Move(AITask.Person); // 调用Move方法，让角色移动
    }

    // Move方法，负责角色的移动逻辑
    private void Move(Person person)
    {
        Vector3 targetPosition = Transform ? Transform.position : Position;
        if (person.Navigation)
        {
            MovementStatus status = person.Navigation.Navigate(Transform, targetPosition, GetExecuteRange(AITask), AITask.RequiresEva, AITask.EstimatedWorkTime);
            if (status == MovementStatus.Arrived)
            {
                ActionState = AIActionState.Success;
                person.OldGoAction = null;
            }
            else if (status == MovementStatus.Failed)
            {
                ActionState = AIActionState.Fail;
                person.OldGoAction = null;
                person.TryOnNull();
            }

            return;
        }

        // 计算角色的本地位置，考虑是否在船上
        var localPosition = person.transform.localPosition ;
        var roundLocation = BaseValueUtil.Round(localPosition); // 对位置进行四舍五入
        Vector2Int personLoction = new Vector2Int((int)roundLocation.x, (int)roundLocation.y); // 获取角色的整数坐标

        // 计算目标位置的相对坐标
        Vector2Int taskRelposition = BaseValueUtil.RelPositionTransform(targetPosition, person.transform.localPosition);

        // 如果角色没有旧的GoAction，或者目标位置变化较大，重新计算路径
        if (!Equals(person.OldGoAction) || Vector2Int.Distance(taskRelposition, _oldTargetPosition) > 1)
        {
            person.OldGoAction = this;
            person.OldGoAction._oldTargetPosition = taskRelposition;

            // 调用路径查找算法，尝试找到路径
            var isFind = person.FindPathPlus.FindingPath(personLoction, taskRelposition, out person._getPaths, GetExecuteRange(AITask));
            if (!isFind)
            {
                // 如果找不到路径，设置动作为失败状态
                ActionState = AIActionState.Fail;
                _oldTargetPosition = default;
                person.OldGoAction = null;
                person.TryOnNull();

                // 在调试模式下绘制一条红线，表示路径查找失败
//                var pos = (Vector2)personLoction + (Vector2)MainShip.Self.transform.position;
//                Debug.DrawLine(pos, pos + new Vector2(0.1f, 0.1f), Color.red);
                return;
            }
        }

        // 更新角色的空闲计时器
        if (GameManager.Instance && GameManager.Instance.gameValue != null)
        {
            person.OnNullTimer = GameManager.Instance.gameValue.currentPeriod;
        }

        // 执行移动逻辑，如果移动完成，设置动作为成功状态
        if (!MoveExecute(person))
        {
            ActionState = AIActionState.Success;
        }
    }

    // MoveExecute方法，负责执行具体的移动逻辑
    public static bool MoveExecute(Person person)
    {
        if (person._getPaths.Count > 0)
        {
            // 计算移动方向
            Vector2 direction = (person._getPaths[0] - (Vector2)person.transform.localPosition).normalized;

            // 根据移动方向调整角色的朝向
            Vector3 scale = person.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            person.transform.localScale = scale;

            // Move in world space through the shared collision-safe movement path.
            Transform parent = person.CurrentRegion ? person.CurrentRegion.CoordinateRoot : person.shiper ? person.shiper.transform : person.transform.parent;
            Vector2Int pathPoint = person._getPaths[0];
            Vector3 localWaypoint = new Vector3(pathPoint.x, pathPoint.y, person.transform.localPosition.z);
            Vector2 worldTarget = parent ? parent.TransformPoint(localWaypoint) : localWaypoint;
            Vector2 worldDirection = (worldTarget - (Vector2)person.transform.position).normalized;
            person.TryMoveSafely(worldDirection * ((person.MoveSpeed + person.MoveSpeedSlow) * Time.deltaTime), out _);

            // Only consume a waypoint after the collision-safe move actually reached it.
            if (Vector2.Distance(person.transform.localPosition, person._getPaths[0]) < 0.02f)
            {
                person._getPaths.RemoveAt(0);
            }

            if (person._getPaths.Count > 0)
            {
                Debug.DrawLine(person.transform.localPosition, (Vector2)person._getPaths.Last());
            }
        }

        // 更新角色的跑步状态
        person.IsRun = person._getPaths.Count > 0;
        //person.SetRunState(person.IsRun);

        // 返回角色是否还在移动
        return person.IsRun;
    }

    // GetExecuteRange方法，获取执行范围
    public static int GetExecuteRange(AITask aITask, Person person = null)
    {
        if (!person)
        {
            person = aITask.Person;
        }

        // 根据任务类型决定使用长范围还是短范围
        int reduceRange;
        reduceRange = aITask.IsLongRange ? person._longRange : person._shortRange;
        return reduceRange;
    }

    // CheckFindPath方法，检查是否可以找到路径
    public static bool CheckFindPath(MyGoAction myGoAction)
    {
        return CheckFindPath(myGoAction.AITask.Person, myGoAction.Transform != null ? myGoAction.Transform.position : myGoAction.Position, GetExecuteRange(myGoAction.AITask));
    }

    // CheckFindPath方法的重载，检查角色是否可以找到路径
    public static bool CheckFindPath(Person person, Vector2 point, int range)
    {
        if (person.Navigation)
        {
            return person.Navigation.CanReach(point, range);
        }

        var localPosition = person.transform.localPosition;
        Vector2Int personLoction = new Vector2Int((int)localPosition.x, (int)localPosition.y);
        Vector2Int taskRelposition = BaseValueUtil.RelPositionTransform(point, person.transform.localPosition);

        // 调用路径查找算法，检查是否可以找到路径
        return person.FindPathPlus.FindingPath(personLoction, taskRelposition, out _, range);
    }

    public override void OnFail()
    {
        AITask.Person?.Navigation?.Cancel();
        base.OnFail();
    }

    public override void OnStop()
    {
        AITask.Person?.Navigation?.Cancel();
        base.OnStop();
    }

    // CheckFindPathAsync方法，异步检查是否可以找到路径
/*    public static async System.Threading.Tasks.Task CheckFindPathAsync(Person person, Vector2 point, int range, FindPath.FindPathResult findPathResult = null)
    {
        var localPosition = person.transform.localPosition + (person.place == PersonPlace.Ship ? Vector3.zero : new Vector3(0.5f, 0.5f));
        Vector2Int personLoction = new Vector2Int((int)localPosition.x, (int)localPosition.y);
        Vector2Int taskRelposition = BaseValueUtil.RelPositionTransform(point, person.place == PersonPlace.Ship ? person.Shiper.transform.localPosition : Vector3.zero);

        // 异步调用路径查找算法
        await person.FindPathPlus.TryFind(personLoction, taskRelposition, range, findPathResult);
    }

    // CheckFailRestore方法，检查是否需要在失败时恢复
    public override bool CheckFailRestore()
    {
        return base.CheckFailRestore() && CheckFindPath(this);
    }*/
}
