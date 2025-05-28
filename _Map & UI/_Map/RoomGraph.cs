using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RoomGraph
{
    public Dictionary<Vector2Int, RoomNode> nodes = new();

    public RoomNode CreateRoom(Vector2Int position)
    {
        if (nodes.ContainsKey(position)) return null;
        var newRoom = new RoomNode(position);
        nodes[position] = newRoom;
        return newRoom;
    }

    public bool RoomExists(Vector2Int pos) => nodes.ContainsKey(pos);
    public RoomNode GetRoom(Vector2Int pos) => nodes.TryGetValue(pos, out var node) ? node : null;

    public List<RoomNode> GetShortestPath(RoomNode start, RoomNode end)
    {
        var visited = new HashSet<RoomNode>();
        var queue = new Queue<List<RoomNode>>();
        queue.Enqueue(new List<RoomNode> { start });

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var current = path.Last();
            if (current == end) return path;

            foreach (var neighbor in current.Neighbors.Values)
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    var newPath = new List<RoomNode>(path) { neighbor };
                    queue.Enqueue(newPath);
                }
            }
        }

        return null;
    }
}

