using UnityEngine;
using UnityEngine.UI;

public class HealthUIController : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Text hpText;

    private int maxHP = 100;
    private int currentHP = 100;

    public int GetCurrentHP() => currentHP;
    public int GetMaxHP() => maxHP;

    public void Init()
    {
        currentHP = maxHP;
        UpdateUI();
    }
    public void SetHP(int current, int max)
    {
        currentHP = current;
        maxHP = max;
        UpdateUI();
    }

    public void ReduceHP(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        UpdateUI();
    }

    public void RecoverHP(int amount)
    {
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateUI();
    }

    public void IncreaseMaxHP(int amount)
    {
        maxHP += amount;
        currentHP = Mathf.Min(currentHP + amount, maxHP);
        UpdateUI();
    }

    private void UpdateUI()
    {
        hpSlider.value = (float)currentHP / maxHP;
        hpText.text = currentHP + " / " + maxHP;
    }
}