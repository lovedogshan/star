# NPC 行动、工作与分层寻路架构方案

## 1. 设计目标

本方案用于实现类似殖民地模拟游戏的 NPC 行动与工作系统，并适配当前项目的特殊环境：

- 飞船、空间站内部为有限且会移动、旋转的区域。
- NPC 可以在舱内工作，也可能通过气闸执行舱外任务。
- 宇宙空间近乎无限，不能建立覆盖全宇宙的 A* 网格。
- 宇宙中可能同时存在 EVA NPC、玩家飞船和 NPC 飞船。
- 没有舱外活动设备的 NPC 不允许出舱。
- 保留并逐步改造现有的 `Person`、`AITask`、`AIAction`、`TaskController`、`FindPath`、`PathGrid` 和 `Shiper` 系统。

核心原则：

> 无限宇宙只在数据层无限；完整寻路、物理和 AI 只在有限的活动区域运行。

---

## 2. 总体架构

```text
设施、货物、环境事件
        ↓ 发布工作
区域工作池 JobBoard
        ↓
NPC 工作选择器
        ↓
作息、优先级、技能、设备、区域权限筛选
        ↓
ReservationManager 预约目标
        ↓
生成 NavigationRoute
        ↓
┌─────────────────────────────────────┐
│ 舱内：GridMovementDriver + 局部 A*  │
│ 气闸：PortalMovementDriver          │
│ 舱外：EvaMovementDriver + 局部避障  │
│ NPC 船：ShipMovementDriver          │
│ 远程：RemoteTravelDriver 数据模拟   │
└─────────────────────────────────────┘
        ↓
执行工作、完成或失败
        ↓
释放所有预约
```

系统分为五层：

1. **需求与作息层**：决定 NPC 当前应该工作、进食、休息还是处理紧急情况。
2. **工作分配层**：从当前 NPC 可执行的任务池中选择工作。
3. **任务执行层**：将完整工作拆成多个可中断的行为步骤。
4. **导航层**：根据区域类型选择对应的移动方式。
5. **世界模拟层**：控制近距离完整模拟和远距离数据模拟。

---

## 3. 与现有代码的兼容关系

现有类建议保留以下职责：

| 现有类 | 保留职责 | 后续改造方向 |
|---|---|---|
| `Person` | NPC 属性、技能、需求和状态 | 增加导航、EVA 能力和所在区域信息 |
| `AITask` | 描述一个完整工作 | 增加区域、能力需求、预约目标和环境要求 |
| `AIAction` | 描述工作中的一个步骤 | 增加取消、超时和失败原因 |
| `AIBehavior` | 执行 NPC 自己的任务队列 | 避免递归执行，改为稳定的逐 Tick 状态机 |
| `TaskController` | 任务注册、调度和强制任务 | 不再用全局列表匹配所有任务和所有 NPC |
| `MyGoAction` | 前往目标的行为步骤 | 改为统一调用 `PersonNavigation` |
| `FindPath` | 舱内 A* 计算 | 使用优先队列、请求队列和查询版本号优化 |
| `PathGrid` | 舱内格子和障碍数据 | 增加区域编号、网格版本和局部更新 |
| `Shiper` | 维护飞船内部网格 | 继续使用飞船本地坐标，注册气闸 Portal |

现有任务和行为结构可以继续使用，不需要一次性推翻。优先把 `MyGoAction` 与具体 A* 实现解耦，之后再逐步升级其他模块。

---

## 4. 工作系统

### 4.1 工作任务

一个 `AITask` 表示完整工作，例如：

```text
搬运货物：
预约货物 → 前往货物 → 拾取 → 前往仓储格 → 放下 → 释放预约

舱外维修：
预约目标 → 取得 EVA 设备 → 前往气闸 → 出舱 → 前往目标
→ 维修 → 返回气闸 → 入舱 → 归还设备 → 释放预约
```

建议为任务增加以下数据：

```csharp
public int RegionId;
public WorkKind WorkKind;
public int Priority;
public bool IsEmergency;
public bool RequiresEva;
public float EstimatedWorkTime;
public float MaxTravelDistance;
```

任务只描述“要做什么”，不直接控制具体寻路方式。

### 4.2 工作优先级

每个 NPC 维护独立工作优先级：

```text
0：禁止
1：低
2：普通
3：高
4：极高
```

任务选择顺序建议为：

```text
生存危险
> 玩家强制任务
> 火灾、失压、医疗等紧急任务
> 作息要求
> 工作优先级
> 技能效率
> 距离与任务负载
> 闲逛
```

候选任务评分示例：

```csharp
score = taskPriority * 1000f
      + workPriority * 200f
      + skillLevel * 20f
      - estimatedDistance
      - queuedTaskCount * 50f;
```

---

## 5. 分区域任务池

### 5.1 为什么不能只扫描全局 MainTasks

如果有 `T` 个任务和 `P` 个 NPC，并且每个任务都检查所有 NPC，分配成本接近：

```text
T × P × 路径检查成本
```

例如 100 个任务和 30 个 NPC，会产生 3000 次候选检查。如果候选检查包含 A*，成本会迅速增长。

可能造成：

- NPC 和任务增加后周期性掉帧。
- 一批货物出现时产生瞬时卡顿。
- 对明显不可达或无能力执行的任务进行无效寻路。
- 为多个 NPC 计算路径，最终却只使用其中一条。
- 大量临时列表和路径结果触发垃圾回收。
- 紧急任务被普通任务扫描拖慢。

### 5.2 推荐结构

保留 `MainTasks` 作为存档、统计、UI 和调试用总表，但匹配任务时使用索引：

```text
JobSystem
├─ EmergencyJobs
├─ ShipAJobBoard
├─ ShipBJobBoard
├─ StationJobBoard
└─ ExteriorJobBoard
```

每个区域再按工作类型分类：

```csharp
Dictionary<JobRegionKey, List<AITask>> jobsByRegionAndType;
```

NPC 只查询：

- 当前区域的任务。
- 通过开放 Portal 可到达的相邻区域任务。
- 当前 NPC 工作优先级允许的任务类型。
- 全局紧急任务。

### 5.3 推荐匹配流程

```text
NPC 进入空闲状态
→ 获取当前区域的可用任务池
→ 按作息、优先级、技能、装备过滤
→ 按区域连通性过滤
→ 用直线距离选出前 2~3 个候选
→ 仅对最终候选运行精确路径查询
→ 预约任务目标
→ 执行任务
```

---

## 6. 预约系统

预约系统用于防止多个 NPC 同时争抢同一个目标。

```csharp
public interface IReservationManager
{
    bool TryReserve(Person person, object target);
    bool IsReservedByOther(Person person, object target);
    void Release(Person person, object target);
    void ReleaseAll(Person person);
}
```

需要预约的对象包括：

- 货物。
- 仓储格。
- 工作台和设施。
- 床位和座位。
- 气闸。
- EVA 设备。
- 舱外工作目标。
- 必要时的下一移动格。

以下情况必须释放预约：

- 工作完成。
- 工作取消或失败。
- NPC 受伤、死亡或失去行动能力。
- 目标被销毁。
- 路径失效且无法恢复。
- 氧气不足，需要强制返航。

---

## 7. 分层导航

### 7.1 导航区域类型

```csharp
public enum NavigationDomain
{
    InteriorGrid,
    ExteriorSpace,
    Portal,
    RemoteSpace
}
```

每个飞船和空间站拥有独立有限网格。宇宙空间不建立全局网格。

### 7.2 统一移动接口

```csharp
public interface IMovementDriver
{
    MovementStatus Tick(Person person, NavigationSegment segment);
    void Cancel();
}
```

实现：

```text
GridMovementDriver     舱内局部 A*
PortalMovementDriver   门、气闸和对接口
EvaMovementDriver      近距离舱外移动
ShipMovementDriver     NPC 飞船局部移动
RemoteTravelDriver     远距离数据模拟
```

`MyGoAction` 只调用统一的 `PersonNavigation`，不再直接操作 `FindPathPlus`。

### 7.3 路线分段

跨区域路线由多个 `NavigationSegment` 组成：

```text
舱内当前位置
→ 舱内 A* 到气闸
→ 气闸 Portal
→ 舱外连续导航
→ 工作目标
```

返回时执行反向路线。

---

## 8. 舱内 A* 寻路

### 8.1 网格范围

每艘飞船、每座空间站拥有独立 `PathGrid`：

```text
ShipGrid_A
ShipGrid_B
StationGrid
```

网格只包含实际地板，不包含宇宙空间。

NPC 在飞船内使用 `localPosition` 寻路。飞船移动和旋转时，舱内路线不会失效，NPC 也会跟随飞船移动。

### 8.2 A* 性能优化

现有 `FindPath` 可以逐步升级：

- 用二叉堆或优先队列管理开放节点。
- 用数组标记或查询版本号代替 `List.Contains`。
- 不在每次查询时重置整张网格。
- 路径请求进入统一队列，每帧限制计算数量。
- 地板、墙壁或门变化时，只更新相关格子。
- 为网格增加 `GridRevision`，地图改变后使旧缓存失效。
- 压缩连续同方向节点，只保留转向点。
- 先检查区域连通性，再运行精确 A*。

### 8.3 路径请求调度

不要让所有 NPC 同一帧计算路径：

```csharp
public class PathRequestScheduler
{
    public int MaxRequestsPerFrame = 2;
}
```

紧急任务可以获得更高请求优先级。

---

## 9. 气闸与 Portal

气闸是导航区域之间的连接点，不是普通可通行格。

```csharp
public class AirlockPortal : MonoBehaviour
{
    public int InteriorRegionId;
    public Vector2Int InteriorCell;
    public Transform ExteriorPoint;
    public bool IsOperational;
    public bool IsOpen;
}
```

气闸行为步骤：

```text
预约气闸
→ 前往内门
→ 检查 EVA 能力
→ 关闭内门
→ 泄压
→ 打开外门
→ 切换导航区域
→ 释放气闸
```

气闸故障、失去电力或正在被其他 NPC 使用时，路线规划应选择其他气闸或等待重试。

---

## 10. EVA 能力与安全限制

NPC 的舱外技能和设备应分开管理：

```csharp
public class EvaCapability
{
    public bool HasSuit;
    public bool HasOxygen;
    public bool HasThruster;
    public bool HasTether;
    public float OxygenRemaining;

    public bool CanEva => HasSuit && HasOxygen && HasThruster;
}
```

必须进行三重检查：

1. **任务分配时**：无设备 NPC 不进入舱外任务候选列表。
2. **路线生成时**：无 EVA 能力时，不生成穿过气闸的路线。
3. **出舱前**：再次检查设备、氧气和气闸状态。

氧气预算必须覆盖：

```text
前往时间 + 工作时间 + 返回时间 + 安全余量
```

氧气低于返航阈值时，中止当前工作并插入强制返航任务。

---

## 11. 舱外连续导航

宇宙空间不运行全局 A*。近距离 EVA 使用连续导航：

```text
朝目标直接移动
→ 低频检测前方障碍
→ 有障碍时生成少量临时绕行点
→ 绕过后恢复直线移动
```

建议：

- 移动插值每帧执行。
- 导航决策每秒 5~10 次。
- 障碍检测每秒 3~5 次。
- 只查询附近空间区块。
- 使用 LayerMask 和 NonAlloc 物理查询。
- EVA NPC 设置最大活动距离和氧气范围。

连续导航开销取决于玩家附近活动对象数量，而不是宇宙总大小。

---

## 12. NPC 飞船和无限宇宙

### 12.1 分级模拟

| 距离层级 | 模拟方式 |
|---|---|
| 玩家附近 | 完整实体、物理、炮台、战斗和避障 |
| 中距离 | 低频位置更新、简化避障和战斗 |
| 远距离 | 数据对象、航线、速度和预计到达时间 |
| 跨星系 | 事件、任务状态和时间推进 |

### 12.2 远距离数据

```csharp
public class SimulatedShip
{
    public Double2 Position;
    public Double2 Destination;
    public double Speed;
    public double ArrivalTime;
    public ShipSimulationState State;
}
```

远距离飞船不需要：

- `GameObject`。
- `Rigidbody2D`。
- 每帧 `Update`。
- 炮台目标检测。
- 完整舱内 NPC 模拟。
- 局部避障。

进入玩家活动范围、发生战斗或准备停靠时，再转换为完整实体。

### 12.3 空间索引

使用空间哈希或区块索引：

```text
SpaceSpatialIndex
├─ Chunk(10, 12)
├─ Chunk(10, 13)
└─ Chunk(11, 12)
```

单位只查询附近区块，避免每个单位检测所有其他单位形成平方级开销。

---

## 13. 调度频率

不同系统使用不同更新频率：

| 系统 | 推荐频率 |
|---|---|
| NPC 画面移动 | 每帧 |
| 当前任务状态机 | 每帧或固定 Tick |
| 工作重新选择 | 0.25~1 秒一次 |
| 舱外导航决策 | 每秒 5~10 次 |
| 障碍检测 | 每秒 3~5 次 |
| 感知和目标搜索 | 每秒 1~2 次 |
| 远距离飞船模拟 | 每秒或更低频 |

NPC 的思考时间应错开，避免所有角色在同一帧更新。

失败任务采用指数退避：

```text
第一次失败：短暂等待
第二次失败：更长等待
连续失败：暂停任务，等待地图或目标状态变化事件
```

不要持续每帧重试不可达任务。

---

## 14. 推荐目录结构

```text
Assets/Script/AI
├─ Core
│  ├─ PersonNavigation.cs
│  ├─ NavigationTarget.cs
│  └─ NavigationRoute.cs
├─ Jobs
│  ├─ JobSystem.cs
│  ├─ RegionJobBoard.cs
│  ├─ JobSelector.cs
│  └─ ReservationManager.cs
├─ Navigation
│  ├─ IMovementDriver.cs
│  ├─ GridMovementDriver.cs
│  ├─ PortalMovementDriver.cs
│  ├─ EvaMovementDriver.cs
│  ├─ ShipMovementDriver.cs
│  ├─ RemoteTravelDriver.cs
│  └─ PathRequestScheduler.cs
├─ World
│  ├─ NavigationRegion.cs
│  ├─ RegionConnectivity.cs
│  ├─ AirlockPortal.cs
│  ├─ SpaceSpatialIndex.cs
│  └─ SpaceSimulationManager.cs
└─ EVA
   ├─ EvaCapability.cs
   ├─ EvaEquipment.cs
   └─ EvaSafetyController.cs
```

---

## 15. 实施顺序

### 第一阶段：舱内工作闭环

- 建立区域工作池。
- 建立预约系统。
- 优化舱内 A* 请求调度。
- 完成“发现货物 → 预约 → 搬运 → 入库 → 释放预约”。

### 第二阶段：统一导航入口

- 新建 `PersonNavigation`。
- 将 `MyGoAction` 改为调用统一导航接口。
- 保留 `GridMovementDriver` 适配现有 `FindPath`。
- 增加路线失败原因和取消逻辑。

### 第三阶段：气闸和 EVA

- 建立 `AirlockPortal`。
- 增加 EVA 设备、氧气和能力检查。
- 实现气闸预约与状态机。
- 完成近距离舱外维修闭环。

### 第四阶段：NPC 飞船和远程模拟

- 建立 `SpaceSpatialIndex`。
- 增加 NPC 飞船局部导航。
- 实现完整实体与远程数据对象的切换。
- 加入远程航线、到达时间和事件模拟。

### 第五阶段：需求和高级工作

- 作息、睡眠、进食和娱乐。
- 维修、生产、医疗和消防。
- 舱压、氧气和危险区域。
- 战斗、逃跑和紧急疏散。

---

## 16. 最终原则

1. `MainTasks` 可以保留为任务总表，但不能作为唯一的运行时查询结构。
2. 空闲 NPC 从小范围、已分类的任务池中主动选择工作。
3. 分配工作前先进行廉价筛选，只对少量候选执行精确寻路。
4. 舱内使用有限局部 A*，宇宙空间不建立全局网格。
5. 气闸负责连接导航区域，并在出舱前执行最终安全检查。
6. 没有 EVA 设备的 NPC 在任务分配、路线生成和气闸执行三个阶段都会被拦截。
7. 玩家附近使用完整实体模拟，远距离宇宙使用数据和时间模拟。
8. 所有工作目标、设施、气闸和资源必须支持预约与可靠释放。
9. 不让每个 NPC 每帧扫描场景、查找目标或重新计算路径。
10. 参考源码只借鉴设计思想，项目中的实现应保持独立、清晰且可维护。

