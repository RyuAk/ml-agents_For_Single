using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerCameraManager : MonoBehaviourPunCallbacks
{
    public Camera[] PlayerCameras;

    private void Start()
    {
        if(PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("SlotNumber"))
        {
            int slotNumber = (int)PhotonNetwork.LocalPlayer.CustomProperties["SlotNumber"];
            ActivatePlayerCamera(slotNumber);
        }
    }

    void ActivatePlayerCamera(int slotNumber)
    {
        for(int i=0;i<PlayerCameras.Length;i++)
        {
            if(i==slotNumber)
            {
                PlayerCameras[i].gameObject.SetActive(true);
            }
            else
            {
                PlayerCameras[i].gameObject.SetActive(false);
            }
        }

    }
}
