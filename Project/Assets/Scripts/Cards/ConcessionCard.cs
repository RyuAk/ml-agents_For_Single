using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICard
{
    void UseCard(PlayerMove playerMove, List<PlayerMove> allPlayers);
}

public class ConcessionCard : MonoBehaviour, ICard
{
    private PlayerMove currentPlayer;
    private List<PlayerMove> players;

    public void UseCard(PlayerMove playerMove, List<PlayerMove> allPlayers)
    {
        currentPlayer = playerMove;
        players = allPlayers;
        Debug.Log("ConcessionCard가 활성화되었습니다. 플레이어를 선택하세요.");
        ActivateEffect();
    }

    private void ActivateEffect()
    {
        // 플레이어 선택 로직 (레이캐스트로 플레이어 선택)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            PlayerMove selectedPlayer = hit.collider.GetComponent<PlayerMove>();

            if (selectedPlayer != null && selectedPlayer != currentPlayer)
            {
                GameManager.instance.EndTurnForPlayer(currentPlayer);
                GameManager.instance.StartTurnForPlayer(selectedPlayer);
                Debug.Log(selectedPlayer.gameObject.name + "의 턴이 시작되었습니다.");
            }
        }
    }
}
