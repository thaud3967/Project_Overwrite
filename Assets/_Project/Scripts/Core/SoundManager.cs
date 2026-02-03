using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("스피커 (AudioSource)")]
    public AudioSource bgmSource; // 배경음악용 스피커 (Loop 켜기!)
    public AudioSource sfxSource; // 효과음용 스피커 (Loop 끄기!)

    [Header("배경음악 (BGM)")]
    public AudioClip bgmBattle;   // 전투 기본 브금
    public AudioClip bgmVictory;  // 승리 브금
    public AudioClip bgmDefeat;   // 패배 브금

    [Header("효과음 (SFX)")]
    public AudioClip sfxHit;      // 타격음
    public AudioClip sfxClick;    // 클릭음

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 씬 넘어가도 음악 안 끊기게 하려면 아래 줄 주석 해제
        // DontDestroyOnLoad(gameObject); 
    }

    private void Start()
    {
        // 게임 시작하면 바로 전투 브금 재생
        PlayBGM(bgmBattle);
    }

    // BGM 교체 함수 (음악을 갈아끼우고 재생)
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 이미 같은 노래가 나오고 있으면 다시 틀지 않음
        if (bgmSource.clip == clip) return;

        bgmSource.Stop(); // 일단 멈추고
        bgmSource.clip = clip; // CD 갈아끼우고
        bgmSource.loop = true; // "반복 재생해" 설정하고 
        bgmSource.Play(); // 재생
    }

    // 효과음 재생 함수 (한 번만 띡 재생)
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}