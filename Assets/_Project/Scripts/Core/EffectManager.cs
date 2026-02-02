// EffectManager.cs (새로 생성)
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    public static EffectManager Instance;

    [Header("이펙트 프리팹 연결")]
    public GameObject hitVFX;       // 기본 타격 (피 튀기는 것 등)
    public GameObject slashVFX;     // 베기 효과
    public GameObject explosionVFX; // 폭발 효과 (보스용)
    public GameObject healVFX;      // 회복/버프 효과

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayVFX(string vfxName, Vector3 position)
    {
        GameObject prefabToSpawn = null;

        switch (vfxName)
        {
            case "Hit": prefabToSpawn = hitVFX; break;
            case "Slash": prefabToSpawn = slashVFX; break;
            case "Explosion": prefabToSpawn = explosionVFX; break;
            case "Heal": prefabToSpawn = healVFX; break;
        }

        if (prefabToSpawn != null)
        {
            // 이펙트 생성 (보통 1~2초 뒤에 알아서 꺼지는 설정이 프리팹에 되어 있어야 함)
            GameObject vfx = Instantiate(prefabToSpawn, position, Quaternion.identity);

            // 만약 에셋에 자동 파괴 스크립트가 없다면 아래 코드 추가
            Destroy(vfx, 2.0f);
        }
    }
}