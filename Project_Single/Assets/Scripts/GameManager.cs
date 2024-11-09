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

        // 현재 턴의 플레이어 가져오기
        PlayerAgent currentPlayer = players[currentPlayerIndex];

        // 플레이어가 비활성화되어 있으면 다음 플레이어로 넘어감
        if (currentPlayer == null || !currentPlayer.gameObject.activeSelf)
        {
            MoveToNextPlayer();
            return;
        }

        // 플레이어의 턴 시작
        Debug.Log("Current player turn: " + currentPlayer.gameObject.name);
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

        // 남아 있는 플레이어가 한 명인지 확인
        if (CheckGameOver()) return;

        // 비활성화된 플레이어는 건너뜀
        while (players[currentPlayerIndex] == null || !players[currentPlayerIndex].gameObject.activeSelf)
        {
            currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        }
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

    private bool CheckGameOver()
    {
        int activePlayersCount = 0;
        PlayerAgent lastPlayer = null;

        foreach (var player in players)
        {
            if (player != null && player.gameObject.activeSelf)
            {
                activePlayersCount++;
                lastPlayer = player.GetComponent<PlayerAgent>(); // PlayerAgent 컴포넌트 참조
            }
        }

        if (activePlayersCount == 1 && lastPlayer != null)
        {
            gameOver = true;
            Debug.Log("Game Over! Winner: " + lastPlayer.gameObject.name);

            // 승자에게 큰 보상 부여
            lastPlayer.AddReward(10.0f); // 예: 10점 보상
            lastPlayer.EndEpisode(); // 에피소드 종료

            Invoke(nameof(RestartGameCycle), 5f); // 5초 후 게임 재시작
            return true;
        }

        return false;
    }


    private void RestartGameCycle()
    {
        StartGame();
    }

    private PlayerAgent GetWinner()
    {
        return players.Find(player => player != null && player.isActiveAndEnabled);
    }
}
