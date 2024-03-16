using UnityEngine;
using System.Collections;

// This script must be attached to "Cannon_Base".
namespace ChobiAssets.KTP
{

    public class Cannon_Control_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to the "Cannon_Base" in the tank.
		 * This script rotates the cannon vertically.
		 * This script works in combination with "Aiming_Control_CS" in the MainBody.
		*/

        [Header("Cannon movement settings")]
        [Tooltip("Rotation speed. (Degree per second)")] public float rotationSpeed = 10.0f;
        [Tooltip("Time to reach the maximum speed from zero. (Sec)")] public float accelerationTime = 0.2f;
        [Tooltip("Time to stop from the maximum speed. (Sec)")] public float decelerationTime = 0.2f;
        [Tooltip("Maximum elevation angle. (Degree)")] public float maxElevation = 15.0f;
        [Tooltip("Maximum depression angle. (Degree)")] public float maxDepression = 10.0f;


        Transform thisTransform;
        Transform turretBaseTransform;
        Aiming_Control_CS aimingScript;
        bool isTurning;
        bool isTracking;
        float angleX;
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
            turretBaseTransform = thisTransform.parent;
            currentLocalAngles = thisTransform.localEulerAngles;
            angleX = currentLocalAngles.x;
            maxElevation = angleX - maxElevation;
            maxDepression = angleX + maxDepression;

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
            { // Tracking the target.
                // Calculate the target angle.
                targetAngle = Auto_Elevation_Angle();
                targetAngle += Mathf.DeltaAngle(0.0f, angleX) + aimingScript.adjustAngle.y;
            }
            else
            { // Not tracking. >> Return to the initial angle.
                targetAngle = -Mathf.DeltaAngle(angleX, 0.0f);
                if (Mathf.Abs(targetAngle) < 0.01f)
                {
                    isTurning = false;
                }
            }

            // Calculate the turn rate.
            float sign = Mathf.Sign(targetAngle);
            targetAngle = Mathf.Abs(targetAngle);
            float currentSlowdownAng = Mathf.Abs(rotationSpeed * previousTurnRate) * decelerationTime;
            float targetTurnRate = -Mathf.Lerp(0.0f, 1.0f, targetAngle / (rotationSpeed * Time.fixedDeltaTime + currentSlowdownAng)) * sign;
            if (targetAngle > currentSlowdownAng)
            {
                turnRate = Mathf.MoveTowards(turnRate, targetTurnRate, Time.fixedDeltaTime / accelerationTime);
            }
            else
            {
                turnRate = Mathf.MoveTowards(turnRate, targetTurnRate, Time.fixedDeltaTime / decelerationTime);
            }
            angleX += rotationSpeed * turnRate * Time.fixedDeltaTime;
            previousTurnRate = turnRate;

            // Rotate
            angleX = Mathf.Clamp(angleX, maxElevation, maxDepression);
            currentLocalAngles.x = angleX;
            thisTransform.localEulerAngles = currentLocalAngles;
        }


        float Auto_Elevation_Angle()
        {
            // Get the target position.
            var targetPosition = aimingScript.targetPosition;

            // Calculate the proper angle.
            float properAngle;
            Vector2 targetPos2D;
            targetPos2D.x = targetPosition.x;
            targetPos2D.y = targetPosition.z;
            Vector2 thisPos2D;
            thisPos2D.x = thisTransform.position.x;
            thisPos2D.y = thisTransform.position.z;
            Vector2 dist;
            dist.x = Vector2.Distance(targetPos2D, thisPos2D);
            dist.y = targetPosition.y - thisTransform.position.y;
            float posBase = (Physics.gravity.y * Mathf.Pow(dist.x, 2.0f)) / (2.0f * Mathf.Pow(bulletVelocity, 2.0f));
            float posX = dist.x / posBase;
            float posY = (Mathf.Pow(posX, 2.0f) / 4.0f) - ((posBase - dist.y) / posBase);
            if (posY >= 0.0f)
            {
                properAngle = Mathf.Rad2Deg * Mathf.Atan(-posX / 2.0f - Mathf.Pow(posY, 0.5f));
            }
            else
            { // The bullet cannot reach the target.
                properAngle = 45.0f;
            }

            // Add the tilt angle of the tank.
            Vector3 forwardPos = turretBaseTransform.forward;
            Vector2 forwardPos2D;
            forwardPos2D.x = forwardPos.x;
            forwardPos2D.y = forwardPos.z;
            properAngle -= Mathf.Rad2Deg * Mathf.Atan(forwardPos.y / Vector2.Distance(Vector2.zero, forwardPos2D));
            return properAngle;
        }


        float Manual_Elevation_Angle()
        {
            // Simply face the target.
            float directAngle;
            Vector3 localPos = turretBaseTransform.InverseTransformPoint(aimingScript.targetPosition);
            directAngle = Mathf.Rad2Deg * (Mathf.Asin((localPos.y - thisTransform.localPosition.y) / Vector3.Distance(thisTransform.localPosition, localPos)));
            return directAngle;
        }


        void Manual_Turn()
        {
            if (aimingScript.cannonTurnRate != 0.0f)
            {
                isTurning = true;
            }

            // Calculate the "turnRate".
            float targetTurnRate = aimingScript.cannonTurnRate;
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
            angleX += rotationSpeed * turnRate * Time.fixedDeltaTime;
            angleX = Mathf.Clamp(angleX, maxElevation, maxDepression);
            currentLocalAngles.x = angleX;
            thisTransform.localEulerAngles = currentLocalAngles;
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".

            // Depress the cannon.
            currentLocalAngles.x = maxDepression;
            thisTransform.localEulerAngles = currentLocalAngles;

            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}
