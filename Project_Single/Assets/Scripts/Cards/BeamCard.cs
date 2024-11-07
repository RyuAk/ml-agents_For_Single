using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamCard : MonoBehaviour, ICard
{
    public LayerMask tileLayerMask;
    private bool isCardActive = false;
    private bool isVertical = true;
    private PlayerMove currentPlayer;

    public Material previewMaterial;
    private List<Renderer> previewRenderers = new List<Renderer>();

    private List<PlayerMove> players;

    // ICard 인터페이스 메서드 구현
    public void ActivateEffect(PlayerMove playerMove, List<PlayerMove> allPlayers)
    {
        currentPlayer = playerMove;
        players = allPlayers;
        isCardActive = true;
        Debug.Log("BeamCard가 활성화되었습니다. 타일을 선택하세요.");
    }

    // ICard 인터페이스의 UseCard 메서드 구현
    public void UseCard(PlayerMove playerMove, List<PlayerMove> allPlayers)
    {
        ActivateEffect(playerMove, allPlayers);
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

                    if (Input.GetMouseButtonDown(1))
                    {
                        isVertical = !isVertical;
                        Debug.Log("파괴 방향이 " + (isVertical ? "세로" : "가로") + "로 변경되었습니다.");
                    }
                }
            }
        }
    }

    private void ShowPreview(Tile centerTile)
    {
        if (isVertical)
        {
            AddPreviewTile(centerTile);
            AddPreviewTile(centerTile.GetTileAbove());
            AddPreviewTile(centerTile.GetTileBelow());
        }
        else
        {
            AddPreviewTile(centerTile);
            AddPreviewTile(centerTile.GetTileLeft());
            AddPreviewTile(centerTile.GetTileRight());
        }
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

    private void DestroyTiles(Tile centerTile)
    {
        if (isVertical)
        {
            DestroyTile(centerTile);
            DestroyTile(centerTile.GetTileAbove());
            DestroyTile(centerTile.GetTileBelow());
        }
        else
        {
            DestroyTile(centerTile);
            DestroyTile(centerTile.GetTileLeft());
            DestroyTile(centerTile.GetTileRight());
        }
    }

    private void DestroyTile(Tile tile)
    {
        if (tile != null)
        {
            tile.gameObject.SetActive(false);
        }
    }

    private bool IsPlayerInRange(Tile centerTile)
    {
        List<Tile> tilesToCheck = new List<Tile>();

        if (isVertical)
        {
            tilesToCheck.Add(centerTile);
            tilesToCheck.Add(centerTile.GetTileAbove());
            tilesToCheck.Add(centerTile.GetTileBelow());
        }
        else
        {
            tilesToCheck.Add(centerTile);
            tilesToCheck.Add(centerTile.GetTileLeft());
            tilesToCheck.Add(centerTile.GetTileRight());
        }

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
