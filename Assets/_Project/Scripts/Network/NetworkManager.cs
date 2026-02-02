using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 씬 전환 시에도 파괴되지 않게 유지
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 서버 접속 시작
        Debug.Log("[네트워크] 서버 접속 시도 중...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // 서버 접속 성공 시 호출되는 콜백
    public override void OnConnectedToMaster()
    {
        Debug.Log("[네트워크] 서버 접속 성공!");
        // 로비 입장
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("[네트워크] 로비 입장 완료. 이제 방에 입장할 수 있습니다.");
        // 테스트를 위해 바로 랜덤 방 입장을 시도합니다.
        PhotonNetwork.JoinRandomRoom();
    }

    // 랜덤 방 입장 실패 시 (방이 없을 때) 호출
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("[네트워크] 방이 없어 새로운 방을 생성합니다.");
        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 2 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[네트워크] 방 입장 성공! 현재 인원: " + PhotonNetwork.CurrentRoom.PlayerCount);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[네트워크] 새로운 플레이어 입장! 현재 인원: " + PhotonNetwork.CurrentRoom.PlayerCount);

        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            Debug.Log("[네트워크] 2명이 모두 모였습니다. 전투를 시작합니다.");
            // 여기서 BattleManager에게 시작 신호를 보내거나 씬을 로드할 수 있습니다.
        }
    }
}
