using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PMove : MonoBehaviour
{
    public LayerMask clickableLayer;
    public bool isPlayerTurn = false;
    public bool hasMoved = false;
    //private bool cardUsed = false;
    private List<Tile> highlightedTiles = new List<Tile>();
    public int movesLeft = 2;
    public Color blockedColor;
    public Color moveColor;
    public Color crashableColor;

    public GameObject meteorPrefab;
    public Transform meteorParent;

    private Tile tileToCrash = null;
    private PhotonView photonView;
    private Camera playerCamera;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (photonView.IsMine)
        {
            playerCamera = GetComponentInChildren<Camera>();
        }
    }

    public void StartTurn()
    {
        hasMoved = false;
        //cardUsed = false;
        isPlayerTurn = true;
        movesLeft = 2;

        if(photonView != null)
        {
            HighlightMovableTilesForPlayer();
        }

        if (!HasMovableTiles())
        {
            HandlePlayerLoss();
            return;
        }

        HighlightMovableTiles();
    }

    [PunRPC]
    void HighlightMovableTilesForPlayer()
    {
        if (photonView.IsMine && isPlayerTurn)
        {
            HighlightMovableTiles();
        }
    }

    public void SetIsPlayerTurn(bool isTurn)
    {
        isPlayerTurn = isTurn;
        hasMoved = false;
        if (isTurn && photonView.IsMine)
        {
            HighlightMovableTiles();
        }
        else
        {
            ClearHighlightedTiles();
        }
    }

    void HighlightMovableTiles()
    {
        ClearHighlightedTiles();

        Vector3 playerPos = transform.position;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3 tilePos = new Vector3(playerPos.x + x, playerPos.y - 1, playerPos.z + y);
                Collider[] hitColliders = Physics.OverlapSphere(tilePos, 0.1f, clickableLayer);

                foreach (Collider hitCollider in hitColliders)
                {
                    Tile tile = hitCollider.GetComponent<Tile>();
                    if (tile != null && !tile.IsOccupied())
                    {
                        tile.Highlight(moveColor);
                        highlightedTiles.Add(tile);
                    }
                }
            }
        }
    }

    void HighlightCrashableTiles()
    {
        ClearHighlightedTiles();

        Vector3 playerPos = transform.position;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3 tilePos = new Vector3(playerPos.x + x, playerPos.y - 1, playerPos.z + y);
                Collider[] hitColliders = Physics.OverlapSphere(tilePos, 0.1f, clickableLayer);

                foreach (Collider hitCollider in hitColliders)
                {
                    Tile tile = hitCollider.GetComponent<Tile>();
                    if (tile != null && !tile.IsOccupied())
                    {
                        tile.Highlight(crashableColor);
                        highlightedTiles.Add(tile);
                    }
                }
            }
        }
    }

    void ClearHighlightedTiles()
    {
        foreach (Tile tile in highlightedTiles)
        {
            tile.Unhighlight();
        }
        highlightedTiles.Clear();
    }

    void Update()
    {
        if (photonView.IsMine && isPlayerTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(playerCamera != null)
                {
                    Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition); //Camera.main
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
                    {
                        Tile clickedTile = hit.collider.GetComponent<Tile>();
                        if (clickedTile != null && highlightedTiles.Contains(clickedTile))
                        {
                            if (movesLeft > 0)
                            {
                                Vector3 targetPosition = clickedTile.transform.position;
                                targetPosition.y += 1;

                                photonView.RPC("MoveToPosition", RpcTarget.All, targetPosition);
                                movesLeft--;

                                if (movesLeft > 0)
                                {
                                    HighlightMovableTiles();
                                }
                                else
                                {
                                    HighlightCrashableTiles();
                                }
                            }
                            else
                            {
                                tileToCrash = clickedTile;
                                ClearHighlightedTiles();
                                isPlayerTurn = false;

                                photonView.RPC("CrashTileAndEndTurn", RpcTarget.All, tileToCrash.transform.position);
                            }
                        }
                    }
                }
            }
        }
    }

    [PunRPC]
    public void MoveToPosition(Vector3 targetPosition)
    {
        if (photonView.IsMine)
        {
            transform.position = targetPosition;
        }
    }

    [PunRPC]
    public void CrashTileAndEndTurn(Vector3 tilePosition)
    {
        Collider[] hitColliders = Physics.OverlapSphere(tilePosition, 0.1f, clickableLayer);
        foreach (Collider hitCollider in hitColliders)
        {
            Tile tile = hitCollider.GetComponent<Tile>();
            if (tile != null)
            {
                tile.SetOccupied(false);
                baekGManager.instance.ScheduleTileCrash(tile); // 타일 파괴 예약
            }
        }

        photonView.RPC("EndTurn", RpcTarget.All);
    }

    [PunRPC]
    public void EndTurn()
    {
        isPlayerTurn = false;
        baekGManager.instance.EndTurn(); // 다음 플레이어로 턴 이동
    }

    bool HasMovableTiles()
    {
        Vector3 playerPos = transform.position;

        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                Vector3 tilePos = new Vector3(playerPos.x + x, playerPos.y - 1, playerPos.z + y);
                Collider[] hitColliders = Physics.OverlapSphere(tilePos, 0.1f, clickableLayer);

                foreach (Collider hitCollider in hitColliders)
                {
                    Tile tile = hitCollider.GetComponent<Tile>();
                    if (tile != null && !tile.IsOccupied())
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private void HandlePlayerLoss()
    {
        isPlayerTurn = false;
        baekGManager.instance.RemovePlayer(gameObject);
        baekGManager.instance.EndTurn();
    }
}
