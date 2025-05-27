using UnityEngine;

public static class ShopItemEffectHandler
{
    public static bool ApplyEffectForCard(Sprite card, int slotIndex)
    {
        PlayerController player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();
        if (player == null) return false;

        HealthUIController healthUI = Object.FindFirstObjectByType<HealthUIController>();
        GoldUIController goldUI = Object.FindFirstObjectByType<GoldUIController>();
        Shop shop = Object.FindAnyObjectByType<Shop>();

        if (healthUI == null || goldUI == null || shop == null) return false;

        int price = shop.actualItemPrices[slotIndex];

        // CardSprites[0]에 해당하면 HP +10
        if (card == shop.cardSprites[0])
        {
            // 체력이 가득 차 있으면 구매할 수 없음
            if (healthUI.GetCurrentHP() >= healthUI.GetMaxHP()) return false;

            // 조건: 골드 충분할 경우에만
            if (!goldUI.SpendGold(price)) return false;

            player.RecoverHP(10);
            Debug.Log($"[상점 효과] 체력 10 회복, {price}G 차감");
            return true;
        }

        // CardSprites[1]에 해당하면 공격력 +5
        if (card == shop.cardSprites[1])
        {
            if (!goldUI.SpendGold(price)) return false;

            player.IncreaseAttack(5);
            PlayerUpgradeData.bonusAttackDamage += 5f;
            Debug.Log($"[상점 효과] 공격력 +5, {price}G 차감");
            return true;
        }

        // CardSprites[2]에 해당하면 코인 획득량 +1
        if (card == shop.cardSprites[2])
        {
            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.bonusGoldGain += 1;
            Debug.Log($"[상점 효과] 코인 획득량 +1 증가됨 (총 +{PlayerUpgradeData.bonusGoldGain}), {price}G 차감");
            return true;
        }

        // CardSprites[3] → maxHP +10, currentHP +10 (단, currentHP는 maxHP를 초과할 수 없음)
        if (card == shop.cardSprites[3])
        {
            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.bonusMaxHP += 10;

            healthUI.IncreaseMaxHP(10); // maxHP 증가 및 UI 갱신 포함
            player.IncreaseMaxHP(10);
            Debug.Log($"[상점 효과] 최대 체력 +10 증가됨 (총 +{PlayerUpgradeData.bonusMaxHP}), {price}G 차감");
            return true;
        }

        // CardSprites[4] → Q스킬(Skill_1) 데미지 +5
        if (card == shop.cardSprites[4])
        {
            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.bonusSkill1Damage += 5;
            Debug.Log($"[상점 효과] Q스킬 데미지 +5 (총 +{PlayerUpgradeData.bonusSkill1Damage}), {price}G 차감");
            return true;
        }

        // EpicCardSprites[0] → 피해 10% 감소 (최대 50%)
        if (card == shop.EpicCardSprites[0])
        {
            if (PlayerUpgradeData.damageReductionPercent >= 0.5f) return false;
            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.damageReductionPercent = Mathf.Min(0.5f, PlayerUpgradeData.damageReductionPercent + 0.1f);
            Debug.Log($"[상점 효과] 피해량 {PlayerUpgradeData.damageReductionPercent * 100}% 감소 적용됨, {price}G 차감");

            return true;
        }

        // EpicCardSprites[1] → 대시 횟수 +1 (최대 3회)
        if (card == shop.EpicCardSprites[1])
        {
            if (PlayerUpgradeData.bonusDashCount >= 1) return false; // 이미 증가된 상태

            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.bonusDashCount += 1;

            // 구매 성공 시 energyUI 재초기화
            HUDManager.Instance?.InitDashUI();
            Debug.Log($"[상점 효과] 대시 횟수 +1 (총 {2 + PlayerUpgradeData.bonusDashCount}회), {price}G 차감");

            return true;
        }

        // EpicCardSprites[2] → 대시 쿨타임 -0.5초
        if (card == shop.EpicCardSprites[2])
        {
            if (PlayerUpgradeData.dashCooldownReduction >= 1.0f) return false; // 제한: 1.0초 이상 줄이면 안됨
            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.dashCooldownReduction += 0.5f;

            // HUD 갱신 필요 (쿨타임 변경됨)
            HUDManager.Instance?.InitDashUI();

            Debug.Log($"[상점 효과] 대시 쿨타임 0.5초 감소됨 (총 감소: {PlayerUpgradeData.dashCooldownReduction}초), {price}G 차감");
            return true;
        }

        // EpicCardSprites[3] → 스킬 쿨타임 10% 감소
        if (card == shop.EpicCardSprites[3])
        {
            if (PlayerUpgradeData.skillCooldownReductionPercent >= 0.5f) return false;// 제한: 스킬 쿨타임 감소가 50% 이상이면 안됨
            if (!goldUI.SpendGold(price)) return false;

            PlayerUpgradeData.skillCooldownReductionPercent += 0.1f;
            PlayerUpgradeData.skillCooldownReductionPercent = Mathf.Min(0.5f, PlayerUpgradeData.skillCooldownReductionPercent);

            Debug.Log($"[상점 효과] 스킬 쿨타임 {PlayerUpgradeData.skillCooldownReductionPercent * 100f}% 감소, {price}G 차감");
            return true;
        }
        return false;
    }
}
