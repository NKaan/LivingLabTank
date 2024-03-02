using Mirror;
using SL.Wait;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : NetworkBehaviour
{

    public Transform startPoint;
    public Portal targetPortal;

    public ParticleSystem portalEffect;
    public float cooldown = 3f;

    [SyncVar(hook = nameof(RpcPortalActive))]
    public bool portalActive;

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
            return;

        if (!portalActive)
            return;

        SetPortalActive(false);

        RpcClientTransformPortal(other.GetComponent<NetworkIdentity>().netId);

        Wait.Seconds(cooldown, () => SetPortalActive(true)).Start();

    }

    [Server]
    public void SetPortalActive(bool active)
    {
        portalActive = active;
        targetPortal.portalActive = active;
    }

    [ClientRpc]
    public void RpcClientTransformPortal(uint netID)
    {
        Player player = NetworkClient.spawned[netID].GetComponent<Player>();
        player.SetPosition(targetPortal.startPoint.position, targetPortal.startPoint.rotation);
    }

    public void RpcPortalActive(bool oldActive,bool nextActive)
    {
        if (!nextActive)
        {
            portalEffect.Stop();
            targetPortal.portalEffect.Stop();
        }
        else
        {
            portalEffect.Play();
            targetPortal.portalEffect.Play();
        }
    }

}
