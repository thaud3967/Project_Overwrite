using UnityEngine;

public class Cmd_Attack_Burn : ICommand
{
    public void Execute(Unit caster, Unit target, SkillData data)
    {
        target.TakeDamage(data.Power);
        target.AddStatus(StatusType.Burn, 3, 5f);
        Debug.Log($"[Command] {data.Name} 실행: 화상 부여");
    }
}