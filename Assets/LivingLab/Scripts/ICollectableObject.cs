using Mirror;
using SL.Wait;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ICollectableObject : NetworkBehaviour
{
    [SyncVar(hook = nameof(CollectedHook))] public bool collected = false;
    public float collectedTime = 5f;
    private MeshRenderer myRenderer;
    public string collectableObjectName;

    private void Awake()
    {
        myRenderer = GetComponent<MeshRenderer>();
    }

    public void CollectedHook(bool oldValue,bool nextValue)
    {
        myRenderer.enabled = !nextValue;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Temas Edilen Objenin Adý : " + other.gameObject.name);

        if (other.tag != "Player")
            return;

        if (collected)
            return;

        collected = true;

        Player player = other.GetComponent<Player>();

        if(player != null)
            OnTriggerEnterPlayer(player);

        Wait.Seconds(collectedTime, () => { collected = false;  }).Start();

    }

    public virtual bool OnTriggerEnterPlayer(Player player)
    {
        return true;
    }

}
