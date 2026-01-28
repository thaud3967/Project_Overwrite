using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;
    public GameObject restartButton;
    [Header("스테이지 정보")]
    public TextMeshProUGUI stageText;

    [Header("Player 1 상태 (방장)")]
    public Slider p1HpBar;
    public TextMeshProUGUI p1HpText;
    public TextMeshProUGUI p1ApText;
    public TextMeshProUGUI p1NameText;

    [Header("Player 2 상태 (참가자)")]
    public Slider p2HpBar;
    public TextMeshProUGUI p2HpText;
    public TextMeshProUGUI p2ApText;
    public TextMeshProUGUI p2NameText;

    [Header("적 상태")]
    public Slider enemyHpBar;
    public TextMeshProUGUI enemyHpText;
    public TextMeshProUGUI enemyNameText;

    [Header("메시지 및 알림")]
    public TextMeshProUGUI turnMessageText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;


    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (resultPanel != null) resultPanel.SetActive(false);
    }
    public void UpdateStageUI(int stage)
    {
        if (stageText != null)
        {
            stageText.text = $"STAGE {stage}";
        }
    }

    public void UpdateAllUI(Unit p1, Unit p2, Unit enemy)
    {
        UpdateUnitStatus(p1, p1HpBar, p1HpText, p1ApText, p1NameText);
        UpdateUnitStatus(p2, p2HpBar, p2HpText, p2ApText, p2NameText);
        UpdateUnitStatus(enemy, enemyHpBar, enemyHpText, null, enemyNameText);
    }

    private void UpdateUnitStatus(Unit unit, Slider hpBar, TextMeshProUGUI hpText, TextMeshProUGUI apText, TextMeshProUGUI nameText)
    {
        if (unit == null) return;

        if (hpBar != null)
        {
            hpBar.maxValue = unit.MaxHP;
            hpBar.value = unit.CurrentHP;
        }

        if (hpText != null)
            hpText.text = $"{Mathf.CeilToInt(unit.CurrentHP)} / {unit.MaxHP}";

        if (apText != null)
            apText.text = $"AP: {unit.CurrentAP} / {unit.MaxAP}";

        if (nameText != null)
            nameText.text = unit.UnitName;

        if (unit.IsDead && hpBar != null)
        {
            if (hpBar.targetGraphic != null)
            {
                hpBar.targetGraphic.color = Color.gray;
            }
        }
    }

    public void SetTurnMessage(string message)
    {
        if (turnMessageText != null)
        {
            string msg = message switch
            {
                "PlayerTurn" => "아군 턴 - 행동을 선택하세요",
                "EnemyTurn" => "적군 턴 - 방어 중...",
                "Action" => "스킬 발동 중!",
                "AugmentSelect" => "증강을 선택하는 중입니다...",
                _ => message
            };
            turnMessageText.text = msg;
        }
    }

    public void ShowResult(string resultTitle, Color color)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultText.text = resultTitle;
            resultText.color = color;
        }
    }
    public void ShowResult(string resultTitle, Color color, bool showRestart)
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(true);
            resultText.text = resultTitle;
            resultText.color = color;

            // 승리했을 땐 안 보이고, 패배했을 때만 버튼이 보이게 설정 가능
            if (restartButton != null) restartButton.SetActive(showRestart);
        }
    }
    public void OnClickRestart()
    {
        // 방장만 재시작 권한을 갖게 하거나, 로비로 나가게 할 수 있습니다.
        if (Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            BattleManager.Instance.RequestRestart();
        }
    }
    public void HideResult()
    {
        if (resultPanel != null)
        {
            resultPanel.SetActive(false);
        }
    }
}
