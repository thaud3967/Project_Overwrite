using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI 화면")]
    public GameObject titlePanel; // 메인 화면
    public GameObject lobbyPanel; // 로비 화면
    public GameObject roomPanel; 
    [Header("방 만들기")]
    public TMP_InputField roomNameInput; // 방 제목 입력칸

    [Header("방 목록")]
    public Transform contentParent; // ScrollView의 Content (버튼 쌓일 곳)
    public GameObject roomItemPrefab; // 아까 만든 버튼 프리팹

    [Header("대기실 UI")]
    public TMP_Text roomInfoText;
    public Button startButton;

    private void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (PhotonNetwork.IsConnectedAndReady)
        {
            if (!PhotonNetwork.InLobby)
            {
                Debug.Log("이미 접속됨 -> 로비 진입 시도");
                PhotonNetwork.JoinLobby();
            }
            else
            {
                OnJoinedLobby();
            }
        }
        else
        {
            titlePanel.SetActive(true);
            lobbyPanel.SetActive(false);
            roomPanel.SetActive(false);

        }
    }

    // [접속하기] 버튼 누름
    public void Connect()
    {
        if (PhotonNetwork.IsConnected)
            PhotonNetwork.JoinLobby(); // 이미 연결됐으면 바로 로비로
        else
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        // 서버 접속 되면 바로 '로비(대기실)'로 진입
        if (PhotonNetwork.NetworkClientState == ClientState.JoiningLobby || PhotonNetwork.InLobby)
        {
            return;
        }

        Debug.Log("서버 연결 완료 -> 로비 진입 시도");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        // 로비 들어왔으니 화면 전환
        titlePanel.SetActive(false);
        lobbyPanel.SetActive(true);
        roomPanel.SetActive(false);
        Debug.Log("로비 입장 완료! 방 목록을 받아옵니다.");
    }

    // [방 만들기] 버튼 누름
    public void CreateRoom()
    {
        string name = roomNameInput.text;
        if (string.IsNullOrEmpty(name)) name = "Room_" + Random.Range(1, 100);

        // 방 옵션 (최대 2명)
        RoomOptions ro = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(name, ro);
    }

    // [방 버튼] 눌러서 입장 (RoomItem이 호출함)
    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    // 방 입장 성공 -> 게임 시작
    public override void OnJoinedRoom()
    {
        Debug.Log("방 입장! 대기실을 엽니다.");
        lobbyPanel.SetActive(false);
        roomPanel.SetActive(true);

        UpdateRoomUI();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer) { UpdateRoomUI(); }
    public override void OnPlayerLeftRoom(Player otherPlayer) { UpdateRoomUI(); }
    void UpdateRoomUI()
    {
        if (PhotonNetwork.CurrentRoom == null) return;

        int current = PhotonNetwork.CurrentRoom.PlayerCount;
        int max = PhotonNetwork.CurrentRoom.MaxPlayers;

        roomInfoText.text = $"{PhotonNetwork.CurrentRoom.Name} ({current}/{max})";

        // 버튼: 내가 방장(MasterClient)일 때만 시작 버튼 켜기
        startButton.interactable = PhotonNetwork.IsMasterClient;
    }
    public void OnClickStartGame()
    {
        // 방장만 실행 가능
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("BattleScene");
        }
    }
    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }
    // 방 목록 갱신 (누군가 방을 만들거나 없앨 때마다 호출됨)
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        // 기존 목록 싹 지우기
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 새로운 목록 다시 만들기
        foreach (RoomInfo room in roomList)
        {
            // 사라진 방, 꽉 찬 방, 닫힌 방은 건너뜀
            if (room.RemovedFromList || room.PlayerCount >= room.MaxPlayers) continue;

            // 버튼 생성
            GameObject newBtn = Instantiate(roomItemPrefab, contentParent);

            // 버튼 세팅 (텍스트 바꾸기, 클릭 기능 연결)
            RoomItem itemScript = newBtn.GetComponent<RoomItem>();
            itemScript.Setup(room, this);

            // 버튼 컴포넌트 가져와서 클릭 이벤트 연결 (프리팹 단계에서 못 했을 경우를 대비)
            newBtn.GetComponent<Button>().onClick.AddListener(itemScript.OnClickItem);
        }
    }
    public void QuitGame()
    {
        Debug.Log("게임을 종료합니다.");

        // 유니티 에디터에서 플레이 중일 때 -> 플레이 멈춤
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        // 실제 빌드된 게임일 때 -> 프로그램 종료
            Application.Quit();
        #endif
    }
}