using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableHealth : ICollectableObject
{

    public int addHealthCount = 25;
    public bool isRandom = true;

    public override bool OnTriggerEnterPlayer(Player player)
    {
        if(!base.OnTriggerEnterPlayer(player))
            return false;

        int addHealth = addHealthCount;
        
        if(isRandom)
            addHealth = Random.Range(1, addHealthCount + 1);

        if (player.health.m_CurrentHealth + addHealth >= player.health.m_StartingHealth)
            player.health.m_CurrentHealth = player.health.m_StartingHealth;
        else
            player.health.m_CurrentHealth += addHealth;

        return true;
    }

}
