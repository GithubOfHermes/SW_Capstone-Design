using UnityEngine;

public static class GameStartupInitializer
{
    public static void Initialize()
    {
        Physics2D.IgnoreLayerCollision(
            LayerMask.NameToLayer("Item"),
            LayerMask.NameToLayer("Item"),
            true
        );
    }
}