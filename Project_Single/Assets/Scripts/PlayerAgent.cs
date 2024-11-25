using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    private GameManager gameManager;
    private int moveCount;
    //private float cumulativePenalty; // 누적 패널티 점수
    private const float penaltyThreshold = -3f; // 탈락 임계치

    public override void Initialize()
    {
        gameManager = FindObjectOfType<GameManager>();
        moveCount = 0;
        //cumulativePenalty = 0f;
    }

    public override void OnEpisodeBegin()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        gameManager.RestartGameCycle();
        moveCount = 0;
        SetInitialPosition();
        Debug.Log("OnEpisodeBegin: " + transform.position);

        // 초기화 후 턴 시작
        StartTurn();
    }

    private void SetInitialPosition()
    {
        switch (gameObject.name)
        {
            case "Player1":
                transform.position = new Vector3(-3.5f, 2f, 3.5f);
                break;
            case "Player2":
                transform.position = new Vector3(3.5f, 2f, -3.5f);
                break;
            case "Player3":
                transform.position = new Vector3(-3.5f, 2f, -3.5f);
                break;
            case "Player4":
                transform.position = new Vector3(3.5f, 2f, 3.5f);
                break;
        }
    }

    public void ResetAgent()
    {
        SetInitialPosition();
        moveCount = 0;
        //cumulativePenalty = 0f;
        Debug.Log("Reset position: " + transform.position);
    }

    public void StartTurn()
    {
        moveCount = 0;
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 현재 에이전트의 위치 관찰
        sensor.AddObservation(transform.position.x / 10f); // 보드 크기 기준 정규화
        sensor.AddObservation(transform.position.z / 10f);

        // 주변 1~2칸 관찰 (3x3 또는 5x5 영역)
        for (int x = -2; x <= 2; x++) // 주변 2칸까지
        {
            for (int z = -2; z <= 2; z++)
            {
                if (x == 0 && z == 0) continue; // 자기 자신 위치 제외

                Vector3 neighborPosition = transform.position + new Vector3(x, 0, z);
                RaycastHit hit;

                // Raycast로 타일이 있는지 확인
                if (Physics.Raycast(neighborPosition + Vector3.up * 5f, Vector3.down, out hit, 10f))
                {
                    if (hit.collider != null && hit.collider.CompareTag("tile"))
                    {
                        // 타일 상태 관찰
                        bool isTileDestroyed = gameManager.IsTileDestroyed(neighborPosition);
                        sensor.AddObservation(isTileDestroyed ? 0f : 1f); // 타일 상태 (0: 파괴됨, 1: 활성화)

                        // 플레이어 위치 여부 관찰
                        bool isPlayerOnTile = gameManager.IsPlayerOnTile(neighborPosition);
                        sensor.AddObservation(isPlayerOnTile ? 1f : 0f); // 1: 플레이어 있음, 0: 없음
                    }
                    else
                    {
                        // 타일이 없는 위치로 간주
                        sensor.AddObservation(0f); // 타일 상태 없음
                        sensor.AddObservation(0f); // 플레이어 없음
                    }
                }
                else
                {
                    // Raycast 결과가 없는 경우 기본값
                    sensor.AddObservation(0f); // 타일 상태 없음
                    sensor.AddObservation(0f); // 플레이어 없음
                }
            }
        }
    }


    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveX = actions.DiscreteActions[0] - 1;
        int moveZ = actions.DiscreteActions[1] - 1;
        int destroyActionX = actions.DiscreteActions[2] - 1;
        int destroyActionZ = actions.DiscreteActions[3] - 1;

        // 움직임 처리
        if (moveCount < 2)
        {
            if (TryMove(moveX, moveZ))
            {
                moveCount++;
            }
            else
            {
                // 유효하지 않은 움직임: 행동 재요청
                RequestDecision();
                return;
            }
        }

        // 타일 파괴 처리
        if (moveCount >= 2)
        {
            if (TryDestroyTile(destroyActionX, destroyActionZ))
            {
                moveCount = 0;
                gameManager.EndTurn();
            }
            else
            {
                // 유효하지 않은 타일 파괴: 행동 재요청
                RequestDecision();
            }
        }
    }

    private bool TryMove(int moveX, int moveZ)
    {
        Vector3 moveDirection = new Vector3(moveX, 0, moveZ);
        Vector3 moveTargetPosition = transform.position + moveDirection;

        if (IsValidMove(moveTargetPosition))
        {
            transform.position = moveTargetPosition;
            AddReward(1f); // 이동 성공 보상
            Debug.Log("Moved to: " + moveTargetPosition);
            return true;
        }
        else
        {
            Debug.Log("Invalid move attempted.");
            return false;
        }
    }

    private bool TryDestroyTile(int destroyX, int destroyZ)
    {
        Vector3 destroyPosition = transform.position + new Vector3(destroyX, 0, destroyZ);

        if (IsValidDestroy(destroyPosition))
        {
            DestroyTile(destroyPosition);
            AddReward(1f); // 타일 파괴 성공 보상
            Debug.Log("Destroyed tile at: " + destroyPosition);
            return true;
        }
        else
        {
            Debug.Log("Invalid destroy attempted.");
            return false;
        }
    }


    private bool IsValidMove(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
            if (hit.collider != null && hit.collider.CompareTag("tile"))
            {
                return hit.collider.gameObject.activeSelf && !gameManager.IsPlayerOnTile(position);
            }
        }
        return false;
    }

    private bool IsValidDestroy(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
            if (hit.collider != null && hit.collider.CompareTag("tile"))
            {
                return hit.collider.gameObject.activeSelf && !gameManager.IsPlayerOnTile(position);
            }
        }
        return false;
    }

    private void DestroyTile(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
            if (hit.collider != null && hit.collider.CompareTag("tile"))
            {
                hit.collider.gameObject.SetActive(false);
                Debug.Log("Tile destroyed at: " + position);
            }
        }
    }
}
