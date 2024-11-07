using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlayerAgent : Agent
{
    private PlayerMove playerMove; // 플레이어의 이동을 처리하는 스크립트
    private Tile currentTile; // 현재 플레이어가 서 있는 타일
    public Tile[,] tileGrid; // 게임보드의 타일 배열

    private void Start()
    {
        playerMove = GetComponent<PlayerMove>();
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
        // 주변 타일의 점유 여부와 파괴 가능 여부를 관찰
        foreach (var tile in tileGrid)
        {
            sensor.AddObservation(tile.IsOccupied() ? 1 : 0); // 타일이 점유되었는지
            sensor.AddObservation(tile.highlightColor == tile.crashableColor ? 1 : 0); // 타일이 파괴 가능한지
        }

        // 플레이어의 현재 위치를 관찰
        sensor.AddObservation(currentTile.x);
        sensor.AddObservation(currentTile.y);
    }

    // 에이전트가 행동을 결정하는 메서드 (움직임과 타일 파괴)
    public override void OnActionReceived(ActionBuffers actions)
    {
        // 액션을 받아서 이동 처리
        int moveX = actions.DiscreteActions[0];
        int moveY = actions.DiscreteActions[1];

        // 이동 가능 여부 확인 후 이동
        if (IsValidMove(moveX, moveY))
        {
            //playerMove.MoveToTile(tileGrid[moveX, moveY]);
            currentTile = tileGrid[moveX, moveY];

            // 보상: 이동 성공 시 보상 지급
            AddReward(0.1f);
        }

        // 타일 파괴 액션
        if (actions.DiscreteActions[2] == 1 && currentTile != null && currentTile.highlightColor == currentTile.crashableColor)
        {
            currentTile.SetOccupied(false);
            AddReward(0.5f); // 타일을 파괴할 때 보상 지급
        }

        // 만약 자신을 제외한 모든 플레이어가 제거되었다면, 큰 보상
        if (IsLastPlayerStanding())
        {
            AddReward(1.0f);
            EndEpisode(); // 에피소드 종료
        }
    }

    // 플레이어가 직접 키보드로 에이전트를 제어하는 경우 (학습과는 별개)
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = (int)Input.GetAxisRaw("Horizontal"); // 키보드 방향키로 제어
        discreteActions[1] = (int)Input.GetAxisRaw("Vertical");
        discreteActions[2] = Input.GetKey(KeyCode.Space) ? 1 : 0; // 스페이스바로 타일 파괴
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
        // 랜덤 타일에 플레이어 위치 초기화
        int randomX = Random.Range(0, tileGrid.GetLength(0));
        int randomY = Random.Range(0, tileGrid.GetLength(1));
        //playerMove.MoveToTile(tileGrid[randomX, randomY]);
        currentTile = tileGrid[randomX, randomY];
    }

    // 타일 초기화 함수
    private void ResetTiles()
    {
        foreach (var tile in tileGrid)
        {
            tile.SetOccupied(false); // 타일 점유 상태 초기화
        }
    }
}
