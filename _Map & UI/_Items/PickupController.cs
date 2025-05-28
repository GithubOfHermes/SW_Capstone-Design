using UnityEngine;
using System.Collections.Generic;

public static class PickupController
{
    private static readonly List<ItemPickup> activePickups = new();

    public static void Register(ItemPickup pickup)
    {
        if (!activePickups.Contains(pickup))
            activePickups.Add(pickup);
    }

    public static void Unregister(ItemPickup pickup)
    {
        activePickups.Remove(pickup);
    }

    public static bool IsClosestPickup(Vector2 itemPos, Vector2 playerPos)
    {
        ItemPickup closest = GetClosestPickup(playerPos);
        return closest != null && (Vector2)closest.transform.position == itemPos;
    }

    public static ItemPickup GetClosestPickup(Vector2 playerPos)
    {
        float minDistance = float.MaxValue;
        ItemPickup closest = null;

        foreach (var pickup in activePickups)
        {
            if (pickup == null) continue;
            float dist = Vector2.Distance(pickup.transform.position, playerPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                closest = pickup;
            }
        }

        return closest;
    }
}
