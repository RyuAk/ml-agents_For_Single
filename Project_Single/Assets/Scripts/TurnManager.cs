using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TurnManager : MonoBehaviourPunCallbacks
{
    public int totalPlayers;
    private int currentTurnSlotIndex;
    public static TurnManager instance;
    private PhotonView photonview;
    private PMove pMove;

    private void Start()
    {
        photonview = GetComponent<PhotonView>();
        pMove = FindObjectOfType<PMove>();
        totalPlayers = PhotonNetwork.PlayerList.Length;

        if(PhotonNetwork.IsMasterClient)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        currentTurnSlotIndex = 0;
        SetNextTurn();
    }

    void SetNextTurn()
    {
        photonView.RPC("StartPlayerTurn", RpcTarget.All, currentTurnSlotIndex);
    }

    public void EndCurrentTurn()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            currentTurnSlotIndex = (currentTurnSlotIndex) % totalPlayers;
            SetNextTurn();
        }
    }

    [PunRPC]
    public void StartPlayerTurn(int slotINdex)
    {
        // 현재 슬롯 인덱스가 유효한지 확인
        if ((int)PhotonNetwork.LocalPlayer.CustomProperties["SlotNumber"] - 1 == slotINdex)
        {
            // PMove를 찾기 전에 유효성을 체크
            var playerMove = FindObjectOfType<PMove>();
            if (pMove != null)
            {
                pMove.StartTurn();
            }
            else
            {
                Debug.LogError("PlayerMove 객체를 찾을 수 없습니다.");
            }
        }
        EndCurrentTurn();
    }
}
