using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    private GameManager gameManager;
    private int moveCount;

    public override void Initialize()
    {
        gameManager = FindObjectOfType<GameManager>();
        moveCount = 0;
    }

    public override void OnEpisodeBegin()
    {
        moveCount = 0;
        SetInitialPosition();
        Debug.Log("OnEpisodeBegin: " + transform.position);
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
        Debug.Log("Reset position: " + transform.position);
    }

    public void StartTurn()
    {
        moveCount = 0;
        RequestDecision();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("Collecting observations...");

        // 현재 에이전트의 위치 관찰
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);

        // 주변 타일 상태 관찰 (10x10 타일 상태)
        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                Vector3 position = new Vector3(x - 4.5f, 0, z - 4.5f);
                RaycastHit hit;

                // Raycast로 타일이 있는지 확인
                if (Physics.Raycast(position + Vector3.up * 5f, Vector3.down, out hit, 10f))
                {
                    // 히트된 오브젝트의 태그가 "tile"인지 확인
                    if (hit.collider != null && hit.collider.CompareTag("tile"))
                    {
                        bool isTileDestroyed = gameManager.IsTileDestroyed(position);
                        sensor.AddObservation(isTileDestroyed ? 0f : 1f);

                        bool isPlayerOnTile = gameManager.IsPlayerOnTile(position);
                        sensor.AddObservation(isPlayerOnTile ? 1f : 0f);
                    }
                    else
                    {
                        // 타일이 아니면 해당 위치는 0으로 간주 (없음)
                        sensor.AddObservation(0f);
                        sensor.AddObservation(0f);
                    }
                }
                else
                {
                    // Raycast 결과가 없을 경우 (타일이 없을 때) 기본값을 추가
                    sensor.AddObservation(0f);
                    sensor.AddObservation(0f);
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

        bool validMoveAttempted = false;
        bool validDestroyAttempted = false;

        // 이동 시도
        if (moveCount < 2)
        {
            validMoveAttempted = TryMove(moveX, moveZ);
            if (validMoveAttempted)
            {
                moveCount++;
                return; // 올바른 이동이면 다음 행동 대기
            }
            else
            {
                AddReward(-0.2f); // 잘못된 이동 시 페널티
                RequestDecision(); // 다시 행동 요청
                return;
            }
        }

        // 타일 파괴 시도
        if (moveCount >= 2)
        {
            validDestroyAttempted = TryDestroyTile(destroyActionX, destroyActionZ);
            if (validDestroyAttempted)
            {
                moveCount = 0;
                gameManager.EndTurn(); // 올바른 타일 파괴 후 턴 종료
            }
            else
            {
                AddReward(-0.2f); // 잘못된 파괴 시 페널티
                RequestDecision(); // 다시 행동 요청
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
            AddReward(0.5f);
            Debug.Log("Moved to: " + moveTargetPosition);
            return true;
        }
        else
        {
            AddReward(-0.2f);
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
            AddReward(0.5f);
            Debug.Log("Destroyed tile at: " + destroyPosition);
            return true;
        }
        else
        {
            AddReward(-0.2f);
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
