using UnityEngine;
using System.Collections.Generic;

public class PlayerMove : MonoBehaviour
{
    public GameObject meteorPrefab; // 운석 프리팹을 할당할 필드
    public Transform meteorParent; // 운석이 생성될 부모 오브젝트

    public bool isPlayerTurn = false;
    public bool hasMoved = false;
    public Tile currentTile;
    public Color moveHighlightColor;
    private List<Tile> highlightedTiles = new List<Tile>();

    public void StartTurn()
    {
        isPlayerTurn = true;
        hasMoved = false;
        Debug.Log($"{gameObject.name}의 턴이 시작되었습니다.");
        // 필요한 초기화 로직을 여기에 추가할 수 있습니다.
    }

    // 플레이어가 이동할 수 있는 타일을 하이라이트하는 기능
    public void HighlightMovableTiles(Tile[,] tileGrid, int range)
    {
        ClearHighlightedTiles();
        for (int x = currentTile.x - range; x <= currentTile.x + range; x++)
        {
            for (int y = currentTile.y - range; y <= currentTile.y + range; y++)
            {
                if (x >= 0 && x < tileGrid.GetLength(0) && y >= 0 && y < tileGrid.GetLength(1))
                {
                    Tile tile = tileGrid[x, y];
                    if (tile != null && !tile.IsOccupied())
                    {
                        tile.Highlight(moveHighlightColor);
                        highlightedTiles.Add(tile);
                    }
                }
            }
        }
    }

    // 이동 기능
    public void MoveTo(Tile newTile)
    {
        if (!hasMoved && newTile != null && highlightedTiles.Contains(newTile))
        {
            // 현재 타일 점유 해제
            if (currentTile != null)
            {
                currentTile.SetOccupied(false);
            }

            // 새 타일로 이동
            transform.position = new Vector3(newTile.transform.position.x, transform.position.y, newTile.transform.position.z);
            newTile.SetOccupied(true);
            currentTile = newTile;
            hasMoved = true;

            ClearHighlightedTiles();
        }
    }

    // 타일 하이라이트 해제
    private void ClearHighlightedTiles()
    {
        foreach (Tile tile in highlightedTiles)
        {
            tile.Unhighlight();
        }
        highlightedTiles.Clear();
    }

    // 타일 파괴 기능
    public void DestroyTile(Tile targetTile)
    {
        if (targetTile != null && targetTile == currentTile)
        {
            Destroy(targetTile.gameObject);
            hasMoved = true;
        }
    }
}
