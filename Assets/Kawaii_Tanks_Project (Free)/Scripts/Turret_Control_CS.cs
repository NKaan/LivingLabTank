using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{
	
	public class Turret_Control_CS : MonoBehaviour
	{
        /* 
		 * This script rotates the turret horizontally.
		 * This script works in combination with "Aiming_Control_CS" in the MainBody.
		*/

        [Header("Turret movement settings")]
        [Tooltip("Maximum rotation speed. (Degree per Second)")] public float rotationSpeed = 15.0f;
        [Tooltip("Time to reach the maximum speed from zero. (Sec)")] public float accelerationTime = 0.2f;
        [Tooltip("Time to stop from the maximum speed. (Sec)")] public float decelerationTime = 0.2f;


        Transform thisTransform;
        Transform parentTransform;
        Aiming_Control_CS aimingScript;
        bool isTurning;
        bool isTracking;
        float angleY;
        Vector3 currentLocalAngles;
        float turnRate;
        float previousTurnRate;
        float bulletVelocity = 250.0f;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            parentTransform = thisTransform.parent;
            currentLocalAngles = thisTransform.localEulerAngles;
            angleY = currentLocalAngles.y;

            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            aimingScript = idScript.aimingScript;
            bulletVelocity = idScript.fireSpawnScript.bulletVelocity;
        }


        public void Start_Tracking()
        { // Called from "Aiming_Control_CS".
            isTracking = true;
            isTurning = true;
        }


        public void Stop_Tracking()
        { // Called from "Aiming_Control_CS".
            isTracking = false;
        }


        void FixedUpdate()
        {
            if (aimingScript.useAutoTurn)
            {
                Auto_Turn();
            }
            else
            {
                Manual_Turn();
            }
        }


        void Auto_Turn()
        {
            if (isTurning == false)
            {
                return;
            }

            // Calculate the target angle.
            float targetAngle;
            if (isTracking)
            { // Tracking the target now.
                
                // Get the target position.
                var targetPosition = aimingScript.targetPosition;

                // Calculate the target angle.
                Vector3 targetLocalPos = parentTransform.InverseTransformPoint(targetPosition);
                Vector2 targetLocalPos2D;
                targetLocalPos2D.x = targetLocalPos.x;
                targetLocalPos2D.y = targetLocalPos.z;
                targetAngle = Vector2.Angle(Vector2.up, targetLocalPos2D) * Mathf.Sign(targetLocalPos.x);
                targetAngle = Mathf.DeltaAngle(angleY, targetAngle);
                targetAngle += aimingScript.adjustAngle.x;
            }
            else
            { // Not tracking now. >> Return to the initial angle.
                targetAngle = Mathf.DeltaAngle(angleY, 0.0f);
                if (Mathf.Abs(targetAngle) < 0.01f)
                {
                    isTurning = false;
                }
            }

            // Calculate the turn rate.
            float sign = Mathf.Sign(targetAngle);
            targetAngle = Mathf.Abs(targetAngle);
            float currentSlowdownAng = Mathf.Abs(rotationSpeed * previousTurnRate) * decelerationTime;
            float targetTurnRate = Mathf.Lerp(0.0f, 1.0f, targetAngle / (rotationSpeed * Time.fixedDeltaTime + currentSlowdownAng)) * sign;
            if (targetAngle > currentSlowdownAng)
            {
                turnRate = Mathf.MoveTowards(turnRate, targetTurnRate, Time.fixedDeltaTime / accelerationTime);
            }
            else
            {
                turnRate = Mathf.MoveTowards(turnRate, targetTurnRate, Time.fixedDeltaTime / decelerationTime);
            }
            previousTurnRate = turnRate;

            // Rotate.
            angleY += rotationSpeed * turnRate * Time.fixedDeltaTime;
            currentLocalAngles.y = angleY;
            thisTransform.localEulerAngles = currentLocalAngles;
        }


        void Manual_Turn()
        {
            if (aimingScript.turretTurnRate != 0.0f)
            {
                isTurning = true;
            }

            if (isTurning == false)
            {
                return;
            }

            // Calculate the turn rate.
            float targetTurnRate = aimingScript.turretTurnRate;
            if (targetTurnRate != 0.0f)
            {
                turnRate = Mathf.MoveTowards(turnRate, targetTurnRate, Time.fixedDeltaTime / accelerationTime);
            }
            else
            {
                turnRate = Mathf.MoveTowards(turnRate, targetTurnRate, Time.fixedDeltaTime / decelerationTime);
            }
            if (turnRate == 0.0f)
            {
                isTurning = false;
            }

            // Rotate.
            angleY += rotationSpeed * turnRate * Time.fixedDeltaTime;
            currentLocalAngles.y = angleY;
            thisTransform.localEulerAngles = currentLocalAngles;
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".

            // Blow off the turret.
            var turretRigidbody = gameObject.AddComponent<Rigidbody>();
            turretRigidbody.mass = 100.0f;
            Vector3 addForceOffset;
            addForceOffset.x = Random.Range(-2.0f, 2.0f);
            addForceOffset.y = 0.0f;
            addForceOffset.z = Random.Range(-2.0f, 2.0f);
            turretRigidbody.AddForceAtPosition(thisTransform.up * Random.Range(1000.0f, 1500.0f), thisTransform.position + addForceOffset, ForceMode.Impulse);

            // Change the hierarchy.
            thisTransform.parent = parentTransform.parent; // Make it a child of the top object.

            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}