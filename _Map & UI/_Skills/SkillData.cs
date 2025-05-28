using UnityEngine;

[CreateAssetMenu(fileName = "Skill", menuName = "Skills/CooldownSkill", order = 0)]
public class SkillData : ScriptableObject
{
    public string skillName;
    public KeyCode triggerKey;
    public int cooldownSeconds = 3;
}