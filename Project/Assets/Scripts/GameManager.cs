using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public GameObject[] players;
    public GameObject currentPlayer;
    public int currentPlayerIndex = 0;
    private bool hasMoved = false;


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
         CardManager.instance.DistributeCards(players);
         StartPlayerTurn();

    }

    public void EndTurn()
    {
        HidePlayerCards(players[currentPlayerIndex]);
        // 현재 플레이어의 턴을 종료하고, 다음 플레이어의 턴을 시작
        StartCoroutine(HandleEndTurn());
    }

    public void EndTurnForPlayer(PlayerMove playerMove)
    {
        int index = System.Array.IndexOf(players, playerMove.gameObject);
        if (index >= 0)
        {
            currentPlayerIndex = index;
            EndTurn();
        }
    }

    public void StartTurnForPlayer(PlayerMove playerMove)
    {
        int index = System.Array.IndexOf(players, playerMove.gameObject);
        if (index >= 0)
        {
            currentPlayerIndex = index;
            StartPlayerTurn();
        }
    }

    public void ScheduleTileCrash(Tile tileToCrash)
    {
        // 타일을 부수기 위해서 현재 턴을 종료하고 처리
        StartCoroutine(HandleTileCrash(tileToCrash));
    }

    private IEnumerator HandleEndTurn()
    {
        // 플레이어의 턴이 종료될 때까지 대기 (필요 시 대기 시간을 조절)
        yield return new WaitForSeconds(2f);

        // 남은 플레이어가 한 명인지 확인
        if (GetRemainingPlayerCount() == 1)
        {
            Debug.Log("게임 종료! 승자는 " + players[currentPlayerIndex]?.name);
            // 게임 종료 로직 추가 (예: UI 표시, 게임 상태 변경 등)
            yield break;
        }

        // 다음 플레이어의 턴을 시작
        AdvanceToNextPlayer();
        StartPlayerTurn();
    }

    private void AdvanceToNextPlayer()
    {
        int originalIndex = currentPlayerIndex;

        do
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;
        } while (players[currentPlayerIndex] == null && currentPlayerIndex != originalIndex);

        if (players[currentPlayerIndex] == null)
        {
            Debug.Log("모든 플레이어가 탈락했습니다.");
            // 모든 플레이어가 탈락한 경우 처리할 로직 추가 (게임 종료 등)
        }
    }

    private IEnumerator HandleTileCrash(Tile tileToCrash)
    {
        if (tileToCrash == null)
        {
            Debug.LogError("타일이 null입니다!");
            yield break;
        }

        // 일정 시간 대기 후 타일을 부수고 운석을 생성
        yield return new WaitForSeconds(0f); // 적절한 대기 시간 조절

        // 운석 VFX 생성
        if (tileToCrash != null)
        {
            GameObject meteorPrefab = players[currentPlayerIndex]?.GetComponent<PlayerMove>()?.meteorPrefab;
            Transform meteorParent = players[currentPlayerIndex]?.GetComponent<PlayerMove>()?.meteorParent;

            if (meteorPrefab == null || meteorParent == null)
            {
                Debug.LogError("Meteor prefab 또는 parent가 PlayerMove에 설정되어 있지 않습니다.");
                yield break;
            }

            GameObject meteor = Instantiate(meteorPrefab, meteorParent);
            Vector3 meteorPosition = tileToCrash.transform.position;
            meteorPosition.y += 1; // 선택한 타일에서 Y축으로 1칸 위에 생성
            meteor.transform.position = meteorPosition;

            // 타일 파괴
            Destroy(tileToCrash.gameObject, 2f); // 2초 후 타일 파괴
            Destroy(meteor, 2f); // 2초 후 운석 파괴
        }
        else
        {
            Debug.LogError("타일이 null입니다!");
        }

        // 다음 플레이어의 턴을 시작
        EndTurn();
    }

    public void StartPlayerTurn()
    {
        if (players[currentPlayerIndex] != null)
        {
            hasMoved = false;
            ShowPlayerCards(players[currentPlayerIndex]);
            PlayerMove currentPlayerMove = players[currentPlayerIndex].GetComponent<PlayerMove>();
            if (currentPlayerMove != null)
            {
                currentPlayerMove.StartTurn();
            }
        }
        else
        {
            Debug.LogError("현재 플레이어가 null입니다: " + currentPlayerIndex);
            AdvanceToNextPlayer();
            StartPlayerTurn();
        }
    }

    public void RemovePlayer(GameObject player)
    {
        int index = System.Array.IndexOf(players, player);
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

    public bool CanPlayerMove()
    {
        return !hasMoved; // 플레이어가 아직 움직이지 않은 경우에만 true 반환
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
