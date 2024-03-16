using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{

    public class Bullet_Nav_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to the prefab of the bullet.
		 * This script controls the posture and the collision of the bullet.
         * When the bullet hits the target, this script sends the damage value to the "Damage_Control_##_##_CS" script in the hit collider.
		 * The damage value is calculated considering the hit angle.
		*/

        [Header("Bullet settings")]
        [Tooltip("Life time of the bullet. (Sec)")] public float lifeTime = 5.0f;
        [Tooltip("Prefab for the broken effects.")] public GameObject brokenObject;
        [Tooltip("Set this Rigidbody.")] public Rigidbody thisRigidbody;

        // Set by "Fire_Spawn".
        [HideInInspector] public float attackForce;

        Transform thisTransform;
        bool isLiving = true;
        bool hasDetected = false;
        Ray ray = new Ray();
        Vector3 hitPos;
        Transform hitTransform;
        Vector3 hitNormal;


        void Start()
        {
            thisTransform = transform;
            if (thisRigidbody == null)
            {
                thisRigidbody = GetComponent<Rigidbody>();
            }

            // Set the life time.
            Destroy(this.gameObject, lifeTime);
        }


        void FixedUpdate()
        {
            // Check the bullet is living.
            if (isLiving == false)
            { // The bullet already hit anything.
                return;
            }

            // Control the posture.
            thisTransform.LookAt(thisRigidbody.position + thisRigidbody.velocity);

            // Cast a ray to detect an obstacle..
            Cast_Ray();
        }


        void Cast_Ray()
        {
            if (hasDetected == false)
            { // The bullet has not detected any obstacle.

                // Cast a ray forward.
                ray.origin = thisRigidbody.position;
                ray.direction = thisRigidbody.velocity;
                Physics.Raycast(ray, out RaycastHit raycastHit, thisRigidbody.velocity.magnitude * Time.fixedDeltaTime, Layer_Settings_CS.Layer_Mask);
                if (raycastHit.collider)
                { // The ray hits.
                    // Store the hit point for the next frame.
                    hitPos = raycastHit.point;

                    // Get the hit object.
                    hitTransform = raycastHit.collider.transform;

                    // Get the hit normal.
                    hitNormal = raycastHit.normal;

                    // Switch the flag.
                    hasDetected = true;
                }
            }
            else
            { // The bullet detected an obstacle in the previous frame.

                // Set the position.
                thisTransform.position = hitPos;

                // Start the hit process.
                Hit();
            }
        }


        void OnCollisionEnter(Collision collision)
        { // Collision is detected by the physics engine before the ray casting.

            // Check the bullet is living.
            if (isLiving == false)
            { // The bullet already hit anything.
                return;
            }

            // Get the hit object.
            hitTransform = collision.collider.transform;

            // Get the hit normal.
            hitNormal = collision.contacts[0].normal;

            // Start the hit process.
            Hit();
        }


        void Hit()
        {
            // Set the living flag.
            isLiving = false;

            // Spawn the broken object.
            if (brokenObject)
            {
                Instantiate(brokenObject, thisTransform.position, Quaternion.identity);
            }

            // Check the hit object exists.
            if (hitTransform == null)
            {
                // The hit object has already been removed from the scene.
                return;
            }

            // Call "Damage_Control_CS" in the hit object.
            var targetDamageScript = hitTransform.root.GetComponent<Damage_Control_CS>();
            if (targetDamageScript)
            { // The hit object has "Damage_Control_CS".

                // Calculate damage value accoding to the hit angle.
                var hitAngle = Mathf.Abs(90.0f - Vector3.Angle(thisTransform.forward, hitNormal));
                var damageValue = attackForce * Mathf.Lerp(0.0f, 1.0f, Mathf.Sqrt(hitAngle / 90.0f));

                // Check the hit object is "Armor_Collider".
                var armorScript = hitTransform.GetComponent<Armor_Collider_CS>();
                if (armorScript)
                { // The hit object has "Armor_Collider_CS".

                    // Apply the multiplier to the damage value.
                    damageValue *= armorScript.damageMultiplier;
                }

                // Send the damage value to the "Damage_Control_CS".
                targetDamageScript.Get_Damage(damageValue);
            }

            // Destroy this object.
            Destroy(this.gameObject);
        }
    }

}
