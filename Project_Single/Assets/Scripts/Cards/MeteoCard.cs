using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeteoCard : MonoBehaviour
{
    public LayerMask tileLayerMask;
    private bool isCardActive = false;
    private PlayerMove currentPlayer;

    public Material previewMaterial;
    private List<Renderer> previewRenderers = new List<Renderer>();

    private List<PlayerMove> players;

    public void ActivateEffect(PlayerMove playerMove, List<PlayerMove> allPlayers)
    {
        currentPlayer = playerMove;
        players = allPlayers;
        isCardActive = true;
        Debug.Log("MeteoCard가 활성화되었습니다. 타일을 선택하세요.");
    }

    private void Update()
    {
        if (isCardActive)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
            {
                Tile selectedTile = hit.collider.GetComponent<Tile>();

                if (selectedTile != null)
                {
                    ClearPreview();
                    ShowPreview(selectedTile);

                    if (Input.GetMouseButtonDown(0))
                    {
                        if (!IsPlayerInRange(selectedTile))
                        {
                            DestroyTiles(selectedTile);
                            isCardActive = false;
                            ClearPreview();
                        }
                        else
                        {
                            Debug.Log("범위 내에 플레이어가 있어 카드를 사용할 수 없습니다.");
                        }
                    }
                }
            }
        }
    }

    // 2x2 범위에 있는 타일들을 미리보기로 표시
    private void ShowPreview(Tile centerTile)
    {
        AddPreviewTile(centerTile);
        AddPreviewTile(centerTile.GetTileAbove());
        AddPreviewTile(centerTile.GetTileBelow());
        AddPreviewTile(centerTile.GetTileLeft());
        AddPreviewTile(centerTile.GetTileRight());

        // 대각선 타일들도 미리보기 추가
        AddPreviewTile(centerTile.GetTileAbove()?.GetTileLeft());
        AddPreviewTile(centerTile.GetTileAbove()?.GetTileRight());
        AddPreviewTile(centerTile.GetTileBelow()?.GetTileLeft());
        AddPreviewTile(centerTile.GetTileBelow()?.GetTileRight());
    }

    private void AddPreviewTile(Tile tile)
    {
        if (tile != null)
        {
            Renderer renderer = tile.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = previewMaterial;
                previewRenderers.Add(renderer);
            }
        }
    }

    private void ClearPreview()
    {
        foreach (Renderer renderer in previewRenderers)
        {
            renderer.material = null;
        }
        previewRenderers.Clear();
    }

    // 2x2 범위에 있는 타일들을 파괴
    private void DestroyTiles(Tile centerTile)
    {
        DestroyTile(centerTile);
        DestroyTile(centerTile.GetTileAbove());
        DestroyTile(centerTile.GetTileBelow());
        DestroyTile(centerTile.GetTileLeft());
        DestroyTile(centerTile.GetTileRight());

        // 대각선 타일들도 파괴
        DestroyTile(centerTile.GetTileAbove()?.GetTileLeft());
        DestroyTile(centerTile.GetTileAbove()?.GetTileRight());
        DestroyTile(centerTile.GetTileBelow()?.GetTileLeft());
        DestroyTile(centerTile.GetTileBelow()?.GetTileRight());
    }

    private void DestroyTile(Tile tile)
    {
        if (tile != null)
        {
            tile.gameObject.SetActive(false);
        }
    }

    // 파괴 범위 내에 플레이어가 있는지 확인
    private bool IsPlayerInRange(Tile centerTile)
    {
        List<Tile> tilesToCheck = new List<Tile>
        {
            centerTile,
            centerTile.GetTileAbove(),
            centerTile.GetTileBelow(),
            centerTile.GetTileLeft(),
            centerTile.GetTileRight(),
            centerTile.GetTileAbove()?.GetTileLeft(),
            centerTile.GetTileAbove()?.GetTileRight(),
            centerTile.GetTileBelow()?.GetTileLeft(),
            centerTile.GetTileBelow()?.GetTileRight()
        };

        foreach (Tile tile in tilesToCheck)
        {
            if (tile != null)
            {
                foreach (PlayerMove player in players)
                {
                    if (player.CurrentTile == tile)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
