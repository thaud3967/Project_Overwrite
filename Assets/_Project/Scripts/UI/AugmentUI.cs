using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;

public class AugmentUI : MonoBehaviour
{
    [Header("카드들")]
    public AugmentCard[] cards; // 인스펙터에서 카드 3개를 드래그해서 넣으세요.

    [Header("데이터베이스")]
    public List<AugmentData> allAugments = new List<AugmentData>(); // 모든 증강 리스트

    private void Awake()
    {
        // 리소스 폴더에서 모든 증강 데이터를 자동으로 로드 (폴더 경로 확인 필수)
        allAugments = Resources.LoadAll<AugmentData>("Augments").ToList();
        Debug.Log($"[시스템] 로드된 증강 개수: {allAugments.Count}개");
    }

    // 3개의 랜덤 증강을 화면에 띄우는 함수
    public void ShowRandomAugments()
    {
        if (allAugments == null || allAugments.Count == 0)
        {
            allAugments = Resources.LoadAll<AugmentData>("Augments").ToList();
            Debug.Log($"[시스템] 수동 로드 완료: {allAugments.Count}개");
        }

        if (allAugments.Count < 3)
        {
            Debug.LogError($"증강 데이터가 {allAugments.Count}개뿐입니다! 3개 이상 필요합니다.");
            return;
        }

        // 중복 없이 3개 랜덤 추출
        List<AugmentData> shuffled = allAugments.OrderBy(x => System.Guid.NewGuid()).Take(3).ToList();

        for (int i = 0; i < cards.Length; i++)
        {
            cards[i].Setup(shuffled[i], this);
        }

        gameObject.SetActive(true);
    }

    // 카드가 클릭되었을 때 호출됨
    public void SelectAugment(AugmentData data)
    {
        gameObject.SetActive(false);

        int myPlayerNum = PhotonNetwork.IsMasterClient ? 1 : 2;

        // 모든 클라이언트에게 내가 이 증강을 골랐다고 알림
        BattleManager.Instance.photonView.RPC("SyncAugmentSelection", RpcTarget.All, myPlayerNum, data.ID);
    }
}
