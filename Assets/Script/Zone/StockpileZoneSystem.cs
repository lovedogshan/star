using System.Collections.Generic;
using UnityEngine;

public sealed class StockpileZoneTaskGenerator : MonoBehaviour
{
    private readonly Dictionary<CargoCrate2D, StockpileHaulTask> tasks =
        new Dictionary<CargoCrate2D, StockpileHaulTask>();
    private readonly HashSet<string> reservedCells = new HashSet<string>();

    [SerializeField, Min(0.25f)]
    private float scanInterval = 1f;

    private float nextScanTime;
    private ZoneManager zoneManager;

    private void Start()
    {
        zoneManager = GetComponent<ZoneManager>() ?? ZoneManager.Instance;
        if (!zoneManager)
        {
            enabled = false;
            return;
        }

        zoneManager.OnZoneCreated += OnZoneChanged;
        zoneManager.OnZoneModified += OnZoneChanged;
        zoneManager.OnZoneDeleted += OnZoneDeleted;
        nextScanTime = Time.time + scanInterval;
    }

    private void Update()
    {
        CleanupFinishedTasks();
        if (Time.time < nextScanTime)
        {
            return;
        }

        nextScanTime = Time.time + scanInterval;
        GenerateTasks();
    }

    private void OnDestroy()
    {
        if (zoneManager)
        {
            zoneManager.OnZoneCreated -= OnZoneChanged;
            zoneManager.OnZoneModified -= OnZoneChanged;
            zoneManager.OnZoneDeleted -= OnZoneDeleted;
        }
    }

    public void Complete(StockpileHaulTask task)
    {
        Release(task);
    }

    public void Release(StockpileHaulTask task)
    {
        if (task == null)
        {
            return;
        }

        reservedCells.Remove(CellKey(task.ZoneId, task.TargetCell));
        if (task.Cargo && tasks.TryGetValue(task.Cargo, out StockpileHaulTask current) &&
            ReferenceEquals(current, task))
        {
            tasks.Remove(task.Cargo);
        }
    }

    private void GenerateTasks()
    {
        if (!TaskController.Instance || !zoneManager || zoneManager.Zones.Count == 0)
        {
            return;
        }

        CargoCrate2D[] cargoObjects = FindObjectsOfType<CargoCrate2D>();
        HashSet<string> occupiedCells = BuildOccupiedCells(cargoObjects);
        for (int i = 0; i < cargoObjects.Length; i++)
        {
            CargoCrate2D cargo = cargoObjects[i];
            if (!cargo || cargo.CurrentStorage || tasks.ContainsKey(cargo) || IsInsideStockpile(cargo))
            {
                continue;
            }

            if (!TryFindBestCell(cargo, occupiedCells, out Zone zone, out Vector2Int cell))
            {
                continue;
            }

            StockpileHaulTask task = new StockpileHaulTask(this, cargo, zone, cell);
            reservedCells.Add(CellKey(zone.id, cell));
            tasks.Add(cargo, task);
            if (!TaskController.Instance.AddTaskToMain(task))
            {
                Release(task);
            }
        }
    }

    private bool TryFindBestCell(CargoCrate2D cargo, HashSet<string> occupiedCells,
        out Zone bestZone, out Vector2Int bestCell)
    {
        bestZone = null;
        bestCell = default;
        float bestScore = float.NegativeInfinity;
        IReadOnlyList<Zone> zones = zoneManager.Zones;
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            if (zone == null || zone.kind != ZoneKind.Stockpile || !zone.settings.enabled ||
                !zone.settings.allowAutomaticTasks ||
                !zone.settings.filter.Accepts(cargo.cargoId, "Cargo"))
            {
                continue;
            }

            foreach (Vector2Int cell in zone.Cells)
            {
                string key = CellKey(zone.id, cell);
                if (reservedCells.Contains(key) || occupiedCells.Contains(key) ||
                    !zoneManager.CanUseCell(zone.region, cell))
                {
                    continue;
                }

                float distance = Vector2.Distance(cargo.transform.position, zone.CellToWorld(cell));
                float score = zone.settings.priority * 10000f - distance;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestZone = zone;
                    bestCell = cell;
                }
            }
        }

        return bestZone != null;
    }

    private HashSet<string> BuildOccupiedCells(CargoCrate2D[] cargoObjects)
    {
        HashSet<string> occupied = new HashSet<string>();
        IReadOnlyList<Zone> zones = zoneManager.Zones;
        for (int i = 0; i < cargoObjects.Length; i++)
        {
            CargoCrate2D cargo = cargoObjects[i];
            if (!cargo) continue;
            for (int j = 0; j < zones.Count; j++)
            {
                Zone zone = zones[j];
                if (zone == null || zone.kind != ZoneKind.Stockpile) continue;
                Vector2Int cell = zoneManager.WorldToCell(zone.region, cargo.transform.position);
                if (zone.Contains(cell) && Vector2.Distance(cargo.transform.position, zone.CellToWorld(cell)) <= 0.6f)
                {
                    occupied.Add(CellKey(zone.id, cell));
                    break;
                }
            }
        }

        return occupied;
    }

    private bool IsInsideStockpile(CargoCrate2D cargo)
    {
        IReadOnlyList<Zone> zones = zoneManager.Zones;
        for (int i = 0; i < zones.Count; i++)
        {
            Zone zone = zones[i];
            if (zone == null || zone.kind != ZoneKind.Stockpile) continue;
            Vector2Int cell = zoneManager.WorldToCell(zone.region, cargo.transform.position);
            if (zone.Contains(cell) && Vector2.Distance(cargo.transform.position, zone.CellToWorld(cell)) <= 0.6f)
            {
                return true;
            }
        }

        return false;
    }

    private void CleanupFinishedTasks()
    {
        if (tasks.Count == 0) return;
        List<StockpileHaulTask> finished = null;
        foreach (StockpileHaulTask task in tasks.Values)
        {
            if (task == null || !task.Cargo || task.State == Constants.TaskState.succeed ||
                task.State == Constants.TaskState.abort)
            {
                finished = finished ?? new List<StockpileHaulTask>();
                finished.Add(task);
            }
        }

        if (finished == null) return;
        for (int i = 0; i < finished.Count; i++) Release(finished[i]);
    }

    private void OnZoneChanged(Zone zone)
    {
        nextScanTime = Mathf.Min(nextScanTime, Time.time + 0.1f);
    }

    private void OnZoneDeleted(Zone zone)
    {
        List<StockpileHaulTask> cancelled = new List<StockpileHaulTask>();
        foreach (StockpileHaulTask task in tasks.Values)
        {
            if (task != null && task.ZoneId == zone.id) cancelled.Add(task);
        }

        for (int i = 0; i < cancelled.Count; i++)
        {
            TaskController.Instance?.CancelTask(cancelled[i]);
            Release(cancelled[i]);
        }
    }

    private static string CellKey(int zoneId, Vector2Int cell)
    {
        return zoneId + ":" + cell.x + ":" + cell.y;
    }
}

public sealed class StockpileHaulTask : AITask
{
    private readonly StockpileZoneTaskGenerator owner;
    private readonly Zone zone;
    private readonly Vector3 targetPosition;
    private Rigidbody2D cargoBody;
    private bool originalSimulated;
    private bool carrying;

    public CargoCrate2D Cargo { get; }
    public int ZoneId => zone.id;
    public Vector2Int TargetCell { get; }
    public override int AIID => 31;
    public override Constants.WorkKind WorkKind => Constants.WorkKind.Collect;
    public override Vector2 TargetPoint => Cargo ? Cargo.transform.position : targetPosition;
    public override object ReservationTarget => Cargo;

    public StockpileHaulTask(StockpileZoneTaskGenerator taskOwner, CargoCrate2D cargo,
        Zone targetZone, Vector2Int targetCell)
    {
        owner = taskOwner;
        Cargo = cargo;
        zone = targetZone;
        TargetCell = targetCell;
        targetPosition = zone.CellToWorld(targetCell);
        RegionId = zone.regionId;
        Level = 10 + zone.settings.priority * 10;
        EstimatedWorkTime = 1f;
        AIActions.Add(new MyGoAction(this, cargo.transform));
        AIActions.Add(new StockpilePickupAction(this));
        AIActions.Add(new StockpileCarryAction(this));
        AIActions.Add(new StockpileDepositAction(this));
    }

    public override bool TryGetTargetPosition(out Vector3 position)
    {
        position = Cargo ? Cargo.transform.position : targetPosition;
        return Cargo;
    }

    public override bool CheckSame(AITask other)
    {
        return base.CheckSame(other) || other is StockpileHaulTask task && task.Cargo == Cargo;
    }

    public bool BeginCarry()
    {
        if (!Cargo || !Person) return false;
        cargoBody = Cargo.GetComponent<Rigidbody2D>();
        if (cargoBody)
        {
            originalSimulated = cargoBody.simulated;
            cargoBody.velocity = Vector2.zero;
            cargoBody.angularVelocity = 0f;
            cargoBody.simulated = false;
        }

        carrying = true;
        FollowPerson();
        return true;
    }

    public void FollowPerson()
    {
        if (carrying && Cargo && Person)
        {
            Cargo.transform.position = Person.transform.position + new Vector3(0f, 0.25f, 0f);
        }
    }

    public MovementStatus MoveToStorage()
    {
        if (!Person || !Person.Navigation) return MovementStatus.Failed;
        FollowPerson();
        return Person.Navigation.Navigate(null, targetPosition, MyGoAction.GetExecuteRange(this),
            RequiresEva, EstimatedWorkTime);
    }

    public bool Deposit()
    {
        if (!Cargo) return false;
        carrying = false;
        Cargo.transform.position = targetPosition;
        RestorePhysics();
        owner?.Complete(this);
        return true;
    }

    public void SuspendCarry()
    {
        carrying = false;
        RestorePhysics();
    }

    public override void Abort()
    {
        SuspendCarry();
        owner?.Release(this);
        base.Abort();
    }

    private void RestorePhysics()
    {
        if (cargoBody)
        {
            cargoBody.simulated = originalSimulated;
            cargoBody.velocity = Vector2.zero;
            cargoBody.angularVelocity = 0f;
        }
    }
}

public sealed class StockpilePickupAction : AIAction
{
    private readonly StockpileHaulTask task;
    public override string Title => "拾取货物";
    public override AIActionState ActionState { get; set; }

    public StockpilePickupAction(StockpileHaulTask haulTask) : base(haulTask)
    {
        task = haulTask;
    }

    public override void Execute()
    {
        ActionState = task.BeginCarry() ? AIActionState.Success : AIActionState.Fail;
    }
}

public sealed class StockpileCarryAction : AIAction
{
    private readonly StockpileHaulTask task;
    public override string Title => "搬运至存放区";
    public override AIActionState ActionState { get; set; }

    public StockpileCarryAction(StockpileHaulTask haulTask) : base(haulTask)
    {
        task = haulTask;
        IsFailWait = true;
    }

    public override void Execute()
    {
        MovementStatus status = task.MoveToStorage();
        if (status == MovementStatus.Arrived) ActionState = AIActionState.Success;
        else if (status == MovementStatus.Failed) ActionState = AIActionState.Fail;
    }

    public override void OnFail()
    {
        task.SuspendCarry();
        base.OnFail();
    }

    public override void OnStop()
    {
        task.SuspendCarry();
        base.OnStop();
    }

    public override void OnRedo()
    {
        ActionState = task.BeginCarry() ? AIActionState.Doing : AIActionState.Fail;
    }
}

public sealed class StockpileDepositAction : AIAction
{
    private readonly StockpileHaulTask task;
    public override string Title => "存放货物";
    public override AIActionState ActionState { get; set; }

    public StockpileDepositAction(StockpileHaulTask haulTask) : base(haulTask)
    {
        task = haulTask;
    }

    public override void Execute()
    {
        ActionState = task.Deposit() ? AIActionState.Success : AIActionState.Fail;
    }
}
