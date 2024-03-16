using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

    public class Armor_Collider_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Armor_Collider" in the tank.
		 * When the bullet hits this collider, the damage value is multiplied by the specified value.
		 * This collider can be used as a weak point or a strong point in the tank.
		*/

        [Header("Armor settings")]
        [Tooltip("Multiplier for the damage value.")] public float damageMultiplier = 1.0f;


        void Start()
        {
            // Set the layer.
            gameObject.layer = Layer_Settings_CS.Armor_Collider_Layer;

            // Make it invisible.
            var renderer = GetComponent<MeshRenderer>();
            if (renderer)
            {
                renderer.enabled = false;

            }

            // Make the collider not trigger.
            var collider = GetComponent<Collider>();
            if (collider)
            {
                collider.isTrigger = false;
            }
        }

    }

}
