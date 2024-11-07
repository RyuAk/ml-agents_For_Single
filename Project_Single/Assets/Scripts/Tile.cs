using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    private Color originalColor;
    public Color highlightColor;
    public Color crashableColor; // 부술 수 있는 타일의 색
    private MeshRenderer meshRenderer;
    private bool occupied = false; // 타일의 점유 상태
    public int x, y; // 타일의 위치를 나타내는 좌표
    public Tile[,] tileGrid; // 타일들이 저장된 2D 배열

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            originalColor = meshRenderer.material.color;
        }
        else
        {
            Debug.LogWarning("MeshRenderer component not found on " + gameObject.name);
        }
    }

    public void Highlight()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = highlightColor;
        }
    }

    public void Highlight(Color customColor)
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = customColor;
        }
    }

    public void Unhighlight()
    {
        if (meshRenderer != null)
        {
            meshRenderer.material.color = originalColor;
        }
    }

    public void SetOccupied(bool isOccupied)
    {
        occupied = isOccupied;
    }

    public bool IsOccupied()
    {
        return occupied;
    }

    public Tile GetTileLeft()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.left, out hit, 1f))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    public Tile GetTileRight()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.right, out hit, 1f))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    public Tile GetTileAbove()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.forward, out hit, 1f))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }

    public Tile GetTileBelow()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.back, out hit, 1f))
        {
            return hit.collider.GetComponent<Tile>();
        }
        return null;
    }
}
