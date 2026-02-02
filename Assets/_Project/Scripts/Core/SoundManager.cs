// SoundManager.cs (간단 버전)
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("오디오 클립")]
    public AudioClip bgmBattle;
    public AudioClip sfxHit;
    public AudioClip sfxClick;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        PlayBGM(bgmBattle); // 시작하자마자 배틀 음악
    }

    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}