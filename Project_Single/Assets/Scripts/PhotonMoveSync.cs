using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PhotonMoveSync : MonoBehaviour
{
    public PlayerMove playerMove;
    public PhotonView photonView;

    private void Start()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    private void Update()
    {
        if(photonView.IsMine)
        {
            playerMove.AfterCardUse();
        }
    }

    public void SyncMove(Vector3 newPosition)
    {
        photonView.RPC("RPC_SyncMove", RpcTarget.AllBuffered, newPosition);
    }

    [PunRPC]
    void RPC_SyncMove(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    /*
     public PhotonMoveSync photonMoveSync

     */
}
