
using SL.Wait;
using UnityEngine;

public class CollectableSpeed : ICollectableObject
{

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

        return true;
    }

}
