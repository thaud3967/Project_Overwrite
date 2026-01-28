public interface ICommand
{
    // 스킬 실행 시 필요한 정보(시전자, 대상, 데이터)를 전달받습니다.
    void Execute(Unit caster, Unit target, SkillData data);
}
