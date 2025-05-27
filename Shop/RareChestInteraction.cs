using UnityEngine;
using UnityEngine.UI;

public class RareChestInteraction : MonoBehaviour
{
    [Header("Rare Chest 설정")]
    [SerializeField] private GameObject openRareChestPrefab;
    [SerializeField] private GameObject rareChestUI;
    [SerializeField] private Image[] itemSlots; // 카드 3장 슬롯
    [SerializeField] private Sprite[] cardSprites;
    [SerializeField] private Sprite[] EpicCardSprites;
    [SerializeField] private Button[] cardButtons;

    [Header("스탯 텍스트")]
    [SerializeField] private Text playerStatText;
    [SerializeField] private Text playerEpicStatText;

    private bool playerInRange = false;
    private bool isOpened = false;
    private PlayerController player;

    private void Update()
    {
        if (playerInRange && !isOpened && !rareChestUI.activeSelf && Input.GetKeyDown(KeyCode.F))
        {
            OpenRareChestUI();
        }
    }

    private void OpenRareChestUI()
    {
        rareChestUI.SetActive(true);

        if (player == null)
            player = GameObject.FindWithTag("Player")?.GetComponent<PlayerController>();

        if (player != null)
        {
            player.ForceIdleAndStopDash();
            player.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            player.enabled = false;

            // 스탯 텍스트 표시
            if (playerStatText != null)
            {
                float attack = player.GetAttackDamage();
                int skillDamage = player.GetSkill1CurrentDamage();
                int goldGain = player.GetGoldGain();
                playerStatText.text = $"공격력: {attack}\n스킬 데미지: {skillDamage}\n골드 획득량: {goldGain}";
            }

            if (playerEpicStatText != null)
            {
                float damageReduce = PlayerUpgradeData.damageReductionPercent * 100f;
                float cooldownReduce = PlayerUpgradeData.skillCooldownReductionPercent * 100f;
                float dashRefill = PlayerUpgradeData.dashCooldownReduction;
                playerEpicStatText.text = $"피해감소량: {damageReduce:F0}%\n쿨타임 감소: {cooldownReduce:F0}%\n대시 충전: -{dashRefill:F1}초";
            }
        }
        SetRandomRareChestItems();
    }

    private void SetRandomRareChestItems()
    {
        int count = Mathf.Min(3, itemSlots.Length);
        for (int i = 0; i < count; i++)
        {
            bool isEpic = Random.value <= 0.2f;
            Sprite selected;

            if (isEpic)
            {
                var validEpics = new System.Collections.Generic.List<int>();
                for (int j = 0; j < EpicCardSprites.Length; j++)
                {
                    if (
                        (EpicCardSprites[j] == EpicCardSprites[0] && PlayerUpgradeData.damageReductionPercent < 0.5f) ||
                        (EpicCardSprites[j] == EpicCardSprites[1] && PlayerUpgradeData.bonusDashCount < 1) ||
                        (EpicCardSprites[j] == EpicCardSprites[2] && PlayerUpgradeData.dashCooldownReduction < 1.0f) ||
                        (EpicCardSprites[j] == EpicCardSprites[3] && PlayerUpgradeData.skillCooldownReductionPercent < 0.5f)
                    )
                    {
                        validEpics.Add(j);
                    }
                }

                if (validEpics.Count > 0)
                {
                    int selectedIndex = validEpics[Random.Range(0, validEpics.Count)];
                    selected = EpicCardSprites[selectedIndex];
                }
                else
                {
                    selected = cardSprites[Random.Range(0, cardSprites.Length)];
                }
            }
            else
            {
                selected = cardSprites[Random.Range(0, cardSprites.Length)];
            }

            itemSlots[i].sprite = selected;
            itemSlots[i].gameObject.SetActive(true);
            int index = i; // 람다 캡처 방지
            Sprite card = selected; // 캡처용 복사본
            cardButtons[i].interactable = true;
            cardButtons[i].onClick.RemoveAllListeners();
            cardButtons[i].onClick.AddListener(() => OnSelectCard(card));
        }
    }

    private void OnSelectCard(Sprite selectedCard)
    {
        if (selectedCard == cardSprites[0])
        {
            var h = FindAnyObjectByType<HealthUIController>();
            if (h.GetCurrentHP() < h.GetMaxHP()) h.RecoverHP(10);
        }
        else if (selectedCard == cardSprites[1])
        {
            player.IncreaseAttack(5);
            PlayerUpgradeData.bonusAttackDamage += 5f;
        }
        else if (selectedCard == cardSprites[2])
        {
            PlayerUpgradeData.bonusGoldGain += 1;
        }
        else if (selectedCard == cardSprites[3])
        {
            PlayerUpgradeData.bonusMaxHP += 10;
            FindAnyObjectByType<HealthUIController>().IncreaseMaxHP(10);
        }
        else if (selectedCard == cardSprites[4])
        {
            PlayerUpgradeData.bonusSkill1Damage += 5;
        }
        else if (selectedCard == EpicCardSprites[0] && PlayerUpgradeData.damageReductionPercent < 0.5f)
        {
            PlayerUpgradeData.damageReductionPercent += 0.1f;
            PlayerUpgradeData.damageReductionPercent = Mathf.Min(0.5f, PlayerUpgradeData.damageReductionPercent);
        }
        else if (selectedCard == EpicCardSprites[1] && PlayerUpgradeData.bonusDashCount < 1)
        {
            PlayerUpgradeData.bonusDashCount += 1;
            HUDManager.Instance?.InitDashUI();
        }
        else if (selectedCard == EpicCardSprites[2] && PlayerUpgradeData.dashCooldownReduction < 1.0f)
        {
            PlayerUpgradeData.dashCooldownReduction += 0.5f;
            HUDManager.Instance?.InitDashUI();
        }
        else if (selectedCard == EpicCardSprites[3] && PlayerUpgradeData.skillCooldownReductionPercent < 0.5f)
        {
            PlayerUpgradeData.skillCooldownReductionPercent += 0.1f;
            PlayerUpgradeData.skillCooldownReductionPercent = Mathf.Min(0.5f, PlayerUpgradeData.skillCooldownReductionPercent);
        }

        // 이후 UI 닫기 + 상자 오픈 처리 동일
        foreach (var btn in cardButtons)
            btn.interactable = false;

        rareChestUI.SetActive(false);

        if (player != null)
            player.enabled = true;

        Vector3 chestPos = transform.position + new Vector3(0f, 0.09f, 0f);
        GameObject openChest = Instantiate(openRareChestPrefab, chestPos, transform.rotation);
        openChest.transform.localScale = new Vector3(2.0f, 2.0f, 1f);
        Destroy(gameObject);

        isOpened = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            player = other.GetComponent<PlayerController>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
