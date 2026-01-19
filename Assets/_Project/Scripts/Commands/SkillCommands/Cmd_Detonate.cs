using UnityEngine;

public class Cmd_Detonate : ICommand
{
    public void Execute(Unit caster, Unit target, SkillData data)
    {
        StatusEffect burn = target.ActiveStatuses.Find(s => s.Type == StatusType.Burn);
        if (burn != null)
        {
            float bonusDamage = burn.Duration * 10f;
            target.TakeDamage(bonusDamage);
            target.ActiveStatuses.Remove(burn);
            Debug.Log("<color=red>[Command] °ÝÆø ¼º°ø!</color>");
        }
    }
}