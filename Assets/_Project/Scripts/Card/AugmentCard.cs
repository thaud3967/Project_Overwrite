using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AugmentCard : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;

    private AugmentData currentData;
    private AugmentUI parentUI;

    // 카드를 데이터에 맞춰 초기화하는 함수
    public void Setup(AugmentData data, AugmentUI ui)
    {
        currentData = data;
        parentUI = ui;

        nameText.text = data.Name;
        descText.text = data.Description;
    }

    // 버튼의 On Click()에서 호출할 함수
    public void OnClickCard()
    {
        Debug.Log($"[증강] {currentData.Name} 선택됨!");
        parentUI.SelectAugment(currentData);
    }
}
