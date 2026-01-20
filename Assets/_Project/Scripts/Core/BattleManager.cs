using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;

    [Header("현재 전투 상태")]
    public BattleState currentState;

    [Header("유닛 연결")]
    public Unit playerUnit;
    public Unit enemyUnit;

    // [Key: 스킬ID, Value: 남은 턴 수]
    private Dictionary<int, int> skillCooldowns = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ChangeState(BattleState.Start);
    }

    private void Update()
    {
        if (currentState == BattleState.PlayerTurn)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) UseSkill(1101); // 파이어볼
            if (Input.GetKeyDown(KeyCode.Alpha2)) UseSkill(1102); // 격폭

            // 스페이스바를 누르면 남은 AP와 상관없이 적에게 턴을 넘깁니다.
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[시스템] 플레이어가 수동으로 턴을 종료했습니다.");
                EndTurn();
            }
        }
    }
    // 수동으로 턴을 종료하는 함수
    public void EndTurn()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // 현재 플레이어의 남은 AP를 0으로 만들고 적의 턴으로 넘깁니다.
        playerUnit.CurrentAP = 0;
        BattleUI.Instance.UpdateAP(0, playerUnit.MaxAP); // UI도 0으로 갱신

        ChangeState(BattleState.EnemyTurn);
    }
    public void ChangeState(BattleState newState)
    {
        currentState = newState;

        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.SetTurnMessage($"{newState}");
            BattleUI.Instance.UpdateAllUI(playerUnit, enemyUnit);
        }

        switch (currentState)
        {
            case BattleState.Start:
                StartCoroutine(SetupBattle());
                break;

            case BattleState.PlayerTurn:
                Debug.Log($"[플레이어 턴] 행동 대기 중... (남은 AP: {playerUnit.CurrentAP})");
                break;

            case BattleState.EnemyTurn:
                StartCoroutine(EnemyRoutine());
                break;
            case BattleState.Win:
                Debug.Log("전투에서 승리했습니다!");
                BattleUI.Instance.ShowResult("VICTORY", Color.yellow);
                break;
            case BattleState.Lose:
                Debug.Log("전투에서 패배했습니다...");
                BattleUI.Instance.ShowResult("DEFEAT", Color.red);
                break;
        }
    }

    private IEnumerator SetupBattle()
    {
        yield return new WaitForSeconds(1f);
        playerUnit.ResetAP();
        skillCooldowns.Clear(); // 새로운 전투 시작 시 쿨타임 초기화
        ChangeState(BattleState.PlayerTurn);
    }

    public void UseSkill(int skillID)
    {
        if (currentState != BattleState.PlayerTurn) return;

        // 쿨타임 체크
        if (skillCooldowns.ContainsKey(skillID) && skillCooldowns[skillID] > 0)
        {
            Debug.LogWarning($"[실패] 해당 스킬은 아직 쿨타임 중입니다! (남은 턴: {skillCooldowns[skillID]})");
            return;
        }

        SkillData data = SkillManager.Instance.GetSkill(skillID);
        if (data == null) return;

        // AP 체크
        if (playerUnit.CurrentAP < data.AP_Cost)
        {
            Debug.LogWarning($"[실패] AP가 부족합니다.");
            return;
        }

        // 모든 검증 통과 - 실행 시작
        ChangeState(BattleState.Action);
        playerUnit.ConsumeAP(data.AP_Cost);

        // 쿨타임 등록 (엑셀에 적힌 CoolTime 값 적용)
        if (data.CoolTime > 0)
        {
            skillCooldowns[skillID] = data.CoolTime;
            Debug.Log($"[시스템] {data.Name} 사용! {data.CoolTime}턴의 쿨타임이 적용됩니다.");
        }

        BattleUI.Instance.UpdateAP(playerUnit.CurrentAP, playerUnit.MaxAP);

        ICommand command = CommandFactory.GetCommand(data.CommandKey);
        if (command != null)
        {
            command.Execute(playerUnit, enemyUnit, data);
            BattleUI.Instance.UpdateHP(playerUnit, enemyUnit);
            StartCoroutine(PostActionRoutine());
        }
    }

    // 모든 스킬의 쿨타임을 1씩 줄이는 함수
    private void ReduceCooldowns()
    {
        // Dictionary를 돌면서 0보다 큰 쿨타임을 모두 1씩 뺍니다.
        List<int> keys = new List<int>(skillCooldowns.Keys);
        foreach (int id in keys)
        {
            if (skillCooldowns[id] > 0)
            {
                skillCooldowns[id]--;
            }
        }
    }

    private IEnumerator PostActionRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (enemyUnit.CurrentHP <= 0) ChangeState(BattleState.Win);
        else
        {
            if (playerUnit.CurrentAP > 0) ChangeState(BattleState.PlayerTurn);
            else ChangeState(BattleState.EnemyTurn);
        }
    }

    private IEnumerator EnemyRoutine()
    {
        yield return new WaitForSeconds(1.5f);
        playerUnit.TakeDamage(10);
        BattleUI.Instance.UpdateHP(playerUnit, enemyUnit);

        if (playerUnit.CurrentHP <= 0) ChangeState(BattleState.Lose);
        else
        {
            // 적의 턴이 끝나고 플레이어 턴으로 넘어가기 '직전'에만 쿨타임을 깎습니다.
            ReduceCooldowns();
            Debug.Log("[시스템] 한 라운드가 지나 모든 스킬의 쿨타임이 1턴 감소했습니다.");

            playerUnit.ResetAP();
            ChangeState(BattleState.PlayerTurn);
        }
    }
}