using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject meteorPrefab; // 운석 효과 프리팹
    private bool hasMovedTwice = false;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    public void Move(Vector3 direction)
    {
        Debug.Log("Move called with direction: " + direction);

        if (hasMovedTwice) return;

        Vector3 targetPosition = transform.position + direction;
        if (gameManager.IsValidMove(targetPosition))
        {
            transform.position = targetPosition;
            hasMovedTwice = true;
            Debug.Log("Player moved to: " + targetPosition);
        }
        else
        {
            Debug.Log("Invalid move attempted.");
        }
    }

    public void DestroyTile(Vector3 tilePosition)
    {
        Debug.Log("DestroyTile called at position: " + tilePosition);

        if (gameManager.IsDestructible(tilePosition))
        {
            Instantiate(meteorPrefab, tilePosition, Quaternion.identity);
            gameManager.DestroyTile(tilePosition);
            gameManager.EndTurn();
        }
        else
        {
            Debug.Log("Invalid tile destruction attempt.");
        }
    }

    public void ResetMove()
    {
        hasMovedTwice = false;
    }
}
