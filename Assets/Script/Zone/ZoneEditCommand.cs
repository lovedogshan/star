using System;
using UnityEngine;

public sealed class ZoneEditCommand : ICommand
{
    private readonly ZoneTypeDefinition type;
    private readonly NavigationRegion region;
    private readonly Vector2Int[] cells;
    private Zone createdZone;
    private bool finished;
    private bool cancelled;

    public string Id { get; } = Guid.NewGuid().ToString("N");
    public CommandType Type => CommandType.Zone;
    public string DisplayName => "创建物资存放区";
    public Person Issuer => null;
    public bool IsQueued => false;
    public bool IsCancelled => cancelled;
    public bool IsFinished => finished || cancelled;
    public AITask Task => null;

    private ZoneEditCommand(ZoneTypeDefinition zoneType,
        NavigationRegion zoneRegion, Vector2Int[] zoneCells)
    {
        type = zoneType;
        region = zoneRegion;
        cells = zoneCells;
    }

    public static ZoneEditCommand CreateCreate(ZoneTypeDefinition type,
        NavigationRegion region, System.Collections.Generic.IEnumerable<Vector2Int> cells)
    {
        return new ZoneEditCommand(type, region,
            new System.Collections.Generic.List<Vector2Int>(cells).ToArray());
    }

    public bool Validate(out string reason)
    {
        if (!ZoneManager.Instance)
        {
            reason = "区域管理器尚未就绪。";
            return false;
        }

        if (!type || type.kind != ZoneKind.Stockpile)
        {
            reason = "当前只支持物资存放区。";
            return false;
        }

        if (cells == null || cells.Length == 0)
        {
            reason = "没有有效的区域格子。";
            return false;
        }

        reason = null;
        return true;
    }

    public CommandResult Execute()
    {
        if (!Validate(out string reason))
        {
            return CommandResult.Failure(reason);
        }

        createdZone = ZoneManager.Instance.CreateZone(type, region, cells);
        if (createdZone == null)
        {
            return CommandResult.Failure("区域内没有可用格子。 ");
        }

        finished = true;
        return CommandResult.Success("物资存放区已创建。");
    }

    public CommandResult Cancel()
    {
        if (createdZone != null && ZoneManager.Instance)
        {
            ZoneManager.Instance.DeleteZone(createdZone.id);
        }

        cancelled = true;
        return CommandResult.Success("区域操作已取消。");
    }
}
