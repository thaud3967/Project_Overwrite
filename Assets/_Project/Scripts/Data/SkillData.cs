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

    [Header("연출 설정")]
    public string vfxName; // EffectManager에 등록한 이름
    public AudioClip skillSound; // 스킬 전용 사운드
}
