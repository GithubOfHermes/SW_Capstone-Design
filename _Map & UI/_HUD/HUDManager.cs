using UnityEngine;
using System.Collections;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance;

    [Header("하위 UI 컨트롤러들")]
    [SerializeField] private HealthUIController healthUI;
    [SerializeField] private GoldUIController goldUI;
    [SerializeField] private EnergyUIController energyUI;
    [SerializeField] private CanvasGroup deadScreenCanvasGroup;
    [SerializeField] private PauseManager pauseManager;


    private PlayerController playerController;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        playerController = FindFirstObjectByType<PlayerController>();
        healthUI.Init();
        InitDashUI();
    }

    public void ReduceHP(int amount) => healthUI.ReduceHP(amount);
    public void RecoverHP(int amount) => healthUI.RecoverHP(amount);
    public int GetCurrentHP() => healthUI.GetCurrentHP();

    public void AddGold(int amount) => goldUI.AddGold(amount);

    public bool RequestDash() => energyUI.RequestDash();
    public void EndDash() => energyUI.EndDash();
    public void ForceUseAndResetEnergy() => energyUI.ForceUseAndResetEnergy();

    public void InitDashUI()
    {
        float dashCooldown = playerController?.GetDashCooldown() ?? 2f;
        int dashCount = 2 + PlayerUpgradeData.bonusDashCount;
        energyUI.Init(dashCooldown, dashCount);
    }

    public void ShowDeadScreen()
    {
        StartCoroutine(FadeInAndRestart());
    }

    private IEnumerator FadeInAndRestart()
    {
        float time = 0f;
        float duration = 1.5f;

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            deadScreenCanvasGroup.alpha = Mathf.Lerp(0f, 1f, time / duration);
            yield return null;
        }

        deadScreenCanvasGroup.alpha = 1f;

        yield return new WaitForSecondsRealtime(2f);

        Time.timeScale = 1f;

        RestartGameManager.Restart("0 Main");
    }

    public void OnPauseButtonPressed()
    {
        pauseManager.Pause();
    }

    public void UpdateHPUI(int current, int max)
    {
        healthUI.SetHP(current, max);
    }
}