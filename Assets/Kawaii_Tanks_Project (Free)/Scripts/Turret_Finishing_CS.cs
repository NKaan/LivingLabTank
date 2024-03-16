using UnityEngine;

namespace ChobiAssets.KTP
{

    public class Turret_Finishing_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "Turret Objects" in the tank.
		 * This script change the hierarchy of the child objects such as "Turret_Base", "Cannon_Base" and "Barrel_Base" at the first time.
		*/


        void Awake()
        { // These function must be called before "Start()".
            var turretBase = transform.Find("Turret_Base");
            var cannonBase = transform.Find("Cannon_Base");
            var barrelBase = transform.Find("Barrel_Base");
            if (turretBase && cannonBase && barrelBase)
            {
                // Change the hierarchy.
                barrelBase.parent = cannonBase;
                cannonBase.parent = turretBase;
            }
            else
            {
                Debug.LogError("'Turret_Finishing_CS' could not change the hierarchy of the turret.");
                Debug.LogWarning("Make sure the names of 'Turret_Base', 'Cannon_Base' and 'Barrel_Base'.");
            }

            Destroy(this);
        }

    }

}