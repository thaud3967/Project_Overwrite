using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Data/SkillData")]
public class SkillData : ScriptableObject
{
    public int ID;
    public string Name;
    public int AP_Cost;
    public float Power;
    public string CommandKey;
}