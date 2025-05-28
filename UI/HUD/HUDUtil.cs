using UnityEngine;

public static class HUDUtil
{
    public static void ReduceHP(int amount)
    {
        HUDManager.Instance?.ReduceHP(amount);
    }

    public static void RecoverHP(int amount)
    {
        HUDManager.Instance?.RecoverHP(amount);
    }

    public static int GetCurrentHP()
    {
        return HUDManager.Instance?.GetCurrentHP() ?? 0;
    }

    public static void AddGold(int amount)
    {
        HUDManager.Instance?.AddGold(amount);
    }

    public static bool RequestDash()
    {
        return HUDManager.Instance?.RequestDash() ?? false;
    }

    public static void EndDash()
    {
        HUDManager.Instance?.EndDash();
    }

    public static void ForceUseAndResetEnergy()
    {
        HUDManager.Instance?.ForceUseAndResetEnergy();
    }
}
