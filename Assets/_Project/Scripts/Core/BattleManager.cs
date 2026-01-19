using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public Unit targetEnemy;
    private void Update()
    {
        // 키보드 1번을 누르면 ID 1101 스킬을 사용하는 테스트
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            UseSkill(1101); // 파이어볼
        }

        // 2번 격폭
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            UseSkill(1102); // 격폭
        }
    }

    public void UseSkill(int skillID)
    {
        if (targetEnemy == null) return;

        SkillData data = SkillManager.Instance.GetSkill(skillID);
        if (data == null) return;

        // 데이터에 적힌 키로 명령어를 가져옵니다.
        ICommand command = CommandFactory.GetCommand(data.CommandKey);

        // 명령어가 있다면 실행합니다.
        if (command != null)
        {
            command.Execute(null, targetEnemy, data); // 현재 caster는 임시로 null
        }
        else
        {
            Debug.LogError($"{data.CommandKey}에 해당하는 명령어를 찾을 수 없습니다.");
        }
    }
}