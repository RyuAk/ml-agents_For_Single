using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PsychokinesisCard : MonoBehaviour
{
    public LayerMask tileLayerMask;  // 타일 레이어 마스크
    private PlayerMove selectedPlayer;
    private List<PlayerMove> players;  // 게임 내 모든 플레이어 리스트

    // 카드를 활성화할 때 지정한 플레이어와 전체 플레이어 리스트를 받아옴
    public void ActivateEffect(PlayerMove playerMove, List<PlayerMove> allPlayers)
    {
        selectedPlayer = playerMove;
        players = allPlayers;
        Debug.Log("PsychokinesisCard가 활성화되었습니다. 플레이어를 이동시킬 타일을 선택하세요.");
    }

    private void Update()
    {
        if (selectedPlayer != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 마우스로 클릭한 타일 감지
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
            {
                Tile selectedTile = hit.collider.GetComponent<Tile>();

                if (selectedTile != null && Input.GetMouseButtonDown(0)) // 타일 클릭 시
                {
                    if (!IsPlayerOnTile(selectedTile))  // 타일에 플레이어가 없으면
                    {
                        MovePlayerToTile(selectedPlayer, selectedTile); // 플레이어를 이동
                        selectedPlayer = null;  // 카드 사용 종료
                    }
                    else
                    {
                        Debug.Log("해당 타일에는 이미 플레이어가 있습니다.");
                    }
                }
            }
        }
    }

    // 해당 타일에 플레이어가 있는지 확인하는 메서드
    private bool IsPlayerOnTile(Tile tile)
    {
        foreach (PlayerMove player in players)
        {
            if (player.CurrentTile == tile)  // 플레이어가 해당 타일에 있는지 확인
            {
                return true;
            }
        }
        return false;
    }

    // 플레이어를 선택한 타일로 이동시키는 메서드
    private void MovePlayerToTile(PlayerMove playerMove, Tile targetTile)
    {
        if (playerMove != null && targetTile != null)
        {
            playerMove.transform.position = targetTile.transform.position;  // 플레이어 위치를 타일 위치로 이동
            playerMove.SetCurrentTile(targetTile);  // 플레이어의 현재 타일을 업데이트
            Debug.Log(playerMove.name + "이(가) " + targetTile.name + "로 이동했습니다.");
        }
    }
}
