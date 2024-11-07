using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Photon.Pun;

public class PlayerMove : MonoBehaviour
{
    public LayerMask clickableLayer;
    public bool isPlayerTurn = false;
    public bool hasMoved = false; // 플레이어가 움직였는지 여부
    private bool cardUsed = false; // 카드가 사용되었는지 여부
    private List<Tile> highlightedTiles = new List<Tile>();
    public int movesLeft = 2; // 두 번의 이동을 위해
    public bool isArrested = false;
    public Color blockedColor;
    public Color moveColor;
    public Color crashableColor; // 부술 수 있는 타일의 색

    // 운석 VFX 관련 변수들
    public GameObject meteorPrefab; // 운석 프리팹
    public Transform meteorParent; // 운석이 떨어질 부모 오브젝트

    private Tile tileToCrash = null; // 부술 타일을 저장할 변수

    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
    }

    public void StartTurn()
    {
        if (isArrested)
        {
            Debug.Log($"{gameObject.name}는 구속 상태로 인해 움직일 수 없습니다.");
            movesLeft = 0;
            isArrested = false; // 턴 종료 후 Arrest 상태 해제
            GameManager.instance.EndTurn(); // 턴 자동 종료
            return;
        }

        hasMoved = false;
        cardUsed = false;
        isPlayerTurn = true;
        movesLeft = 2;

        // 카드 사용 여부 묻기
        //CardManager.instance.ShowCardOptions(this);
        HighlightMovableTiles();

        // 주변에 이동할 타일이 있는지 확인
        if (!HasMovableTiles())
        {
            HandlePlayerLoss();
            return;
        }

        HighlightMovableTiles();
    }
    public Tile CurrentTile { get; private set; }

    // 플레이어의 이동 로직에 따라 CurrentTile을 업데이트해야 함
    public void SetCurrentTile(Tile tile)
    {
        CurrentTile = tile;
    }

    public void MoveTo(Tile newTile)
    {
        // 이동 로직 구현
        CurrentTile = newTile;
    }

    public void UseCard()
    {
        if (!hasMoved)
        {
            cardUsed = true;
            // 카드 사용 로직
        }
        else
        {
            Debug.Log("이미 움직였기 때문에 카드를 사용할 수 없습니다.");
        }
    }

    public void Move(Vector3 targetPosition)
    {
        if(photonView.IsMine)
        {
            if (!hasMoved)
            {
                if (!cardUsed)
                {
                    Debug.Log("카드를 사용하지 않고 움직였습니다.");
                }
                // 이동 로직 구현
                transform.position = targetPosition;
                hasMoved = true;
            }
            else
            {
                Debug.Log("이미 움직였습니다.");
            }
        }
    }

    public void AfterCardUse()
    {
        HighlightMovableTiles(); // 카드 사용 후 이동 가능한 타일을 하이라이트
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

    void HandlePlayerLoss()
    {
        // 현재 타일의 점유 상태를 해제
        Vector3 currentTilePos = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
        Collider[] currentTileColliders = Physics.OverlapSphere(currentTilePos, 0.1f, clickableLayer);
        foreach (Collider collider in currentTileColliders)
        {
            Tile currentTile = collider.GetComponent<Tile>();
            if (currentTile != null)
            {
                currentTile.SetOccupied(false);
            }
        }

        // GameManager에 플레이어 제거 요청
        GameManager.instance.RemovePlayer(gameObject);

        // 플레이어를 보드에서 제거
        Destroy(gameObject);

        // 다음 플레이어의 턴을 시작하도록 GameManager에 통보
        GameManager.instance.EndTurn();
    }

    void HighlightMovableTiles()
    {
        // 플레이어가 구속 상태일 경우, 타일 하이라이트를 생략
        if (isArrested)
        {
            return;
        }

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
        if(photonView.IsMine)
        {
            if (isPlayerTurn)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
                    {
                        Tile clickedTile = hit.collider.GetComponent<Tile>();
                        if (clickedTile != null && highlightedTiles.Contains(clickedTile))
                        {
                            if (movesLeft > 0)
                            {
                                Vector3 targetPosition = clickedTile.transform.position;
                                targetPosition.y += 1;  // 타일 위 Y축 +1 위치로 이동

                                // 현재 타일 비움
                                Vector3 currentTilePos = new Vector3(transform.position.x, transform.position.y - 1, transform.position.z);
                                Collider[] currentTileColliders = Physics.OverlapSphere(currentTilePos, 0.1f, clickableLayer);
                                foreach (Collider collider in currentTileColliders)
                                {
                                    Tile currentTile = collider.GetComponent<Tile>();
                                    if (currentTile != null)
                                    {
                                        currentTile.SetOccupied(false);
                                    }
                                }

                                transform.position = targetPosition;
                                clickedTile.SetOccupied(true); // 새로운 타일 점유 설정
                                ClearHighlightedTiles();

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
                                // 부술 타일을 저장하고 다음 단계로 이동
                                tileToCrash = clickedTile;
                                ClearHighlightedTiles();
                                isPlayerTurn = false;

                                // 턴 종료를 지연시키고 타일을 부수기 위해 GameManager에 통보
                                GameManager.instance.ScheduleTileCrash(tileToCrash);
                            }
                        }
                    }
                }
            }
        }
    }
}
