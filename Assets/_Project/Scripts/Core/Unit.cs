using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class Unit : MonoBehaviourPun
{
    public string UnitName;
    public float MaxHP = 100;
    public float CurrentHP;
    public int MaxAP = 3;
    public int CurrentAP;

    [Header("능력치 배율")]
    public float damageMultiplier = 1.0f; // 기본 데미지 배율 (1.0 = 100%)
    public float defenseMultiplier = 1.0f; // 받는 데미지 감소율

    // 이 유닛이 내 유닛인지 확인
    public bool IsMine => photonView.IsMine;

    // 네트워크를 통해 유닛의 소유권을 설정하는 함수
    public void SetOwner(Photon.Realtime.Player owner)
    {
        photonView.TransferOwnership(owner);
    }

    public List<StatusEffect> ActiveStatuses = new List<StatusEffect>();
    public bool IsDead => CurrentHP <= 0;

    private void Awake()
    {
        CurrentHP = MaxHP;
        CurrentAP = MaxAP;
    }

    public void TakeDamage(float amount)
    {
        float finalDamage = amount * defenseMultiplier;

        CurrentHP = Mathf.Clamp(CurrentHP - finalDamage, 0, MaxHP);
        Debug.Log($"[데미지] {UnitName}이(가) {finalDamage}의 피해를 입음. 남은 HP: {CurrentHP}");

        if (IsDead) Die();
    }
    public void ApplyAugment(AugmentData data)
    {
        if (data == null) return;

        // 스탯 보너스 적용
        MaxHP += data.HealthBonus;
        CurrentHP = Mathf.Min(CurrentHP + data.HealthBonus, MaxHP); // 늘어난 만큼 회복도 시켜줌
        MaxAP += data.ApMaxBonus;
        CurrentAP = Mathf.Min(CurrentAP + data.ApMaxBonus, MaxAP);
        damageMultiplier += data.AtkPowerBonus;

        Debug.Log($"<color=green>[증강 적용]</color> {UnitName}: 공격력 배율 {damageMultiplier * 100}%");
    }

    public void ConsumeAP(int amount) { CurrentAP -= amount; }
    public void ResetAP() { CurrentAP = MaxAP; }

    public void AddStatus(StatusType type, int duration, float value)
    {
        ActiveStatuses.Add(new StatusEffect(type, duration, value));
    }

    private void Die()
    {
        Debug.Log($"[사망] {UnitName}이(가) 쓰러졌습니다.");

        // 시각적으로만 죽은 것처럼 보이게 합니다.
        // 예: 90도 눕히기
        transform.rotation = Quaternion.Euler(0, 0, 90f);
    }
}
