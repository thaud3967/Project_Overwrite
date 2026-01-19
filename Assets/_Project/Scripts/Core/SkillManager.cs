using UnityEngine;
using System.Collections.Generic;

public class SkillManager : MonoBehaviour
{
    // 싱글톤
    public static SkillManager Instance;

    // ID를 키(Key)로 해서 스킬 데이터를 보관하는 사전(Dictionary)
    private Dictionary<int, SkillData> skillDatabase = new Dictionary<int, SkillData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        LoadAllSkills();
    }

    private void LoadAllSkills()
    {
        // Resources/Skills 폴더에 있는 모든 SkillData 에셋을 로드합니다.
        SkillData[] skills = Resources.LoadAll<SkillData>("Skills");

        // 딕셔너리에 ID별로 저장합니다.
        foreach (var skill in skills)
        {
            if (!skillDatabase.ContainsKey(skill.ID))
            {
                skillDatabase.Add(skill.ID, skill);
                Debug.Log($"[SkillManager] 로드 완료: {skill.Name} (ID: {skill.ID})");
            }
        }

        Debug.Log($"[SkillManager] 총 {skillDatabase.Count}개의 스킬이 데이터베이스에 등록되었습니다.");
    }

    // ID를 주면 스킬 데이터를 반환하는 함수
    public SkillData GetSkill(int id)
    {
        if (skillDatabase.TryGetValue(id, out SkillData skill))
        {
            return skill;
        }
        Debug.LogError($"[SkillManager] ID {id}에 해당하는 스킬을 찾을 수 없습니다.");
        return null;
    }
}