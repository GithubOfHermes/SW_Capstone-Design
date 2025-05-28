using UnityEngine;

public static class PickupEffectHandler
{
    public static void Apply(ItemPickup.PickupType type, int heal, int gold)
    {
        if (type == ItemPickup.PickupType.Heart && HUDManager.Instance != null)
        {
            PlayerController player = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerController>();
            if (player != null)
            {
                player.RecoverHP(heal); // 체력 직접 회복
            }
        }
        else if (type == ItemPickup.PickupType.Coin && HUDManager.Instance != null)
        {
            int bonus = PlayerUpgradeData.bonusGoldGain;
            HUDUtil.AddGold(gold + bonus); // 코인 획득량 증가 효과 적용
        }
    }
}