using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : NetworkBehaviour
{
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        CameraController cameraController = Camera.main.GetComponent<CameraController>();

        if(cameraController != null)
        {
            cameraController.SetTarget(this.transform);
        }

    }

}
