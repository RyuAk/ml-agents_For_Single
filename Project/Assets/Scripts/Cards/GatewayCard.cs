using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatewayCard : MonoBehaviour
{
    public void ActivateEffect(PlayerMove playerMove)
    {
        // 이동 가능한 타일을 하이라이트하고, 선택하면 이동
        HighlightValidTiles(playerMove);
    }

    private void HighlightValidTiles(PlayerMove playerMove)
    {
        // 모든 타일 검색 (이 예시에서는 Scene에 있는 모든 Tile 객체를 찾습니다.)
        Tile[] allTiles = FindObjectsOfType<Tile>();
        List<Tile> validTiles = new List<Tile>();

        foreach (Tile tile in allTiles)
        {
            // 타일이 점유되지 않았으며 이미 파괴되지 않은 타일만 유효한 타일로 간주
            if (!tile.IsOccupied() && tile.gameObject.activeSelf)
            {
                validTiles.Add(tile);
                tile.Highlight(playerMove.moveColor); // 이동 가능한 타일 색으로 하이라이트
            }
        }

        // 플레이어의 현재 턴 상태에서만 이동 가능
        if (playerMove.isPlayerTurn)
        {
            // 마우스 클릭 감지를 위해 Update에서 동작 추가
            playerMove.StartCoroutine(WaitForTileSelection(playerMove, validTiles));
        }
    }

    private IEnumerator WaitForTileSelection(PlayerMove playerMove, List<Tile> validTiles)
    {
        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, playerMove.clickableLayer))
                {
                    Tile clickedTile = hit.collider.GetComponent<Tile>();
                    if (clickedTile != null && validTiles.Contains(clickedTile))
                    {
                        // 현재 타일 점유 상태를 해제
                        Tile currentTile = playerMove.CurrentTile;
                        if (currentTile != null)
                        {
                            currentTile.SetOccupied(false);
                        }

                        // 새 타일로 이동
                        playerMove.transform.position = clickedTile.transform.position + Vector3.up;
                        clickedTile.SetOccupied(true);
                        playerMove.SetCurrentTile(clickedTile);

                        // 이동 후 타일 하이라이트 제거
                        foreach (Tile tile in validTiles)
                        {
                            tile.Unhighlight();
                        }

                        // 카드 사용을 이동으로 간주하여 이동 횟수 차감
                        playerMove.movesLeft--;

                        // 카드 사용을 완료하고 코루틴 종료
                        yield break;
                    }
                }
            }
            yield return null; // 다음 프레임까지 대기
        }
    }
}
