using UnityEngine;

public class Cmd_Attack_Burn : ICommand
{
    public void Execute(Unit caster, Unit target, SkillData data)
    {
        // 시전자의 공격력 배율을 스킬 위력에 곱합니다.
        float finalPower = data.Power * caster.damageMultiplier;

        target.TakeDamage(finalPower);
        target.AddStatus(StatusType.Burn, 3, 5f);
        Debug.Log($"[Command] {data.Name} 실행: 최종 데미지 {finalPower}");
    }
}
