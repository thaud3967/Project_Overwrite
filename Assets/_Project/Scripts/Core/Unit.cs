using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public string UnitName;
    public float MaxHP = 100;
    public float CurrentHP;

    // 현재 걸려 있는 상태 이상 리스트
    public List<StatusEffect> ActiveStatuses = new List<StatusEffect>();

    private void Awake()
    {
        CurrentHP = MaxHP;
    }

    // 데미지 입기
    public void TakeDamage(float amount)
    {
        CurrentHP -= amount;
        Debug.Log($"{UnitName}이(가) {amount}의 데미지를 입음. 남은 HP: {CurrentHP}");

        if (CurrentHP <= 0) Die();
    }

    // 상태 이상 추가
    public void AddStatus(StatusType type, int duration, float value)
    {
        ActiveStatuses.Add(new StatusEffect(type, duration, value));
        Debug.Log($"{UnitName}에게 {type} 상태 이상 부여! ({duration}턴)");
    }

    private void Die()
    {
        Debug.Log($"{UnitName} 사망");
    }
}