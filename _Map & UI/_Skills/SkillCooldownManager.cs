using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class SkillUIEntry
{
    public SkillData skill;
    public TextMeshProUGUI textUI;
    public Image skillImage;
}

public class SkillCooldownManager : MonoBehaviour
{
    [SerializeField] private List<SkillUIEntry> skillEntries;

    private List<SkillExecutor> skillExecutors = new();

    private void Start()
    {
        foreach (var entry in skillEntries)
        {
            if (entry.skill == null || entry.textUI == null || entry.skillImage == null)
            {
                Debug.LogWarning("[SkillCooldownManager] Skill or Text UI or Skill Image not assigned.");
                continue;
            }

            var executor = new SkillExecutor(entry.skill, entry.textUI, entry.skillImage);
            skillExecutors.Add(executor);
        }
    }

    private void Update()
    {
        foreach (var executor in skillExecutors)
        {
            executor.ListenForTrigger();
        }
    }
}
