using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableFuel : ICollectableObject
{
    public float addFuelCount = 50;

    public override bool OnTriggerEnterPlayer(Player player)
    {
        if (!base.OnTriggerEnterPlayer(player))
            return false;

        if (player.movoment.m_Fuel + addFuelCount >= player.movoment.m_Max_Fuel)
            player.movoment.m_Fuel = player.movoment.m_Max_Fuel;
        else
            player.movoment.m_Fuel += addFuelCount;

        return true;
    }
}
