using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public List<GameObject> players;
    public int currentPlayerIndex = 0;
    private TileManager tileManager;
    public int boardSize = 10;

    private void Start()
    {
        tileManager = FindObjectOfType<TileManager>();
        StartGame();
    }

    public void StartGame()
    {
        foreach (GameObject player in players)
        {
            player.GetComponent<PlayerController>().ResetMove();
            player.transform.position = GetInitialPosition(player); // 초기 위치로 설정
        }
        currentPlayerIndex = 0;
        Debug.Log("Game started. Current player: " + players[currentPlayerIndex].name);
    }

    public bool IsValidMove(Vector3 position)
    {
        return position.x >= 0 && position.x < boardSize &&
               position.z >= 0 && position.z < boardSize &&
               !tileManager.IsTileDestroyed(position);
    }

    public bool IsDestructible(Vector3 position)
    {
        return IsValidMove(position) && !IsPlayerOnTile(position);
    }

    public void DestroyTile(Vector3 position)
    {
        tileManager.DestroyTile(position);
    }

    public void EndTurn()
    {
        players[currentPlayerIndex].GetComponent<PlayerController>().ResetMove();
        currentPlayerIndex = (currentPlayerIndex + 1) % players.Count;
        Debug.Log("Turn ended. Next player: " + players[currentPlayerIndex].name);
        Invoke(nameof(StartNextTurn), 1f);
    }

    private void StartNextTurn()
    {
        if (CheckGameOver()) return;
        Debug.Log("Next turn started. Current player index: " + currentPlayerIndex);
    }

    private bool CheckGameOver()
    {
        int activePlayers = players.FindAll(player => player != null).Count;
        if (activePlayers == 1)
        {
            Debug.Log("Game over! Winner: " + players[currentPlayerIndex].name);
            Invoke(nameof(RestartGameCycle), 5f);
            return true;
        }
        return false;
    }

    public bool IsPlayerOnTile(Vector3 position)
    {
        foreach (GameObject player in players)
        {
            if (player != null && Vector3.Distance(player.transform.position, position) < 0.1f)
                return true;
        }
        return false;
    }

    private void RestartGameCycle()
    {
        StartGame();
    }

    private Vector3 GetInitialPosition(GameObject player)
    {
        // 각 플레이어의 초기 위치 설정 로직
        return player.transform.position;
    }
}
