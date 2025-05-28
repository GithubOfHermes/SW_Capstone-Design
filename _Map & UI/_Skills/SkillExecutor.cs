using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class SkillExecutor
{
    private readonly SkillData skillData;
    private readonly TextMeshProUGUI textUI;
    private readonly Image skillImage;
    private bool isCoolingDown = false;
    private MonoBehaviour coroutineHost;

    public SkillExecutor(SkillData skillData, TextMeshProUGUI textUI, Image skillImage)
    {
        this.skillData = skillData;
        this.textUI = textUI;
        this.skillImage = skillImage;

        if (textUI != null) textUI.gameObject.SetActive(false);
        coroutineHost = Object.FindFirstObjectByType<SkillCooldownManager>();
    }

    public void ListenForTrigger()
    {
        if (!isCoolingDown && Input.GetKeyDown(skillData.triggerKey))
        {
            isCoolingDown = true;
            coroutineHost.StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        if (textUI == null || skillImage == null)
            yield break;

        textUI.gameObject.SetActive(true);
        textUI.transform.SetAsLastSibling();

        skillImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        float totalCooldown = skillData.cooldownSeconds;
        float reducedCooldown = totalCooldown * (1f - Mathf.Clamp01(PlayerUpgradeData.skillCooldownReductionPercent));
        float timeLeft = reducedCooldown;

        while (timeLeft > 0f)
        {
            textUI.text = timeLeft.ToString("F1") + "s";
            yield return null;
            timeLeft -= Time.deltaTime;
        }

        textUI.gameObject.SetActive(false);
        skillImage.color = Color.white;
        isCoolingDown = false;
    }
}
