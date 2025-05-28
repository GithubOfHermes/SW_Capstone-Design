using UnityEngine;
using TMPro;

public class GoldUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;
    private int currentGold = 50;

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public int GetGold()
    {
        return currentGold;
    }

    private void UpdateUI()
    {
        goldText.text = currentGold.ToString();
    }
}