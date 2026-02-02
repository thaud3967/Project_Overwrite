using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    [Header("스테이지 정보")]
    public int currentStage = 1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // 스테이지 이동 시에도 파괴되지 않게 설정
    }

    // 다음 스테이지로 진입할 때 호출
    public void NextStage()
    {
        currentStage++;
        Debug.Log($"<color=cyan>[스테이지]</color> {currentStage} 스테이지에 진입했습니다!");
    }

    // 현재 스테이지에 맞춰 적의 체력을 계산 (예: 스테이지당 20%씩 증가)
    public float GetScaledHP(float baseHP)
    {
        return baseHP * (1f + (currentStage - 1) * 0.2f);
    }
    public float GetScaledDamage(float baseDamage)
    {
        // 스테이지당 공격력 10%씩 증가
        return baseDamage * (1f + (currentStage - 1) * 0.1f);
    }
    public void ResetStage()
    {
        currentStage = 1;
        Debug.Log("<color=red>[시스템]</color> 스테이지가 초기화되었습니다.");
    }
}
