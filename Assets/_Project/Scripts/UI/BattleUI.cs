using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleUI : MonoBehaviour
{
    public static BattleUI Instance;

    [Header("HP Bars")]
    public Slider playerHPBar;
    public Slider enemyHPBar;

    [Header("AP Status")]
    public TextMeshProUGUI apText;

    [Header("Turn Info")]
    public TextMeshProUGUI turnText;

    [Header("전투 결과 UI")]
    public GameObject resultPanel;      // 결과창 부모 패널
    public TextMeshProUGUI resultText;  // 결과 메시지 (Victory / Defeat)

    private void Awake()
    {
        Instance = this;
    }

    public void ShowResult(string message, Color color)
    {
        resultPanel.SetActive(true);
        resultText.text = message;
        resultText.color = color;
    }

    // 모든 UI를 한 번에 갱신하는 함수
    public void UpdateAllUI(Unit player, Unit enemy)
    {
        UpdateHP(player, enemy);
        UpdateAP(player.CurrentAP, player.MaxAP);
    }

    public void UpdateHP(Unit player, Unit enemy)
    {
        playerHPBar.value = player.CurrentHP / player.MaxHP;
        enemyHPBar.value = enemy.CurrentHP / enemy.MaxHP;
    }

    public void UpdateAP(int current, int max)
    {
        apText.text = $"AP: {current} / {max}";
    }

    public void SetTurnMessage(string message)
    {
        turnText.text = message;
    }
}