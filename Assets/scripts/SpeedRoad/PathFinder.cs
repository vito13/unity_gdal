using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder{

    Dictionary<long, HashSet<long>> graph = null;
    public void Init(Dictionary<long, HashSet<long>> data)
    {
        graph = data;
    }

    public List<long> FindShortestPath(long start, long end, List<long> path)
    {
        if (path == null)
        {
            path = new List<long>();
        }
        path.Add(start);
        if (start == end)
        {
            return path;
        }

        if (!graph.ContainsKey(start))
        {
            return null;
        }
        List<long> shortest = null;
        foreach (var node in graph[start])
        {
            if (!path.Contains(node))
            {
                var newpath = FindShortestPath(node, end, path);
                if (newpath != null)
                {
                    bool b1 = shortest == null;
                    bool b2 = b1 || newpath.Count < shortest.Count;
                    if (b2)
                    {
                        shortest = newpath;
                    }
                }
            }
        }
        return shortest;
    }

    public List<long> FindPath(long start, long end, List<long> path)
    {
        path.Add(start);
        if (start == end)
        {
            return path;
        }
 
        if (!graph.ContainsKey(start))
        {
            return null;
        }
        foreach (var node in graph[start])
        {
            if (!path.Contains(node))
            {
                var newpath = FindPath(node, end, path);
                if (newpath != null)
                {
                    return newpath;
                }
            }
        }
        return null;
    }

    public List<Vector3> GetPathPoint(List<long> lst, SpeedRoadCrossingMgr crossingMgr)
    {
        List<Vector3> result = new List<Vector3>();
        for (int i = 0; i < lst.Count - 1; i++)
        {
            var r = crossingMgr.GetSectionFrom2Corssing(lst[i], lst[i + 1]);
            result.AddRange(r);
        }
        return result;
    }
}
