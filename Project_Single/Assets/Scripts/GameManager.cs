using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<PlayerAgent> players;
    public int currentPlayerIndex = 0;
    private bool gameOver = false;

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                player.ResetAgent(); // 각 플레이어 에이전트 초기화
            }
        }
        currentPlayerIndex = 0;
        gameOver = false;
        StartNextTurn();
    }

    public void ResetAllPlayers()
    {
        foreach (var player in players)
        {
            if (player != null)
            {
                player.ResetAgent(); // 각 플레이어 초기화
            }
        }
    }

    private void StartNextTurn()
    {
        if (gameOver) return;

        PlayerAgent currentPlayer = players[currentPlayerIndex];

        if (currentPlayer == null || !currentPlayer.gameObject.activeSelf)
        {
            Debug.Log("Skipping inactive player. Moving to next player.");
            MoveToNextPlayer();
            return;
        }

        Debug.Log("Starting turn for player: " + currentPlayer.gameObject.name);
        currentPlayer.StartTurn();
    }

    public void EndTurn()
    {
        MoveToNextPlayer();
        Invoke(nameof(StartNextTurn), 1f); // 1초 딜레이 후 다음 턴 시작
    }

    private void MoveToNextPlayer()
    {
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

        while (players[currentPlayerIndex] == null || !players[currentPlayerIndex].gameObject.activeSelf)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;

            if (CheckGameOver())
            {
                return; // 게임 종료
            }
        }

        Debug.Log("Next player: " + currentPlayerIndex);
    }

    public bool IsTileDestroyed(Vector3 position)
    {
        // 타일이 존재하지 않거나 비활성화된 경우 true 반환
        Collider[] colliders = Physics.OverlapSphere(position, 0.1f);
        foreach (var collider in colliders)
        {
            if (collider.CompareTag("tile") && collider.gameObject.activeSelf)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsPlayerOnTile(Vector3 position)
    {
        // 주어진 위치에 플레이어가 있는지 확인
        foreach (var player in players)
        {
            if (player != null && Vector3.Distance(player.transform.position, position) < 0.1f)
            {
                return true;
            }
        }
        return false;
    }

    public bool CheckGameOver()
    {
        int activePlayersCount = 0;
        PlayerAgent lastPlayer = null;

        foreach (var player in players)
        {
            if (player != null && player.gameObject.activeSelf)
            {
                activePlayersCount++;
                lastPlayer = player;
            }
        }

        if (activePlayersCount == 1)
        {
            gameOver = true;
            Debug.Log("Game Over! Winner: " + lastPlayer.gameObject.name);
            lastPlayer.AddReward(10.0f); // 승리자에게 큰 보상

            // 일정 시간 후 게임을 초기화
            Invoke(nameof(RestartGameCycle), 5f);
            return true;
        }

        return false;
    }


    public void RestartGameCycle()
    {
        TileManager tileManager = FindObjectOfType<TileManager>();
        if (tileManager != null)
        {
            tileManager.ResetTiles(); // 타일 초기화
        }

        // 나머지 초기화 로직은 그대로 유지
        foreach (var player in players)
        {
            if (player != null)
            {
                player.gameObject.SetActive(true);
                player.GetComponent<PlayerAgent>().ResetAgent();
            }
        }

        gameOver = false;
        Debug.Log("Game reset and ready for a new cycle.");
    }

    private PlayerAgent GetWinner()
    {
        return players.Find(player => player != null && player.isActiveAndEnabled);
    }
}
