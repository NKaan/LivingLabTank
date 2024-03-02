
using Mirror;
using SL.Wait;
using UnityEngine;

public class CollectableSpeed : ICollectableObject
{
    public ParticleSystem addPlayerEffect;
    public SpeedMiniPanel speedMiniPanel;
    public float addSpeed = 3f;
    public float endTime = 2f; 

    public override bool OnTriggerEnterPlayer(Player player)
    {
        if (!base.OnTriggerEnterPlayer(player))
            return false;

        player.movoment.m_Speed += addSpeed;

        Wait.Seconds(endTime, () => 
        {
            player.movoment.m_Speed -= addSpeed;

        }).Start();

        TargetOnTriggerEnterClient(player.connectionToClient);
        RpcAllClientEffectAdd(player.netId);
        return true;
    }

    [ClientRpc]
    public void RpcAllClientEffectAdd(uint netID)
    {
        Player player = NetworkClient.spawned[netID].GetComponent<Player>();

        ParticleSystem effect = Instantiate(addPlayerEffect.gameObject).GetComponent<ParticleSystem>();
        player.AddAffect(effect);
        Destroy(effect.gameObject, endTime);
    }

    [TargetRpc]
    public void TargetOnTriggerEnterClient(NetworkConnectionToClient target)
    {
        GameUI gameUI = GameObject.FindObjectOfType<GameUI>();

        SpeedMiniPanel speedPanel = Instantiate(speedMiniPanel, gameUI.leftPanelContent);
        speedPanel.SetUI(this);
    }

}
