using UnityEngine;

public static class PlayerUpgradeData
{
    public static float bonusAttackDamage = 0f;
    public static int bonusGoldGain = 0; 
    public static int bonusMaxHP = 0;
    public static int bonusSkill1Damage = 0;
    
    public static float damageReductionPercent = 0f; // 0.1f가 10% 감소
    public static int bonusDashCount = 0;
    public static float dashCooldownReduction = 0f; // 누적 감소량
    public static float skillCooldownReductionPercent = 0f; // 누적 스킬 쿨타임 감소율 (0.1f가 10% 감소)

    public static void Reset()
    {
        bonusAttackDamage = 0f;
        bonusGoldGain = 0;
        bonusMaxHP = 0;
        bonusSkill1Damage = 0;

        damageReductionPercent = 0f;
        bonusDashCount = 0;
        dashCooldownReduction = 0f;
        skillCooldownReductionPercent = 0f;
    }
}
