using UnityEngine;
using System.Collections.Generic;

public class TileManager : MonoBehaviour
{
    private List<GameObject> tiles = new List<GameObject>();

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.CompareTag("tile"))
            {
                tiles.Add(child.gameObject);
            }
        }
    }

    public void InitializeTiles()
    {
        foreach (GameObject tile in tiles)
        {
            if (tile != null)
            {
                tile.SetActive(true);
            }
        }
    }

    public bool IsTileDestroyed(Vector3 position)
    {
        GameObject tile = GetTileAtPosition(position);
        return tile == null || !tile.activeSelf;
    }

    public void DestroyTile(Vector3 position)
    {
        GameObject tile = GetTileAtPosition(position);
        if (tile != null)
        {
            tile.SetActive(false);
            Debug.Log("Tile destroyed at position: " + position);
        }
    }

    private GameObject GetTileAtPosition(Vector3 position)
    {
        foreach (GameObject tile in tiles)
        {
            if (tile != null && tile.activeSelf && Vector3.Distance(tile.transform.position, position) < 0.1f)
            {
                return tile;
            }
        }
        return null;
    }
}
