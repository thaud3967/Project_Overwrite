using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro; // TMP 사용을 위해 필수

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

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

    public void UpdateAllUI(Unit p1, Unit p2, Unit enemy)
    {
        UpdateUnitStatus(p1, p1HpBar, p1HpText, p1ApText, p1NameText);
        UpdateUnitStatus(p2, p2HpBar, p2HpText, p2ApText, p2NameText);
        UpdateUnitStatus(enemy, enemyHpBar, enemyHpText, null, enemyNameText);
    }

    // [중요] 매개변수 타입을 Text -> TextMeshProUGUI로 모두 변경했습니다.
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
            hpBar.targetGraphic.color = Color.gray;
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
}