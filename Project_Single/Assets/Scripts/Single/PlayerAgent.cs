using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlayerAgent : Agent
{
    private PlayerMove playerMove; // 플레이어의 이동을 처리하는 스크립트
    private Tile currentTile; // 현재 플레이어가 서 있는 타일
    public Tile[,] tileGrid; // GameManager에서 할당되는 타일 배열

    private void Start()
    {
        playerMove = GetComponent<PlayerMove>();

        // GameManager에서 tileGrid 할당 여부를 확인
        if (tileGrid == null)
        {
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                tileGrid = gameManager.tileGrid;
            }
            else
            {
                Debug.LogError("GameManager가 씬에 존재하지 않습니다.");
                return;
            }
        }
    }

    private void SetInitialPosition()
    {
        int randomX = Random.Range(0, tileGrid.GetLength(0));
        int randomY = Random.Range(0, tileGrid.GetLength(1));
        currentTile = tileGrid[randomX, randomY];
        transform.position = new Vector3(currentTile.transform.position.x, transform.position.y, currentTile.transform.position.z);
    }

    // 에피소드가 시작될 때 호출 (매 경기 시작)
    public override void OnEpisodeBegin()
    {
        // 플레이어와 타일 상태를 초기화
        ResetPlayer();
        ResetTiles();
    }

    // 환경을 관찰하는 메서드
    public override void CollectObservations(VectorSensor sensor)
    {
        for (int i = 0; i < 10; i++)
        {
            sensor.AddObservation(i); // 실제 관찰값을 추가
        }

        if (tileGrid == null || currentTile == null)
        {
            Debug.LogError("tileGrid 또는 currentTile이 null입니다.");
            return;
        }

        // 예: 주변 타일의 점유 상태를 관찰
        int observationCount = 0;
        foreach (var tile in tileGrid)
        {
            if (tile != null)
            {
                sensor.AddObservation(tile.IsOccupied() ? 1 : 0);
                observationCount++;
            }
        }

        // 플레이어의 현재 위치를 관찰
        sensor.AddObservation(currentTile.x);
        sensor.AddObservation(currentTile.y);
        observationCount += 2; // x와 y 좌표 추가

        Debug.Log($"관찰값의 수: {observationCount}");

        // 관찰값이 기대되는 `Space Size`와 일치하는지 확인
        if (observationCount != sensor.ObservationSize())
        {
            Debug.LogWarning($"관찰값의 수({observationCount})가 설정된 벡터 관찰 크기({sensor.ObservationSize()})와 일치하지 않습니다.");
        }
    }

    // 에이전트가 행동을 결정하는 메서드 (움직임과 타일 파괴)
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (tileGrid == null || currentTile == null)
        {
            Debug.LogError("tileGrid 또는 currentTile이 null입니다.");
            return;
        }

        // 액션 인덱스 추출
        int moveX = actions.DiscreteActions[0]; // 이동할 X 좌표
        int moveY = actions.DiscreteActions[1]; // 이동할 Y 좌표
        int actionType = actions.DiscreteActions[2]; // 0: 이동, 1: 타일 파괴

        if (actionType == 0) // 이동
        {
            if (IsValidMove(moveX, moveY))
            {
                Tile targetTile = tileGrid[moveX, moveY];
                if (targetTile != null)
                {
                    playerMove.MoveTo(targetTile);
                    AddReward(0.1f); // 이동 성공 시 보상
                }
            }
        }
        else if (actionType == 1) // 타일 파괴
        {
            if (currentTile != null)
            {
                playerMove.DestroyTile(currentTile);
                AddReward(0.5f); // 타일을 파괴할 때 보상
            }
        }

        // 게임 종료 조건 확인
        if (IsLastPlayerStanding())
        {
            AddReward(100.0f); // 승리 시 큰 보상
            EndEpisode(); // 에피소드 종료
        }
    }

    // 유효한 이동인지 확인하는 함수
    private bool IsValidMove(int x, int y)
    {
        if (x >= 0 && x < tileGrid.GetLength(0) && y >= 0 && y < tileGrid.GetLength(1))
        {
            return !tileGrid[x, y].IsOccupied(); // 타일이 점유되지 않았을 때만 이동 가능
        }
        return false;
    }

    // 마지막 플레이어인지 확인하는 함수
    private bool IsLastPlayerStanding()
    {
        int activePlayers = 0;
        foreach (var player in FindObjectsOfType<PlayerAgent>())
        {
            if (player != this && player.gameObject.activeSelf)
            {
                activePlayers++;
            }
        }
        return activePlayers == 0;
    }

    // 플레이어 초기화 함수
    private void ResetPlayer()
    {
        if (tileGrid == null)
        {
            Debug.LogError("tileGrid가 초기화되지 않았습니다.");
            return;
        }

        int randomX = Random.Range(0, tileGrid.GetLength(0));
        int randomY = Random.Range(0, tileGrid.GetLength(1));

        if (tileGrid[randomX, randomY] == null)
        {
            Debug.LogError("랜덤 타일이 null입니다.");
            return;
        }

        // playerMove.MoveToTile(tileGrid[randomX, randomY]);
        currentTile = tileGrid[randomX, randomY];
    }

    // 타일 초기화 함수
    private void ResetTiles()
    {
        if (tileGrid == null)
        {
            Debug.LogError("tileGrid가 초기화되지 않았습니다.");
            return;
        }

        foreach (var tile in tileGrid)
        {
            if (tile != null)
            {
                tile.SetOccupied(false); // 타일 점유 상태 초기화
            }
            else
            {
                Debug.LogError("tileGrid 내 타일이 null입니다.");
            }
        }
    }
}
