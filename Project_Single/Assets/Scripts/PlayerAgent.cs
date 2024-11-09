using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PlayerAgent : Agent
{
    private GameManager gameManager;
    private int moveCount;
    public GameObject meteorPrefab;

    public override void Initialize()
    {
        gameManager = FindObjectOfType<GameManager>();
        moveCount = 0;
    }

    public override void OnEpisodeBegin()
    {
        moveCount = 0;
        SetInitialPosition(); // 초기 위치 설정
        Debug.Log("Agent reset to initial position: " + transform.position);
    }

    private void SetInitialPosition()
    {
        // 각 플레이어의 이름에 따라 초기 위치를 설정
        switch (gameObject.name)
        {
            case "Player1":
                transform.position = new Vector3(-3.5f, 2f, 3.5f); // Player1의 고정 위치
                break;
            case "Player2":
                transform.position = new Vector3(3.5f, 2f, -3.5f); // Player2의 고정 위치
                break;
            case "Player3":
                transform.position = new Vector3(-3.5f, 2f, -3.5f); // Player3의 고정 위치
                break;
            case "Player4":
                transform.position = new Vector3(3.5f, 2f, 3.5f); // Player4의 고정 위치
                break;
        }

        Debug.Log("Agent reset to initial position: " + transform.position);
    }


    public void ResetAgent()
    {
        // 초기 위치로 설정하거나 초기화 작업 수행
        SetInitialPosition(); // 이미 있는 메서드 사용
        moveCount = 0; // 이동 횟수 초기화
        Debug.Log("Agent reset to initial position: " + transform.position);
    }

    public void StartTurn()
    {
        moveCount = 0;
        RequestDecision(); // 행동 요청
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 에이전트의 현재 위치 관찰
        sensor.AddObservation(transform.position.x);
        sensor.AddObservation(transform.position.z);

        // 주변 타일 상태 관찰 (총 8개 방향)
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if (x == 0 && z == 0) continue; // 자기 자신 위치는 제외
                Vector3 neighborPosition = transform.position + new Vector3(x, 0, z);
                bool isTileDestroyed = gameManager.IsTileDestroyed(neighborPosition);
                sensor.AddObservation(isTileDestroyed ? 0f : 1f); // 타일 상태 관찰
                bool isPlayerOnTile = gameManager.IsPlayerOnTile(neighborPosition);
                sensor.AddObservation(isPlayerOnTile ? 1f : 0f); // 플레이어 존재 여부
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (moveCount >= 2) return; // 이미 2번 이동했다면 행동 불가

        int moveX = actions.DiscreteActions[0] - 1; // -1, 0, 1
        int moveZ = actions.DiscreteActions[1] - 1; // -1, 0, 1
        int destroyAction = actions.DiscreteActions.Length > 2 ? actions.DiscreteActions[2] : 0;

        Vector3 moveDirection = new Vector3(moveX, 0, moveZ);
        Vector3 targetPosition = transform.position + moveDirection;

        if (moveCount < 2 && IsValidMove(targetPosition))
        {
            transform.position = targetPosition;
            moveCount++;
            AddReward(0.1f); // 이동 성공 시 보상
            Debug.Log("Moved to: " + targetPosition);
        }
        else if (!IsValidMove(targetPosition))
        {
            AddReward(-0.5f); // 잘못된 이동 시 페널티
            Debug.Log("Invalid move attempted.");
        }

        // 타일 파괴 행동
        if (moveCount >= 2 && destroyAction == 1)
        {
            Vector3 destroyPosition = transform.position + moveDirection;
            if (IsValidDestroy(destroyPosition))
            {
                InstantiateMeteor(destroyPosition);
                AddReward(1.0f); // 타일 파괴 성공 시 보상
                Debug.Log("Destroyed tile at: " + destroyPosition);
                gameManager.EndTurn(); // 턴 종료
            }
            else
            {
                AddReward(-0.5f); // 파괴 불가능한 타일 선택 시 페널티
                Debug.Log("Invalid destroy attempted.");
            }
        }
    }

    private bool IsValidMove(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
            if (hit.collider != null && hit.collider.CompareTag("tile"))
            {
                Debug.Log("Valid tile detected at: " + position); // 디버그 로그로 타일 감지 확인
                return hit.collider.gameObject.activeSelf && !gameManager.IsPlayerOnTile(position);
            }
            else
            {
                Debug.Log("No valid tile detected at: " + position); // 타일이 없을 때 로그
            }
        }
        return false;
    }

    private bool IsValidDestroy(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position + Vector3.up, Vector3.down, out hit, 2.0f))
        {
            return hit.collider != null && hit.collider.CompareTag("tile") &&
                   hit.collider.gameObject.activeSelf && !gameManager.IsPlayerOnTile(position);
        }
        return false;
    }

    private void InstantiateMeteor(Vector3 position)
    {
        if (meteorPrefab != null)
        {
            Vector3 vfxPosition = new Vector3(position.x, position.y + 1, position.z);
            GameObject meteor = Instantiate(meteorPrefab, vfxPosition, Quaternion.identity);
            RaycastHit hit;
            if (Physics.Raycast(vfxPosition, Vector3.down, out hit, 2.0f))
            {
                if (hit.collider != null && hit.collider.CompareTag("tile"))
                {
                    hit.collider.gameObject.SetActive(false); // 타일 비활성화
                    Destroy(meteor, 2.0f); // 메테오 제거
                }
            }
        }
        else
        {
            Debug.LogWarning("Meteor prefab is not assigned in the Inspector.");
        }
    }
}
