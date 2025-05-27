using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopButton : MonoBehaviour
{
    public Shop shop;                       // Shop 스크립트 참조
    [SerializeField] private TextMeshProUGUI rerollText;     // Reroll 버튼 텍스트
    private int rerollCost = 5;

    public void RerollButton()
    {
        GoldUIController goldUI = FindAnyObjectByType<GoldUIController>();
        if (goldUI == null) return;

        // 골드 부족 → 리롤 불가
        if (goldUI.GetGold() < rerollCost)
        {
            Debug.Log("[상점] 골드 부족으로 리롤 불가");
            return;
        }

        // 골드 차감 성공 시 리롤
        if (goldUI.SpendGold(shop.rerollCost))
        {
            shop.SetRandomShopItems();
            shop.rerollCost = Mathf.Min(shop.rerollCost * 2, 40); // 리롤 비용은 최대 40까지
            rerollText.text = "   " + shop.rerollCost.ToString() + "G";
        }
    }

    public void ExitButton()
    {
        shop.shopUI.SetActive(false);

        // Shop 내부에서 playerController 접근
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            PlayerController controller = playerObj.GetComponent<PlayerController>();
            if (controller != null)
                controller.enabled = true;
        }
    }

    public void InitializeRerollText()
    {
        rerollText.text = "   " + shop.rerollCost.ToString() + "G";
    }
}
