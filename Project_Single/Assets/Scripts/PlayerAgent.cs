using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class PlayerAgent : Agent
{
    private PlayerController playerController;
    private GameManager gameManager;
    private TileManager tileManager;

    public override void Initialize()
    {
        playerController = GetComponent<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();
        tileManager = FindObjectOfType<TileManager>();
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("OnEpisodeBegin called for agent: " + gameObject.name);
        playerController.ResetMove();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Debug.Log("CollectObservations called for agent: " + gameObject.name);

        Vector3 playerPosition = transform.position;
        sensor.AddObservation(playerPosition.x);
        sensor.AddObservation(playerPosition.z);

        for (int x = 0; x < 10; x++)
        {
            for (int z = 0; z < 10; z++)
            {
                Vector3 position = new Vector3(x, 0, z);
                bool isTileDestroyed = tileManager.IsTileDestroyed(position);
                sensor.AddObservation(isTileDestroyed ? 0f : 1f);

                bool isPlayerOnTile = gameManager.IsPlayerOnTile(position);
                sensor.AddObservation(isPlayerOnTile ? 1f : 0f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        Debug.Log("OnActionReceived called for agent: " + gameObject.name);

        int moveAction = actions.DiscreteActions[0];
        Debug.Log("Move action: " + moveAction);

        int destroyAction = actions.DiscreteActions[1];
        Debug.Log("Destroy action: " + destroyAction);

        Vector3 moveDirection = Vector3.zero;

        switch (moveAction)
        {
            case 0: moveDirection = Vector3.forward; break;
            case 1: moveDirection = Vector3.back; break;
            case 2: moveDirection = Vector3.right; break;
            case 3: moveDirection = Vector3.left; break;
        }

        if (moveDirection != Vector3.zero)
        {
            playerController.Move(moveDirection);
        }

        if (destroyAction == 1)
        {
            Vector3 tileToDestroy = transform.position + moveDirection;
            if (gameManager.IsDestructible(tileToDestroy))
            {
                playerController.DestroyTile(tileToDestroy);
                AddReward(1.0f);
            }
            else
            {
                AddReward(-0.5f);
            }
        }

        if (moveDirection != Vector3.zero && gameManager.IsValidMove(transform.position + moveDirection))
        {
            AddReward(0.1f);
        }

        Debug.Log("OnActionReceived completed for agent: " + gameObject.name);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;
        if (Input.GetKey(KeyCode.W)) discreteActions[0] = 0;
        if (Input.GetKey(KeyCode.S)) discreteActions[0] = 1;
        if (Input.GetKey(KeyCode.D)) discreteActions[0] = 2;
        if (Input.GetKey(KeyCode.A)) discreteActions[0] = 3;

        if (Input.GetKey(KeyCode.Space)) discreteActions[1] = 1;
    }
}
