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
    [Header("애니메이션")]
    public Animator modelAnimator;
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
        if (BattleUI.Instance != null)
        {
            BattleUI.Instance.ShowDamagePopup(this, finalDamage);
        }
        if (CameraShake.Instance != null)
        {
            float shakePower = (finalDamage > 20) ? 0.3f : 0.1f; // 강약 조절
            float shakeTime = 0.2f; // 0.2초간 흔들림
            CameraShake.Instance.Shake(shakeTime, shakePower);
        }

        //  타격 이펙트 (Hit VFX) 
        //if (EffectManager.Instance != null)
        //{
        //    EffectManager.Instance.PlayVFX("Hit", transform.position + Vector3.up);
        //}

        //  타격 사운드 (Hit SFX)
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySFX(SoundManager.Instance.sfxHit);
        }
        Debug.Log($"[데미지] {UnitName}이(가) {finalDamage}의 피해를 입음. 남은 HP: {CurrentHP}");
        PlayAnim("Hit");
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
    public void PlayAnim(string triggerName)
    {
        if (modelAnimator != null)
        {
            modelAnimator.SetTrigger(triggerName);
        }
    }
    public void ProcessStatusEffects()
    {
        // 리스트를 거꾸로 돌면서 처리
        for (int i = ActiveStatuses.Count - 1; i >= 0; i--)
        {
            StatusEffect effect = ActiveStatuses[i];

            // 효과 발동
            switch (effect.Type)
            {
                case StatusType.Burn:
                    TakeDamage(effect.Value); // 데미지 입기
                    Debug.Log($"[상태이상] {UnitName} 화상 데미지 {effect.Value}!");
                    break;
                case StatusType.Poison:
                    // 독 로직 (기능 추가)
                    break;
            }

            // 턴 감소
            effect.Duration--;

            // 지속시간 끝났으면 제거
            if (effect.Duration <= 0)
            {
                ActiveStatuses.RemoveAt(i);
                Debug.Log($"[상태이상] {UnitName}의 {effect.Type} 효과가 사라졌습니다.");
            }
        }
    }
    private void Die()
    {
        Debug.Log($"[사망] {UnitName}이(가) 쓰러졌습니다.");

        PlayAnim("Die");

        //transform.rotation = Quaternion.Euler(0, 0, 90f);
    }


}
