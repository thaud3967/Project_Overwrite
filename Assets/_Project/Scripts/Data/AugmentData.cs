using UnityEngine;

public enum AugmentType { StatBoost, SkillMod, MechanismChange }

[CreateAssetMenu(fileName = "AugmentData", menuName = "Data/AugmentData")]
public class AugmentData : ScriptableObject
{
    public int ID;
    public string Name;
    [TextArea]
    public string Description;
    public AugmentType Type;

    [Header("수치 설정 (필요한 것만 사용)")]
    public float AtkPowerBonus; // 예: 0.2 (20% 증가)
    public int ApMaxBonus;      // 예: 1 (최대 AP 1 증가)
    public float HealthBonus;   // 예: 50 (체력 50 증가)
}
