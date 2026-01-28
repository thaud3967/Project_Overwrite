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
    public Unit p1Unit; // 방장 캐릭터
    public Unit p2Unit; // 참가자 캐릭터

    [Header("적 유닛")]
    public Unit enemyBoss; // AI 보스 몬스터

    [Header("UI 연결")]
    public AugmentUI augmentUI;

    private int playersSelectedCount = 0;
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

        // 현재 방의 인원수를 확인합니다.
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;

        if (playerCount == 1) // 1인 모드 (방장 혼자 조종)
        {
            // P1 조종 (1, 2번 키)
            if (Input.GetKeyDown(KeyCode.Alpha1)) RequestUseSkill(1101, 1);
            if (Input.GetKeyDown(KeyCode.Alpha2)) RequestUseSkill(1102, 1);

            // P2 조종 (3, 4번 키)
            if (Input.GetKeyDown(KeyCode.Alpha3)) RequestUseSkill(1101, 2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) RequestUseSkill(1102, 2);

            if (Input.GetKeyDown(KeyCode.Space)) RequestEndTurn();
        }
        else // 2인 협동 모드
        {
            if (PhotonNetwork.IsMasterClient) HandleInput(1);
            else HandleInput(2);
        }
    }
    [PunRPC]
    public void SyncAugmentSelection(int playerNum, int augmentID)
    {
        // 대상 결정 로직 (P1 또는 P2)
        int actualTargetNum = playerNum;
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            actualTargetNum = playersSelectedCount + 1;
        }
        Unit targetUnit = (actualTargetNum == 1) ? p1Unit : p2Unit;

        // 데이터 찾기
        AugmentData data = augmentUI.allAugments.Find(x => x.ID == augmentID);

        if (data != null && targetUnit != null)
        {
            targetUnit.ApplyAugment(data);
            BattleUI.Instance.UpdateAllUI(p1Unit, p2Unit, enemyBoss);
            Debug.Log($"[네트워크] {targetUnit.UnitName}에게 {data.Name} 적용 완료!");
        }

        // 선택 횟수 증가
        playersSelectedCount++;

        // 다음 단계 및 전투 시작 체크
        if (PhotonNetwork.CurrentRoom.PlayerCount == 1 && playersSelectedCount == 1)
        {
            Debug.Log("P1 적용 완료. 이제 P2 증강을 선택하세요.");
            augmentUI.ShowRandomAugments();
            return;
        }

        if (playersSelectedCount >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            StartCoroutine(SetupBattle());
            playersSelectedCount = 0;
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
    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.LogWarning($"[시스템] {otherPlayer.NickName} 유저가 나갔습니다. 방장이 조종 권한을 이어받습니다.");

        // P2 캐릭터의 권한을 방장(MasterClient)에게 귀속시킵니다.
        if (PhotonNetwork.IsMasterClient)
        {
            p2Unit.SetOwner(PhotonNetwork.LocalPlayer);
        }
    }
    // --- [네트워크] 턴 종료 로직 ---
    public void RequestEndTurn()
    {
        // 개별 턴 종료가 아니라 팀 전체 턴 종료를 위해 RPC 호출
        photonView.RPC("SyncEndTurn", RpcTarget.All);
    }
    private IEnumerator ShowNextAugmentDelay()
    {
        // UI가 완전히 꺼질 시간을 0.2초 정도 줍니다.
        yield return new WaitForSeconds(0.2f);

        if (augmentUI != null)
        {
            augmentUI.ShowRandomAugments(); 
        }
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
                if (augmentUI != null) augmentUI.ShowRandomAugments();
                Debug.Log("[로그라이크] 증강을 선택하세요.");
                // 예시: 두 명 모두 선택 완료 시 PlayerTurn으로 전환
                //StartCoroutine(SetupBattle());
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

        if (CheckBattleResult()) yield break;

        if (p1Unit.CurrentAP > 0 || p2Unit.CurrentAP > 0)
            ChangeState(BattleState.PlayerTurn);
        else
            ChangeState(BattleState.EnemyTurn);
    }

    // --- 몬스터 AI (방장만 계산) ---
    private IEnumerator MonsterAIRoutine()
    {
        Debug.Log("[AI] 몬스터 행동 결정 중...");
        yield return new WaitForSeconds(1.5f);

        // 방장만 누구를 때릴지, 데미지가 얼마일지 결정합니다.
        if (PhotonNetwork.IsMasterClient)
        {
            List<int> alivePlayers = new List<int>();
            if (!p1Unit.IsDead) alivePlayers.Add(1);
            if (!p2Unit.IsDead) alivePlayers.Add(2);

            if (alivePlayers.Count > 0)
            {
                int targetNum = alivePlayers[Random.Range(0, alivePlayers.Count)];

                // 현재 스테이지 배율이 적용된 데미지 계산
                float baseDamage = 10f;
                float finalDamage = StageManager.Instance.GetScaledDamage(baseDamage);

                // 결정된 정보를 모든 클라이언트에게 전송
                photonView.RPC("SyncMonsterAction", RpcTarget.All, targetNum, finalDamage);
            }
            else
            {
                ChangeState(BattleState.Lose);
            }
        }
    }
    private void PrepareNextBattle()
    {
        BattleUI.Instance.HideResult();
        if (StageManager.Instance != null)
        {
            BattleUI.Instance.UpdateStageUI(StageManager.Instance.currentStage);
        }
        // 아군 상태 초기화 (AP만 리셋, 증강 수치는 유지)
        p1Unit.ResetAP();
        p2Unit.ResetAP();

        // 스테이지 클리어 보너스로 체력 20 회복 (최대 체력은 넘지 않게)
        HealUnit(p1Unit, 20f);
        HealUnit(p2Unit, 20f);

        // 적 유닛 부활 및 스탯 스케일링
        enemyBoss.transform.rotation = Quaternion.identity; // 누워있던 보스 세우기

        // 스테이지에 맞춰 체력 설정 (기본 100 기준)
        float scaledHP = StageManager.Instance.GetScaledHP(100f);
        enemyBoss.MaxHP = scaledHP;
        enemyBoss.CurrentHP = scaledHP;

        // UI 갱신
        BattleUI.Instance.UpdateAllUI(p1Unit, p2Unit, enemyBoss);

        // 다시 증강 선택 단계로 돌아감!
        ChangeState(BattleState.AugmentSelect);
    }
    [PunRPC]
    public void SyncMonsterAction(int targetNum, float damage)
    {
        Unit target = (targetNum == 1) ? p1Unit : p2Unit;
        target.TakeDamage(damage);

        // UI 즉시 갱신
        BattleUI.Instance.UpdateAllUI(p1Unit, p2Unit, enemyBoss);
        if (CheckBattleResult()) return;
        // 몬스터 공격 연출 후 아군 턴으로 복귀
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(MonsterActionWait());
        }
    }
    private void HealUnit(Unit unit, float amount)
    {
        if (unit == null || unit.IsDead) return;
        unit.CurrentHP = Mathf.Min(unit.CurrentHP + amount, unit.MaxHP);
    }
    private IEnumerator MonsterActionWait()
    {
        yield return new WaitForSeconds(1f);
        p1Unit.ResetAP();
        p2Unit.ResetAP();
        ChangeState(BattleState.PlayerTurn);
    }
    public void RequestRestart()
    {
        // 포톤의 RPC를 사용해 모든 유저에게 재시작 신호를 보냅니다.
        photonView.RPC("SyncRestart", RpcTarget.All);
    }
    [PunRPC]
    public void SyncRestart()
    {
        // 데이터 초기화
        StageManager.Instance.ResetStage();

        // 현재 씬을 다시 로드 (포톤 전용 함수)
        // 씬 이름을 정확히 적어주세요. 예: "BattleScene"
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
        }
    }
    private bool CheckBattleResult()
    {
        if (enemyBoss.IsDead)
        {
            Debug.Log("<color=gold>[승리]</color>");
            ChangeState(BattleState.Win);
            // 승리 시에는 재시작 버튼 안 보이게 (false)
            BattleUI.Instance.ShowResult("VICTORY", Color.yellow, false);
            StartCoroutine(HandleWinSequence()); // 승리 후 다음 스테이지 이동용
            return true;
        }
        else if (p1Unit.IsDead && p2Unit.IsDead)
        {
            Debug.Log("<color=red>[패배]</color>");
            ChangeState(BattleState.Lose);
            // 패배 시에는 재시작 버튼 보이게 (true)
            BattleUI.Instance.ShowResult("DEFEAT", Color.red, true);
            return true;
        }
        return false; // 아직 승패가 안 남
    }

    // 승리했을 때만 다음 스테이지로 넘어가는 코루틴
    private IEnumerator HandleWinSequence()
    {
        yield return new WaitForSeconds(2f);
        StageManager.Instance.NextStage();
        PrepareNextBattle();
    }
}
