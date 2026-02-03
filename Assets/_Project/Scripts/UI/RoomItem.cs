using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime; 

public class RoomItem : MonoBehaviour
{
    public TMP_Text roomInfoText; 

    private string roomName; // 이 버튼이 담당하는 방 이름
    private LobbyManager manager; // 로비 매니저에게 "나 눌렸어!" 알리기용

    public void Setup(RoomInfo info, LobbyManager lobbyManager)
    {
        manager = lobbyManager;
        roomName = info.Name;

        // 텍스트 표시
        roomInfoText.text = $"{info.Name} ({info.PlayerCount}/{info.MaxPlayers})";
    }

    // 버튼 누르면 실행될 함수 (인스펙터 OnClick에 연결할 것)
    public void OnClickItem()
    {
        manager.JoinRoom(roomName); 
    }
}