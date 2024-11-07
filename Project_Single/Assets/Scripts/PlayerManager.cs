using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    public GameObject[] playerSlots;
    private List<Player> playerList = new List<Player>();

    private void Start()
    {
        UpdatePlayerSlots();
    }

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"plyaer {newPlayer.NickName} has entered the room.");
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"player {otherPlayer.NickName} has left the room.");
        UpdatePlayerList();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("joined room, new checking player list");
        UpdatePlayerList();
    }

    void UpdatePlayerList()
    {
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < players.Length)
            {
                playerSlots[i].SetActive(true);
                playerSlots[i].GetComponentInChildren<Text>().text = players[i].NickName;
                AssignSlotNumber(players[i], i);
            }
            else
            {
                playerSlots[i].SetActive(false);
            }
        }

    }

    void UpdatePlayerSlots()
    {
        //현재 방에 있는 모든 플레이어를 가져옴
        playerList = new List<Player>(PhotonNetwork.PlayerList);

        for (int i = 0; i < playerSlots.Length; i++)
        {
            if (i < playerList.Count)
            {
                //AssignSlotNumber(playerList[i], i);
                playerSlots[i].SetActive(true);
                Text playerNameText = playerSlots[i].GetComponentInChildren<Text>();
                playerNameText.text = playerList[i].NickName;
            }
            else
            {
                playerSlots[i].SetActive(false);
            }
        }
    }

    void AssignSlotNumber(Player player, int slotNumber)
    {
        ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable { { "SlotNumber", slotNumber } };
        player.SetCustomProperties(playerProperties);
    }
}
