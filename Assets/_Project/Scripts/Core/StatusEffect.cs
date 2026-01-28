using System;
using System.Collections.Generic;
using UnityEngine;

// 상태 이상의 종류 (나중에 엑셀과 맞출 용도)
public enum StatusType { Burn, Poison, Stun }

[Serializable]
public class StatusEffect
{
    public StatusType Type;
    public int Duration;   // 남은 턴
    public float Value;    // 도트 데미지 등 수치

    public StatusEffect(StatusType type, int duration, float value)
    {
        Type = type;
        Duration = duration;
        Value = value;
    }
}
