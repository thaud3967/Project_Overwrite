public enum BattleState
{
    Start,      // 전투 준비 (데이터 로드, 유닛 배치)
    AugmentSelect,
    PlayerTurn, // 플레이어 입력 대기
    Action,     // 스킬 연출 및 로직 실행
    EnemyTurn,  // 적 AI 행동
    Win,        // 승리
    Lose        // 패배
}