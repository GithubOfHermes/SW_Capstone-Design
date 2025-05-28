using UnityEngine;
using UnityEngine.UI;

public class ShopBuyButton : MonoBehaviour
{
    public int slotIndex = 0; // 해당 버튼이 어떤 슬롯인지
    private Shop shop;

    private void Start()
    {
        shop = FindAnyObjectByType<Shop>();
        GetComponent<Button>().onClick.AddListener(OnBuy);
    }

    private void OnBuy()
    {
        if (shop == null || slotIndex >= shop.itemSlots.Length) return;

        Sprite selected = shop.itemSlots[slotIndex].sprite;
        bool effectApplied = ShopItemEffectHandler.ApplyEffectForCard(selected, slotIndex);

        if (effectApplied)
        {
            // 해당 슬롯 GameObject 비활성화
            if (shop.itemSlots[slotIndex] != null)
                shop.itemSlots[slotIndex].transform.gameObject.SetActive(false);

            // 플레이어 스탯 텍스트 업데이트
            if (shop.playerStatText != null)
            {
                float attack = shop.playerController.GetAttackDamage();
                int skillDamage = shop.playerController.GetSkill1CurrentDamage();
                int goldGain = shop.playerController.GetGoldGain();
                shop.playerStatText.text = $"공격력: {attack}\n스킬 데미지: {skillDamage}\n골드 획득량: {goldGain}";
            }

            if (shop.playerEpicStatText != null)
            {
                float damageReduce = PlayerUpgradeData.damageReductionPercent * 100f;
                float cooldownReduce = PlayerUpgradeData.skillCooldownReductionPercent * 100f;
                float dashRefill = PlayerUpgradeData.dashCooldownReduction;

                shop.playerEpicStatText.text = $"피해감소량: {damageReduce:F0}%\n쿨타임 감소: {cooldownReduce:F0}%\n대시 충전: -{dashRefill:F1}초";
            }

            // 모든 상품 슬롯이 비활성화 상태인지 확인
            bool allPurchased = true;
            foreach (var slot in shop.itemSlots)
            {
                if (slot.gameObject.activeSelf)
                {
                    allPurchased = false;
                    break;
                }
            }

            // 전부 비활성화 상태면 자동 리롤 (비용 증가 없이)
            if (allPurchased)
            {
                shop.SetRandomShopItems();

                // 가격 텍스트도 다시 세팅
                FindAnyObjectByType<ShopButton>()?.InitializeRerollText();
            }
        }
    }
}
