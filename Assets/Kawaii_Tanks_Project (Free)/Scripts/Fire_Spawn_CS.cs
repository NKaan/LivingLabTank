using UnityEngine;
using System.Collections;

// This script must be attached to "Fire_Point".
namespace ChobiAssets.KTP
{
	
	public class Fire_Spawn_CS : MonoBehaviour
	{
        /* 
		 * This script is attached to "Fire_Point" under the "Barrel_Base" in the tank.
		 * This script instantiates the bullet prefab and shoot it from the fire point.
		*/

        [Header("Firing settings")]
        [Tooltip("Prefab of the bullet.")] public GameObject bulletPrefab;
        [Tooltip("Prefab of the muzzle fire.")] public GameObject firePrefab;
        [Tooltip("Attack force (AP) of the bullet.")] public float attackForce = 100.0f;
        [Tooltip("Initial velocity of the bullet. (Meter per Second)")] public float bulletVelocity = 250.0f;
        [Tooltip("Offset distance for spawning the bullet. (Meter)")] public float spawnOffset = 1.0f;


        Transform thisTransform;


        void Start()
        {
            thisTransform = transform;
        }


        public void Fire_Linkage()
        { // Called from "Fire_Control_CS".

            // Generate the bullet and shoot it.
            StartCoroutine("Generate_Bullet");
        }


        IEnumerator Generate_Bullet()
        {
            // Instantiate the muzzle fire prefab.
            if (firePrefab)
            {
                Instantiate(firePrefab, thisTransform.position, thisTransform.rotation, thisTransform);
            }

            // Instantiate the bullet prefab.
            var bulletObject = Instantiate(bulletPrefab, thisTransform.position + thisTransform.forward * spawnOffset, thisTransform.rotation) as GameObject;

            // Setup "Bullet_Nav_CS" in the bullet.
            var bulletScript = bulletObject.GetComponent<Bullet_Nav_CS>();
            bulletScript.attackForce = attackForce;
            
            // Set the tag.
            bulletObject.tag = "Finish"; // (Note.) The object with "Finish" tag cannot be locked on.

            // Set the layer.
            bulletObject.layer = Layer_Settings_CS.Bullet_Layer;

            // Shoot.
            yield return new WaitForFixedUpdate();
            var rigidbody = bulletObject.GetComponent<Rigidbody>();
            var currentVelocity = bulletObject.transform.forward * bulletVelocity;
            rigidbody.velocity = currentVelocity;
        }

    }

}
