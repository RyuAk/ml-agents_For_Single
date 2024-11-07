using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class gameLobby : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private Button startButton;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        //방장이 아니거나 방의 인원이 1명일 때, 버튼 비활성화
        startButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //플레이어 방에 들어올 때 버튼 상태 업데이트
        UpdateStartButton();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //플레이어 방 나갈 때 버튼 상태 업데이트
        UpdateStartButton();
    }

    void UpdateStartButton()
    {
        //방장이면서 방에 2명 이상 있을 때만 버튼 활성화
        startButton.interactable = PhotonNetwork.IsMasterClient && PhotonNetwork.CurrentRoom.PlayerCount > 1;
    }

    public void OnStartButtonClicked()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("GameSence");
        }
    }
}
