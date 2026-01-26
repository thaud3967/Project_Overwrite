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
        CurrentHP = Mathf.Clamp(CurrentHP - amount, 0, MaxHP);
        Debug.Log($"[데미지] {UnitName}의 남은 HP: {CurrentHP}");

        if (IsDead) Die();
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