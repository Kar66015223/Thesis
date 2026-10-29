using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class DoorRoutePlanner
{
    private const float ClosedDoorCost = 2f;
    private const float CarveSettleTime = 2f;

    private sealed class DoorPoint
    {
        public Transform Transform;
        public Door Door;
        public int OtherSide;
        public int Area = -1;
    }

    private static readonly List<DoorPoint> points = new();
    private static readonly List<int> areaRepresentative = new();
    private static float[,] walkLengths = new float[0, 0];
    private static int graphAreaMask;
    private static float graphBuiltTime = float.NegativeInfinity;

    private static readonly List<float> cost = new();
    private static readonly List<int> previous = new();
    private static readonly List<Door> doorCrossedToReach = new();
    private static readonly List<bool> settled = new();
    private static readonly List<bool> usable = new();

    private static NavMeshPath pathBuffer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        points.Clear();
        areaRepresentative.Clear();
        walkLengths = new float[0, 0];
        graphBuiltTime = float.NegativeInfinity;
    }

    public static bool TryFindFirstDoor(Vector3 from, Vector3 destination, int areaMask, float maxDoorDistance,
        out Door door, out Transform approachPoint)
    {
        door = null;
        approachPoint = null;

        if (WalkLength(from, destination, areaMask) < float.PositiveInfinity)
            return false;

        RefreshGraph(areaMask);

        int startArea = FindArea(from, isStart: true, areaMask);
        int goalArea = FindArea(destination, isStart: false, areaMask);
        if (startArea < 0 || goalArea < 0)
            return false;

        int start = points.Count;
        int goal = points.Count + 1;
        ResetSearch(destination, maxDoorDistance);
        cost[start] = 0f;

        while (true)
        {
            int current = CheapestUnsettled();
            if (current < 0)
                return false;
            if (current == goal)
                break;

            settled[current] = true;

            if (current == start)
            {
                for (int next = 0; next < points.Count; next++)
                {
                    if (usable[next] && points[next].Area == startArea)
                        Relax(start, next, WalkLength(from, points[next].Transform.position, areaMask), null);
                }

                continue;
            }

            DoorPoint point = points[current];
            Vector3 position = point.Transform.position;

            if (point.Area == goalArea)
                Relax(current, goal, WalkLength(position, destination, areaMask), null);

            float crossing = Vector3.Distance(position, points[point.OtherSide].Transform.position);
            if (!point.Door.IsOpen)
                crossing += ClosedDoorCost;

            Relax(current, point.OtherSide, crossing, point.Door);

            for (int next = 0; next < points.Count; next++)
            {
                if (next != current && !settled[next] && usable[next] && points[next].Area == point.Area)
                    Relax(current, next, CachedWalkLength(current, next, areaMask), null);
            }
        }

        for (int step = goal; step != start; step = previous[step])
        {
            Door crossed = doorCrossedToReach[step];
            if (crossed == null || crossed.IsOpen)
                continue;

            door = crossed;
            approachPoint = points[previous[step]].Transform;
        }

        return door != null;
    }

    private static void RefreshGraph(int areaMask)
    {
        float changed = Door.LastRouteChangeTime;
        bool doorsChangedSinceBuild = graphBuiltTime < changed;
        bool builtBeforeCarvingSettled = graphBuiltTime < changed + CarveSettleTime && Time.time >= changed + CarveSettleTime;

        if (!doorsChangedSinceBuild && !builtBeforeCarvingSettled && areaMask == graphAreaMask)
            return;

        graphBuiltTime = Time.time;
        graphAreaMask = areaMask;

        points.Clear();
        foreach (Door door in Door.ActiveDoors)
        {
            if (door == null || door.Root != door || door.PlayerPointFront == null || door.PlayerPointBack == null)
                continue;

            int front = points.Count;
            points.Add(new DoorPoint { Transform = door.PlayerPointFront, Door = door, OtherSide = front + 1 });
            points.Add(new DoorPoint { Transform = door.PlayerPointBack, Door = door, OtherSide = front });
        }

        walkLengths = new float[points.Count, points.Count];
        for (int a = 0; a < points.Count; a++)
        {
            for (int b = 0; b < points.Count; b++)
                walkLengths[a, b] = a == b ? 0f : float.NaN;
        }

        areaRepresentative.Clear();
        for (int seed = 0; seed < points.Count; seed++)
        {
            if (points[seed].Area >= 0)
                continue;

            int area = areaRepresentative.Count;
            areaRepresentative.Add(seed);
            points[seed].Area = area;

            for (int other = seed + 1; other < points.Count; other++)
            {
                if (points[other].Area < 0 && CachedWalkLength(seed, other, areaMask) < float.PositiveInfinity)
                    points[other].Area = area;
            }
        }
    }

    private static int FindArea(Vector3 position, bool isStart, int areaMask)
    {
        for (int area = 0; area < areaRepresentative.Count; area++)
        {
            Vector3 representative = points[areaRepresentative[area]].Transform.position;
            float length = isStart
                ? WalkLength(position, representative, areaMask)
                : WalkLength(representative, position, areaMask);

            if (length < float.PositiveInfinity)
                return area;
        }

        return -1;
    }

    private static void ResetSearch(Vector3 destination, float maxDoorDistance)
    {
        int count = points.Count + 2;
        Fill(cost, count, float.PositiveInfinity);
        Fill(previous, count, -1);
        Fill(doorCrossedToReach, count, null);
        Fill(settled, count, false);
        Fill(usable, count, true);

        for (int i = 0; i < points.Count; i++)
        {
            Door door = points[i].Door;
            usable[i] = door != null && Vector3.Distance(door.transform.position, destination) <= maxDoorDistance;
        }
    }

    private static void Fill<T>(List<T> list, int count, T value)
    {
        list.Clear();
        for (int i = 0; i < count; i++)
            list.Add(value);
    }

    private static int CheapestUnsettled()
    {
        int cheapest = -1;
        for (int i = 0; i < cost.Count; i++)
        {
            if (!settled[i] && cost[i] < float.PositiveInfinity && (cheapest < 0 || cost[i] < cost[cheapest]))
                cheapest = i;
        }

        return cheapest;
    }

    private static void Relax(int from, int to, float edgeCost, Door crossedDoor)
    {
        float newCost = cost[from] + edgeCost;
        if (newCost >= cost[to])
            return;

        cost[to] = newCost;
        previous[to] = from;
        doorCrossedToReach[to] = crossedDoor;
    }

    private static float CachedWalkLength(int a, int b, int areaMask)
    {
        float length = walkLengths[a, b];
        if (float.IsNaN(length))
        {
            length = WalkLength(points[a].Transform.position, points[b].Transform.position, areaMask);
            walkLengths[a, b] = length;
            walkLengths[b, a] = length;
        }

        return length;
    }

    private static float WalkLength(Vector3 from, Vector3 to, int areaMask)
    {
        pathBuffer ??= new NavMeshPath();
        if (!NavMesh.CalculatePath(from, to, areaMask, pathBuffer) || pathBuffer.status != NavMeshPathStatus.PathComplete)
            return float.PositiveInfinity;

        Vector3[] corners = pathBuffer.corners;
        float length = 0f;
        for (int i = 1; i < corners.Length; i++)
            length += Vector3.Distance(corners[i - 1], corners[i]);

        return length;
    }
}
