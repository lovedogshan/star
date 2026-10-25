using System;
using System.Collections.Generic;
using UnityEngine;

public class PathRequestScheduler : MonoBehaviour
{
    private class PathRequest
    {
        public int Id;
        public FindPath Finder;
        public Vector2Int Start;
        public Vector2Int End;
        public int Range;
        public HashSet<Vector2Int> TemporarilyBlocked;
        public Action<bool, List<Vector2Int>> Callback;
    }

    private readonly Queue<PathRequest> emergencyRequests = new Queue<PathRequest>();
    private readonly Queue<PathRequest> normalRequests = new Queue<PathRequest>();
    private readonly HashSet<int> cancelledRequests = new HashSet<int>();
    private int nextRequestId = 1;

    [SerializeField]
    private int maxRequestsPerFrame = 2;

    public static PathRequestScheduler Instance { get; private set; }

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
    }

    private void Update()
    {
        int count = Mathf.Max(1, maxRequestsPerFrame);
        for (int i = 0; i < count; i++)
        {
            PathRequest request = Dequeue();
            if (request == null)
            {
                break;
            }

            if (cancelledRequests.Remove(request.Id))
            {
                continue;
            }

            List<Vector2Int> path = null;
            bool success = request.Finder != null && request.Finder.pathGrid != null &&
                           request.Finder.FindingPath(request.Start, request.End, out path, request.Range,
                               request.TemporarilyBlocked);
            request.Callback?.Invoke(success, success ? path : null);
        }
    }

    public int RequestPath(FindPath finder, Vector2Int start, Vector2Int end, int range,
        bool emergency, Action<bool, List<Vector2Int>> callback)
    {
        return RequestPath(finder, start, end, range, emergency, null, callback);
    }

    public int RequestPath(FindPath finder, Vector2Int start, Vector2Int end, int range,
        bool emergency, ISet<Vector2Int> temporarilyBlocked, Action<bool, List<Vector2Int>> callback)
    {
        PathRequest request = new PathRequest
        {
            Id = nextRequestId++,
            Finder = finder,
            Start = start,
            End = end,
            Range = Mathf.Max(0, range),
            TemporarilyBlocked = temporarilyBlocked != null
                ? new HashSet<Vector2Int>(temporarilyBlocked)
                : null,
            Callback = callback
        };

        (emergency ? emergencyRequests : normalRequests).Enqueue(request);
        return request.Id;
    }

    public void Cancel(int requestId)
    {
        if (requestId > 0)
        {
            cancelledRequests.Add(requestId);
        }
    }

    private PathRequest Dequeue()
    {
        if (emergencyRequests.Count > 0)
        {
            return emergencyRequests.Dequeue();
        }

        return normalRequests.Count > 0 ? normalRequests.Dequeue() : null;
    }
}
