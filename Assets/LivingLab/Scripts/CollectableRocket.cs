using UnityEngine;

public class CollectableRocket : ICollectableObject
{

    public int addAmountCount = 1;
    public bool isRandom = false;

    public override bool OnTriggerEnterPlayer(Player player)
    {
        if (!base.OnTriggerEnterPlayer(player))
            return false;

        int addAmount = addAmountCount;

        if (isRandom)
            addAmount = Random.Range(1, addAmountCount + 1);

        InterceptMissileController interceptMissileController = player.GetComponent<InterceptMissileController>();

        interceptMissileController.MissileCount += addAmount;
        interceptMissileController.ResetSpawn();

        return true;
    }
}
