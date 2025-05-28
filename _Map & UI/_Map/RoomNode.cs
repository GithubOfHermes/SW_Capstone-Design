using System.Collections.Generic;
using UnityEngine;

public class RoomNode
{
    public Vector2Int Position;
    public Dictionary<Direction, RoomNode> Neighbors = new();
    public bool IsVisited = false;
    public bool IsBossRoom = false;
    public bool IsStartRoom = false;
    public RoomBehavior roomBehavior;

    public RoomNode(Vector2Int pos)
    {
        Position = pos;
    }

    public void Connect(RoomNode target, Direction dir)
    {
        if (!Neighbors.ContainsKey(dir))
            Neighbors[dir] = target;
        if (!target.Neighbors.ContainsKey(dir.GetOpposite()))
            target.Neighbors[dir.GetOpposite()] = this;
    }

    public bool IsClosedRoom()
    {
        return Neighbors.Count == 1;
    }

    public bool HasNeighbor(Direction dir)
    {
        return Neighbors.ContainsKey(dir);
    }
}
