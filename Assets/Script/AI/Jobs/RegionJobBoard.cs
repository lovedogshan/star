using System.Collections.Generic;
using static Constants;

public class RegionJobBoard
{
    private readonly Dictionary<WorkKind, List<AITask>> jobsByKind = new Dictionary<WorkKind, List<AITask>>();

    public int RegionId { get; }

    public RegionJobBoard(int regionId)
    {
        RegionId = regionId;
    }

    public void Add(AITask task)
    {
        if (task == null)
        {
            return;
        }

        if (!jobsByKind.TryGetValue(task.WorkKind, out List<AITask> jobs))
        {
            jobs = new List<AITask>();
            jobsByKind.Add(task.WorkKind, jobs);
        }

        if (!jobs.Contains(task))
        {
            jobs.Add(task);
        }
    }

    public void Remove(AITask task)
    {
        if (task != null && jobsByKind.TryGetValue(task.WorkKind, out List<AITask> jobs))
        {
            jobs.Remove(task);
        }
    }

    public void Collect(List<AITask> output)
    {
        foreach (List<AITask> jobs in jobsByKind.Values)
        {
            output.AddRange(jobs);
        }
    }

    public void Collect(WorkKind workKind, List<AITask> output)
    {
        if (jobsByKind.TryGetValue(workKind, out List<AITask> jobs))
        {
            output.AddRange(jobs);
        }
    }
}
