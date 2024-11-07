using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class baekGManager : MonoBehaviourPunCallbacks
{
    public static baekGManager instance;
    public GameObject[] players;
    public GameObject currentPlayer;
    private int currentPlayerIndex = 0;

    public const string CurrentTurnPlayerKey = "CurrentTurnPlayer";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        ActivatePlayersBasedOnCount();
        AssignOwnershipToAllPlayers();
        CardManager.instance.DistributeCards(players);
        if (PhotonNetwork.IsMasterClient)
        {
            AssignTurnToPlayer(players[0]); // 첫 번째 플레이어에게 초기 턴 할당
        }
    }

    private void ActivatePlayersBasedOnCount()
    {
        int playerCount = PhotonNetwork.PlayerList.Length;

        for (int i = 0; i < players.Length; i++)
        {
            players[i].SetActive(i < playerCount);
        }
    }

    private void AssignOwnershipToAllPlayers()
    {
        int activePlayerIndex = 0;

        /*foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (activePlayerIndex < players.Length && players[activePlayerIndex].activeSelf)
            {
                PhotonView photonView = players[activePlayerIndex].GetComponent<PhotonView>();

                if (photonView != null)
                {
                    photonView.TransferOwnership(player.ActorNumber);
                }

                activePlayerIndex++;
            }
        }*/

        for (int i = 0; i < players.Length; i++)
        {
            // 활성화되지 않은 오브젝트는 PhotonView 제거 후 다음 오브젝트로 넘어감
            if (!players[i].activeSelf)
            {
                PhotonView photonViewToRemove = players[i].GetComponent<PhotonView>();
                if (photonViewToRemove != null)
                {
                    Destroy(photonViewToRemove);
                }
                continue;
            }

            // 활성화된 오브젝트만 소유권 할당
            if (activePlayerIndex < PhotonNetwork.PlayerList.Length)
            {
                PhotonView photonView = players[i].GetComponent<PhotonView>();

                if (photonView != null)
                {
                    photonView.TransferOwnership(PhotonNetwork.PlayerList[activePlayerIndex].ActorNumber);
                }

                activePlayerIndex++;
            }
        }
    }

    public void EndTurn()
    {
        HidePlayerCards(players[currentPlayerIndex]);
        StartCoroutine(HandleEndTurn());
    }

    public void EndTurnForPlayer(PMove playerMove)
    {
        int index = Array.IndexOf(players, playerMove.gameObject);
        if (index >= 0)
        {
            currentPlayerIndex = index;
            EndTurn();
        }
    }

    public void StartTurnForPlayer(PMove playerMove)
    {
        int index = Array.IndexOf(players, playerMove.gameObject);
        if (index >= 0)
        {
            currentPlayerIndex = index;
            StartPlayerTurn();
        }
    }

    private void AssignTurnToPlayer(GameObject player)
    {
        if (player != null && photonView != null && photonView.Owner != null) //PhotonNetwork.IsMasterClient
        {
            int actorNumber = photonView.Owner.ActorNumber; //player.GetComponent<PhotonView>().Owner.ActorNumber;
            Hashtable turnProps = new Hashtable { { CurrentTurnPlayerKey,actorNumber } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(turnProps);
        }
    }

    private IEnumerator HandleEndTurn()
    {
        yield return new WaitForSeconds(2f);

        if (GetRemainingPlayerCount() == 1)
        {
            Debug.Log("게임 종료! 승자는 " + players[currentPlayerIndex]?.name);
            yield break;
        }

        if(PhotonNetwork.IsMasterClient)
        {
            AdvanceToNextPlayer();
        }

        //AdvanceToNextPlayer();
        //StartPlayerTurn();
    }

    private void AdvanceToNextPlayer()
    {
        /*int originalIndex = currentPlayerIndex;

        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        } while (!players[currentPlayerIndex].activeSelf == true && currentPlayerIndex != originalIndex);

        if (players[currentPlayerIndex] == null)
        {
            Debug.Log("모든 플레이어가 탈락했습니다.");
        }*/

        currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

        AssignTurnToPlayer(players[currentPlayerIndex]);
    }

    public void ScheduleTileCrash(Tile tileToCrash)
    {
        StartCoroutine(HandleTileCrash(tileToCrash));
    }

    private IEnumerator HandleTileCrash(Tile tileToCrash)
    {
        if (tileToCrash == null)
        {
            Debug.LogError("타일이 null입니다!");
            yield break;
        }

        yield return new WaitForSeconds(1f);  // 타일 파괴가 시각적으로 명확해지도록 약간의 지연을 추가

        GameObject meteorPrefab = players[currentPlayerIndex]?.GetComponent<PMove>()?.meteorPrefab;
        Transform meteorParent = players[currentPlayerIndex]?.GetComponent<PMove>()?.meteorParent;

        if (meteorPrefab == null || meteorParent == null)
        {
            Debug.LogError("Meteor prefab 또는 parent가 PlayerMove에 설정되어 있지 않습니다.");
            yield break;
        }

        GameObject meteor = Instantiate(meteorPrefab, meteorParent);
        Vector3 meteorPosition = tileToCrash.transform.position;
        meteorPosition.y += 1;
        meteor.transform.position = meteorPosition;

        photonView.RPC("DestroyTileAcrossClients", RpcTarget.All, tileToCrash.transform.position);

        Destroy(meteor, 2f);
        EndTurn();
    }

    [PunRPC]
    public void DestroyTileAcrossClients(Vector3 tilePosition)
    {
        int clickableLayer = (int)(players[currentPlayerIndex]?.GetComponent<PMove>()?.clickableLayer.value);
        Collider[] hitColliders = Physics.OverlapSphere(tilePosition, 0.1f, clickableLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            Tile tile = hitCollider.GetComponent<Tile>();
            if (tile != null)
            {
                Destroy(tile.gameObject);
            }
        }
    }

    public void StartPlayerTurn()
    {
        if (PhotonNetwork.CurrentRoom != null && PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey(CurrentTurnPlayerKey))
        {
            int currentTurnPlayerActorNumber = (int)PhotonNetwork.CurrentRoom.CustomProperties[CurrentTurnPlayerKey];

            if (PhotonNetwork.LocalPlayer.ActorNumber == currentTurnPlayerActorNumber)
            {
                ShowPlayerCards(players[currentPlayerIndex]);
                PMove currentPlayerMove = players[currentPlayerIndex].GetComponent<PMove>();

                if (currentPlayerMove != null)
                {
                    currentPlayerMove.StartTurn();
                }
            }
            else
            {
                Debug.Log("현재 다른 플레이어의 턴입니다.");
            }
        }
        else
        {
            Debug.LogWarning("CurrentTurnPlayerKey가 설정되지 않았습니다.");
        }
    }

    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.ContainsKey(CurrentTurnPlayerKey))
        {
            int currentTurnPlayerActorNumber = (int)propertiesThatChanged[CurrentTurnPlayerKey];

            foreach (var player in players)
            {
                if (player == null) continue; // player가 null인지 확인하여 null 검사

                if(player != null)
                {
                    PhotonView photonView = player.GetComponent<PhotonView>();
                    PMove playerMove = player.GetComponent<PMove>();

                    if (photonView != null && playerMove != null) 
                    {
                        bool isPlayerTurn = player.GetComponent<PhotonView>().Owner.ActorNumber == currentTurnPlayerActorNumber;  //photonView.Owner != null && photonView.Owner.ActorNumber == currentTurnPlayerActorNumber;
                        playerMove.SetIsPlayerTurn(isPlayerTurn);
                    }
                }
            }

            // 현재 로컬 플레이어가 턴을 가지는지 확인
            if (PhotonNetwork.LocalPlayer.ActorNumber == currentTurnPlayerActorNumber)
            {
                StartPlayerTurn();
            }
        }
    }


    public void RemovePlayer(GameObject player)
    {
        int index = Array.IndexOf(players, player);
        if (index >= 0)
        {
            players[index] = null;
        }
    }

    private int GetRemainingPlayerCount()
    {
        int count = 0;
        foreach (var player in players)
        {
            if (player != null)
            {
                count++;
            }
        }
        return count;
    }

    private void ShowPlayerCards(GameObject player)
    {
        // 플레이어의 카드 UI를 보여주는 로직 구현
    }

    private void HidePlayerCards(GameObject player)
    {
        // 플레이어의 카드 UI를 숨기는 로직 구현
    }
}
