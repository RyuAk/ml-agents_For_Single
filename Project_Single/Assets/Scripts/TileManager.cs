using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    private List<GameObject> tiles = new List<GameObject>();

    private void Start()
    {
        // 모든 타일을 리스트에 추가
        foreach (Transform child in transform)
        {
            if (child.CompareTag("tile"))
            {
                tiles.Add(child.gameObject);
            }
        }
    }

    public void ResetTiles()
    {
        foreach (var tile in tiles)
        {
            tile.SetActive(true); // 모든 타일을 활성화
        }
    }
}
