using UnityEngine;

public class BattleManager : MonoBehaviour
{
    private void Update()
    {
        // 키보드 1번을 누르면 ID 1101 스킬을 사용하는 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseSkill(1101);
        }
    }

    public void UseSkill(int skillID)
    {
        // SkillManager에서 데이터 가져오기
        SkillData data = SkillManager.Instance.GetSkill(skillID);

        if (data != null)
        {
            Debug.Log($"<color=cyan>[전투]</color> {data.Name} 발동! 위력: {data.Power}, 소모 AP: {data.AP_Cost}");
            // 여기서 나중에 Command_Key를 보고 실제 로직(화상, 격폭 등)을 실행하게 됩니다.
        }
    }
}