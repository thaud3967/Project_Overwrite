using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;


public class BattleManager : MonoBehaviourPunCallbacks
{
    public static BattleManager Instance;

    [Header("전투 상태")]
    public BattleState currentState;

    [Header("아군 유닛")]
    public Unit p1Unit; // 방장(MasterClient) 캐릭터
    public Unit p2Unit; // 참가자(Client) 캐릭터

    [Header("적 유닛")]
    public Unit enemyBoss; // AI 보스 몬스터

    // 쿨타임 관리 (플레이어별 분리 혹은 통합 관리 가능)
    private Dictionary<int, int> skillCooldowns = new Dictionary<int, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (PhotonNetwork.IsConnected)
            ChangeState(BattleState.Start);
    }

    // [권한 설정] 방에 입장했을 때 각 플레이어의 유닛 소유권을 확정합니다.
    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[시스템] 방장입니다. P1 캐릭터를 조종합니다.");
        }
        else
        {
            Debug.Log("[시스템] 참가자입니다. P2 캐릭터의 권한을 요청합니다.");
            // P2 유닛의 소유권을 참가자(Client)에게 넘깁니다.
            p2Unit.SetOwner(PhotonNetwork.LocalPlayer);
        }
    }

    private void Update()
    {
        if (currentState != BattleState.PlayerTurn) return;

        // [협동 입력 로직] 내가 주인인 캐릭터의 턴일 때만 조종 가능
        if (PhotonNetwork.IsMasterClient && p1Unit.IsMine)
        {
            HandleInput(1); // 방장 입력 (P1)
        }
        else if (!PhotonNetwork.IsMasterClient && p2Unit.IsMine)
        {
            HandleInput(2); // 참가자 입력 (P2)
        }
    }

    private void HandleInput(int playerNum)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) RequestUseSkill(1101, playerNum);
        if (Input.GetKeyDown(KeyCode.Alpha2)) RequestUseSkill(1102, playerNum);
        if (Input.GetKeyDown(KeyCode.Space)) RequestEndTurn();
    }

    // --- [네트워크] 스킬 사용 요청 ---
    public void RequestUseSkill(int skillID, int playerNum)
    {
        // 쿨타임/AP 체크는 로컬에서 먼저 수행하여 반응속도 확보
        Unit attacker = (playerNum == 1) ? p1Unit : p2Unit;
        SkillData data = SkillManager.Instance.GetSkill(skillID);

        if (attacker.CurrentAP < data.AP_Cost) return;

        // 모든 클라이언트에 스킬 실행 방송
        photonView.RPC("SyncSkill", RpcTarget.All, skillID, playerNum);
    }

    [PunRPC]
    public void SyncSkill(int skillID, int playerNum)
    {
        Unit attacker = (playerNum == 1) ? p1Unit : p2Unit;
        Unit target = enemyBoss; // PvE이므로 타겟은 항상 보스

        SkillData data = SkillManager.Instance.GetSkill(skillID);
        if (data == null) return;

        Debug.Log($"[전투] Player {playerNum}이(가) {data.Name} 사용!");

        ChangeState(BattleState.Action);
        attacker.ConsumeAP(data.AP_Cost);

        ICommand command = CommandFactory.GetCommand(data.CommandKey);
        if (command != null)
        {
            command.Execute(attacker, target, data);
            // UI 업데이트 (P1, P2, 몬스터 전체 상태 갱신)
            BattleUI.Instance.UpdateAllUI(p1Unit, p2Unit, enemyBoss);
            StartCoroutine(PostActionRoutine());
        }
    }

    // --- [네트워크] 턴 종료 로직 ---
    public void RequestEndTurn()
    {
        // 개별 턴 종료가 아니라 팀 전체 턴 종료를 위해 RPC 호출
        photonView.RPC("SyncEndTurn", RpcTarget.All);
    }

    [PunRPC]
    public void SyncEndTurn()
    {
        Debug.Log("[전투] 아군 턴이 종료되었습니다.");
        p1Unit.CurrentAP = 0;
        p2Unit.CurrentAP = 0;
        ChangeState(BattleState.EnemyTurn);
    }

    // --- 상태 관리 ---
    public void ChangeState(BattleState newState)
    {
        currentState = newState;

        // UI에 현재 상태 표시
        if (BattleUI.Instance != null)
            BattleUI.Instance.SetTurnMessage($"{newState}");

        switch (currentState)
        {
            case BattleState.Start:
                // 게임 시작 시 증강 선택 단계로 진입
                ChangeState(BattleState.AugmentSelect);
                break;
            case BattleState.AugmentSelect:
                // 증강 패널 활성화 로직 (추후 구현)
                Debug.Log("[로그라이크] 증강을 선택하세요.");
                // 예시: 두 명 모두 선택 완료 시 PlayerTurn으로 전환
                StartCoroutine(SetupBattle());
                break;
            case BattleState.EnemyTurn:
                StartCoroutine(MonsterAIRoutine());
                break;
        }
    }

    private IEnumerator SetupBattle()
    {
        yield return new WaitForSeconds(1f);
        p1Unit.ResetAP();
        p2Unit.ResetAP();
        ChangeState(BattleState.PlayerTurn);
    }

    private IEnumerator PostActionRoutine()
    {
        yield return new WaitForSeconds(1f);

        if (enemyBoss.IsDead)
        {
            ChangeState(BattleState.Win);
        }
        //  둘 다 죽었을 때만 패배
        else if (p1Unit.IsDead && p2Unit.IsDead)
        {
            ChangeState(BattleState.Lose);
        }
        else
        {
            // 아군 턴 중에 AP가 남은 사람이 있으면 다시 플레이어 턴으로
            if (p1Unit.CurrentAP > 0 || p2Unit.CurrentAP > 0)
                ChangeState(BattleState.PlayerTurn);
            else
                ChangeState(BattleState.EnemyTurn);
        }
    }

    // --- 몬스터 AI (방장만 계산) ---
    private IEnumerator MonsterAIRoutine()
    {
        Debug.Log("[AI] 몬스터 행동 결정 중...");
        yield return new WaitForSeconds(1.5f);

        if (PhotonNetwork.IsMasterClient)
        {
            // 살아있는 타겟 찾기
            List<int> alivePlayers = new List<int>();
            if (!p1Unit.IsDead) alivePlayers.Add(1);
            if (!p2Unit.IsDead) alivePlayers.Add(2);

            if (alivePlayers.Count > 0)
            {
                int targetNum = alivePlayers[Random.Range(0, alivePlayers.Count)];
                // RpcTarget.All을 써서 방장 화면에서도 데미지가 깎입니다.
                photonView.RPC("SyncMonsterAction", RpcTarget.All, targetNum, 10f);
            }
            else
            {
                ChangeState(BattleState.Lose);
            }
        }
    }

    [PunRPC]
    public void SyncMonsterAction(int targetNum, float damage)
    {
        Unit target = (targetNum == 1) ? p1Unit : p2Unit;
        target.TakeDamage(damage);

        // UI 즉시 갱신
        BattleUI.Instance.UpdateAllUI(p1Unit, p2Unit, enemyBoss);

        // 몬스터 공격 연출 후 아군 턴으로 복귀
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(MonsterActionWait());
        }
    }
    private IEnumerator MonsterActionWait()
    {
        yield return new WaitForSeconds(1f);
        p1Unit.ResetAP();
        p2Unit.ResetAP();
        ChangeState(BattleState.PlayerTurn);
    }
}