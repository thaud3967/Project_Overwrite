using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance; // 어디서든 부를 수 있게 싱글톤 처리

    private Vector3 originalPos;
    private float shakeDuration = 0f;
    private float shakeMagnitude = 0.1f;
    private float dampingSpeed = 2.0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        originalPos = transform.localPosition; // 원래 위치 기억
    }

    private void Update()
    {
        if (shakeDuration > 0)
        {
            // 랜덤한 위치로 카메라를 미친듯이 떨게 만듦
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeMagnitude;

            shakeDuration -= Time.deltaTime * dampingSpeed;
        }
        else
        {
            shakeDuration = 0f;
            transform.localPosition = originalPos; // 흔들림 끝나면 복귀
        }
    }

    // 외부에서 이 함수를 부르면 흔들립니다
    public void Shake(float duration, float magnitude)
    {
        shakeDuration = duration;
        shakeMagnitude = magnitude;
    }
}