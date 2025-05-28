using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shop : MonoBehaviour
{
    public GameObject shopUI; 
    public TextMeshProUGUI saleText;
    public Text playerStatText; // 플레이어 스탯 텍스트
    public Text playerEpicStatText; // 플레이어 에픽픽스탯 텍스트

    // 상품 슬롯
    public Image[] itemSlots;             // 상품 이미지 슬롯
    public TextMeshProUGUI[] itemPriceTexts;   // 각 슬롯에 대응되는 버튼의 Text
    public Sprite[] cardSprites;          // 카드 스프라이트
    public Sprite[] EpicCardSprites;      // 에픽카드 스프라이트
    public int[] itemPrices;              // 각 스프라이트에 대응되는 가격
    public int[] EpicItemPrices;          // 에픽 아이템 가격
    public int[] actualItemPrices;        // 차감된 아이템 가격

    [HideInInspector] public int rerollCost = 5; // 리롤 비용

    private bool playerInRange = false;
    private bool itemsInitialized = false; // 초기 아이템 랜덤 설정 여부
    [HideInInspector] public PlayerController playerController;


    public void SetRandomShopItems()
    {
        if (itemSlots.Length != itemPriceTexts.Length) return;
        if (cardSprites.Length != itemPrices.Length) return;

        actualItemPrices = new int[itemSlots.Length]; // 길이 맞춤

        for (int i = 0; i < itemSlots.Length; i++)
        {
            // 비활성화된 슬롯 다시 활성화
            if (!itemSlots[i].gameObject.activeSelf)
                itemSlots[i].gameObject.SetActive(true);

            bool isEpic = Random.value <= 0.2f; // 20% 확률로 에픽 선택
            Sprite selectedSprite;
            int selectedPrice;

            if (isEpic)
            {
                // Epic 카드 등장 필터링 방식
                System.Collections.Generic.List<int> validEpicIndices = new();

                for (int j = 0; j < EpicCardSprites.Length; j++)
                {
                    if (
                        (EpicCardSprites[j] == EpicCardSprites[0] && PlayerUpgradeData.damageReductionPercent < 0.5f) || // EpicCard[0]은 피해 감소 카드 → 50% 이상이면 등장 제외
                        (EpicCardSprites[j] == EpicCardSprites[1] && PlayerUpgradeData.bonusDashCount < 1) || // EpicCard[1]은 대시 횟수 증가 카드 → 대시 +1 이상이면 등장 제외
                        (EpicCardSprites[j] == EpicCardSprites[2] && PlayerUpgradeData.dashCooldownReduction < 1.0f) || // EpicCard[2]은 대시 쿨타임 감소 카드 → 1.0초 이상 감소된 경우 등장 제한
                        (EpicCardSprites[j] == EpicCardSprites[3] && PlayerUpgradeData.skillCooldownReductionPercent < 0.5f) // EpicCard[3]은 스킬 쿨타임 감소 카드 → 50% 이상이면 등장 제외
                    )
                    {
                        validEpicIndices.Add(j);
                    }
                }

                if (validEpicIndices.Count > 0)
                {
                    int selectedEpic = validEpicIndices[Random.Range(0, validEpicIndices.Count)];
                    selectedSprite = EpicCardSprites[selectedEpic];
                    selectedPrice = EpicItemPrices[selectedEpic];
                }
                else
                {
                    // 유효한 에픽 카드가 없다면 일반 카드 강제 사용
                    int normalIndex = Random.Range(0, cardSprites.Length);
                    selectedSprite = cardSprites[normalIndex];
                    selectedPrice = itemPrices[normalIndex];
                }
            }
            else
            {
                int normalIndex = Random.Range(0, cardSprites.Length);
                selectedSprite = cardSprites[normalIndex];
                selectedPrice = itemPrices[normalIndex];
            }

            itemSlots[i].sprite = selectedSprite;

            if (i == 0)
            {
                // 첫 번째 아이템에만 할인 적용용
                int[] discountRates = new int[] { 30, 50, 70 };
                int selectedDiscount = discountRates[Random.Range(0, discountRates.Length)];
                int discountedPrice = Mathf.RoundToInt(selectedPrice * (1f - selectedDiscount / 100f));
                itemPriceTexts[i].text = $"   {discountedPrice}G";
                saleText.text = $"{selectedDiscount}% ";
                actualItemPrices[i] = discountedPrice;
            }
            else
            {
                itemPriceTexts[i].text = $"   {selectedPrice}G";
                actualItemPrices[i] = selectedPrice;
            }
        }
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            bool isActive = shopUI.activeSelf;
            shopUI.SetActive(!isActive);

            if (playerStatText != null)
            {
                float attack = playerController.GetAttackDamage();
                int skillDamage = playerController.GetSkill1CurrentDamage();
                int goldGain = playerController.GetGoldGain();

                playerStatText.text = $"공격력: {attack}\n스킬 데미지: {skillDamage}\n골드 획득량: {goldGain}";
            }

            if (playerEpicStatText != null)
            {
                float damageReduce = PlayerUpgradeData.damageReductionPercent * 100f;
                float cooldownReduce = PlayerUpgradeData.skillCooldownReductionPercent * 100f;
                float dashRefill = PlayerUpgradeData.dashCooldownReduction;

                playerEpicStatText.text = $"피해감소량: {damageReduce:F0}%\n쿨타임 감소: {cooldownReduce:F0}%\n대시 충전: -{dashRefill:F1}초";
            }

            if (playerController != null)
            {
                if (!isActive) // UI가 켜질 때만 IDLE로 강제 전환
                {
                    playerController.ForceIdleAndStopDash();

                    // 이동 멈춤 처리
                    Rigidbody2D rb = playerController.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                    }
                    if (!itemsInitialized)
                    {
                        SetRandomShopItems();
                        itemsInitialized = true; // 한 번만 실행되도록 플래그 설정
                    }
                    FindAnyObjectByType<ShopButton>()?.InitializeRerollText();
                }
                playerController.enabled = isActive; // UI가 켜질 때 비활성화, 꺼질 때 활성화
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerController = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (shopUI.activeSelf)
            {
                shopUI.SetActive(false);
                if (playerController != null)
                    playerController.enabled = true;
            }
            playerController = null;
        }
    }
}
