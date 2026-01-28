using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Data/SkillData")]
public class SkillData : ScriptableObject
{
    public int ID;
    public string Name;
    public string Description;
    public int AP_Cost;
    public float Power;
    public int CoolTime;

    public string CommandKey;
}
