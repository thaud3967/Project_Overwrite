using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public string UnitName;

    [Header("HP")]
    public float MaxHP = 100;
    public float CurrentHP;

    [Header("AP")]
    public int MaxAP = 3;    // 최대 행동력 (보통 3으로 설정)
    public int CurrentAP;    // 현재 남은 행동력

    public List<StatusEffect> ActiveStatuses = new List<StatusEffect>();

    private void Awake()
    {
        CurrentHP = MaxHP;
        CurrentAP = MaxAP; // 시작 시 AP 풀충전
    }
    
    // 유닛이 죽었는지 확인하는 속성
    public bool IsDead => CurrentHP <= 0;
    public void TakeDamage(float amount)
    {
        CurrentHP -= amount;
        // HP가 0 아래로 내려가지 않도록 제한
        CurrentHP = Mathf.Clamp(CurrentHP, 0, MaxHP);

        Debug.Log($"[데미지] {UnitName}의 남은 HP: {CurrentHP}");

        if (IsDead) Die();
    }

    // AP 소모 함수
    public bool ConsumeAP(int amount)
    {
        if (CurrentAP >= amount)
        {
            CurrentAP -= amount;
            Debug.Log($"[AP] {UnitName}의 AP 소모: {amount}. 남은 AP: {CurrentAP}");
            return true;
        }
        return false;
    }

    // 매 턴 AP 회복 함수
    public void ResetAP()
    {
        CurrentAP = MaxAP;
        Debug.Log($"[AP] {UnitName}의 AP가 충전되었습니다. (현재: {CurrentAP})");
    }

    public void AddStatus(StatusType type, int duration, float value)
    {
        ActiveStatuses.Add(new StatusEffect(type, duration, value));
        Debug.Log($"{UnitName}에게 {type} 상태 이상 부여! ({duration}턴)");
    }

    private void Die()
    {
        Debug.Log($"[사망] {UnitName}이(가) 쓰러졌습니다.");

        gameObject.SetActive(false);
    }
}