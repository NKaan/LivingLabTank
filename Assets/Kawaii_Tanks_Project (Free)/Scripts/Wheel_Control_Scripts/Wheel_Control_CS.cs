using UnityEngine;
using System.Collections;


namespace ChobiAssets.KTP
{

    public class Wheel_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "MainBody" of the tank.
		 * This script controls the movement of the tank.
		 * "Wheel_Rotate_CS" script rotates the wheel referring to this variables.
         * Also, some scripts refer to this script to get the current tank speed.
		*/

        [Header("Driving settings")]
        [Tooltip("Torque added to each wheel")] public float wheelTorque = 5000.0f; // Referred to from "Wheel_Rotate_CS".
        [Tooltip("Maximum speed (meter per second)")] public float maxSpeed = 10.0f; // Referred to from "Wheel_Rotate_CS" and "SE_Control_CS".
        [Tooltip("Downforce added to the body")] public float downforce = 25000.0f;
        [Tooltip("Set the distance from the body's pivot to the ground.")] public float rayDistance = 1.0f;
        [Tooltip("Rate for ease of pivot-turning"), Range(0.0f, 1.0f)] public float pivotTurnClamp = 0.5f;
        [Tooltip("Makes it easier to turn")] public bool supportBrakeTurn = false;
        [Tooltip("Makes it easier to turn")] public float supportBrakeTurnTorque = 50000.0f;


        // Referred to from "Wheel_Rotate_CS".
        [HideInInspector] public float wheelLeftRate;
        [HideInInspector] public float wheelRightRate;

        // Referred to from "SE_Control_CS".
        [HideInInspector] public float currentVelocityMag;
        [HideInInspector] public float currentAngularVelocityMag;

        // Set by "Wheel_Control_Input_##_###_CS".
        [HideInInspector] public Vector2 moveAxis;

        // Referret to from "Wheel_Control_Input_##_###_CS".
        [HideInInspector] public bool isSelected;

        Transform bodyTransform;
        Rigidbody bodyRigidbody;
        bool parkingBrake = false;
        float stoppingTime;
        const float parkingBrakeVelocity = 0.5f;
        const float parkingBrakeLag = 0.5f;

        Wheel_Control_Input_00_Base_CS inputScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            bodyTransform = idScript.bodyTransform;
            bodyRigidbody = idScript.bodyRigidbody;

            // Set the input script.
            Set_Input_Script();

            // Prepare the input script.
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            inputScript = gameObject.AddComponent<Wheel_Control_Input_01_Desktop_CS>();
#else
            inputScript = gameObject.AddComponent<Wheel_Control_Input_02_Mobile_CS>();
#endif
        }


        void Update()
        {
            // Get the input.
            inputScript.Get_Input();

            // Control the speed.
            Speed_Control();

            // Get the current velocity.
            currentVelocityMag = bodyRigidbody.velocity.magnitude;
            currentAngularVelocityMag = bodyRigidbody.angularVelocity.magnitude;
        }


        void Speed_Control()
        {
            if (moveAxis.y < 0.0f)
            { // Going backward.
                // Behave as brake-turn.
                moveAxis.x = -moveAxis.x;
            }

            if (moveAxis.y != 0.0f)
            { // Brake Turn.
                var clamp = Mathf.Abs(moveAxis.y);
                moveAxis.x = Mathf.Clamp(moveAxis.x, -clamp, clamp);
                wheelLeftRate = Mathf.Clamp(-moveAxis.y - moveAxis.x, -1.0f, 1.0f);
                wheelRightRate = Mathf.Clamp(moveAxis.y - moveAxis.x, -1.0f, 1.0f);
            }
            else
            { // Pivot Turn.
                moveAxis.x = Mathf.Clamp(moveAxis.x, -pivotTurnClamp, pivotTurnClamp);
                wheelLeftRate = -moveAxis.x;
                wheelRightRate = -moveAxis.x;
            }
        }


        void FixedUpdate()
        {
            Control_Parking_Brake();
            Anti_Spin();
            Add_Downforce();
            Anti_Slip();

            if (supportBrakeTurn)
            {
                Support_Brake_Turn();
            }
        }


        void Control_Parking_Brake()
        {
            // Auto Parking Brake using 'RigidbodyConstraints'.
            if (moveAxis.y == 0.0f && moveAxis.x == 0.0f)
            { // No input for driving.

                if (parkingBrake)
                { // The parking brake is working now.
                    // Check the Rigidbody velocities.
                    if (currentVelocityMag > parkingBrakeVelocity || currentAngularVelocityMag > parkingBrakeVelocity)
                    { // The Rigidbody should have been moving by receiving external force.
                        // Release the parking brake.
                        parkingBrake = false;
                        bodyRigidbody.constraints = RigidbodyConstraints.None;
                        stoppingTime = 0.0f;
                        return;
                    } // The Rigidbody almost stops.
                    return;
                }
                else
                { // The parking brake is not working.
                    // Check the Rigidbody velocities.
                    if (currentVelocityMag < parkingBrakeVelocity && currentAngularVelocityMag < parkingBrakeVelocity)
                    { // The Rigidbody almost stops.
                        // Count the stopping time.
                        stoppingTime += Time.fixedDeltaTime;
                        if (stoppingTime > parkingBrakeLag)
                        { // The stopping time has been over the "parkingBrakeLag".
                            // Put on the parking brake.
                            parkingBrake = true;
                            bodyRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationY;
                            return;
                        }
                        else
                        { // The stopping time has not over yet.
                            return;
                        }
                    } // The Rigidbody almost stops.
                    return;
                }
            }
            else
            { // The tank should be driving now.
                if (parkingBrake == true)
                { // The parking brake is still working.
                    // Release parking brake.
                    parkingBrake = false;
                    bodyRigidbody.constraints = RigidbodyConstraints.None;
                    stoppingTime = 0.0f;
                }
            }         
        }


        void Anti_Spin()
        {
            // Stop Spinning.
            if (moveAxis.x == 0.0f)
            { // The tank should not be turning.
                // Control the angular velocity of the Rigidbody.
                Vector3 currentAngularVelocity = bodyRigidbody.angularVelocity;
                currentAngularVelocity.y = 0.0f; // Make the angular velocity on Y-axis zero.
                // Set the new angular velocity.
                bodyRigidbody.angularVelocity = currentAngularVelocity;
            }
        }


        void Add_Downforce()
        {
            // Add downforce.
            bodyRigidbody.AddRelativeForce(Vector3.up * -downforce);
        }


        void Anti_Slip()
        {
            // Reduce the slippage by controling the velocity of the Rigidbody.

            // Cast the ray downward to detect the ground.
            var ray = new Ray();
            ray.origin = bodyTransform.position;
            ray.direction = -bodyTransform.up;
            if (Physics.Raycast(ray, rayDistance, Layer_Settings_CS.Anti_Slipping_Layer_Mask) == true)
            { // The ray hits the ground.

                // Control the velocity of the Rigidbody.
                Vector3 currentVelocity = bodyRigidbody.velocity;
                if (moveAxis.y == 0.0f && moveAxis.x == 0.0f)
                { // The tank should stop.
                    // Reduce the Rigidbody velocity gradually.
                    currentVelocity.x *= 0.9f;
                    currentVelocity.z *= 0.9f;
                }
                else
                { // The tank should been driving.
                    float sign;
                    if (moveAxis.y != 0.0f && moveAxis.x == 0.0f)
                    { // The tank should be going straight forward or backward.
                        if (Mathf.Abs(moveAxis.y) < 0.2f)
                        { // The tank almost stops.
                            // Cancel the function, so that the tank can smoothly switches the direction forward and backward.
                            return;
                        }
                        sign = Mathf.Sign(moveAxis.y);
                    }
                    else if (moveAxis.y == 0.0f && moveAxis.x != 0.0f)
                    { // The tank should be doing pivot-turn.
                        sign = 1.0f;
                    }
                    else
                    { // The tank should be doing brake-turn.
                        sign = Mathf.Sign(moveAxis.y);
                    }
                    // Change the velocity of the Rigidbody forcibly.
                    currentVelocity = Vector3.MoveTowards(currentVelocity, bodyTransform.forward * sign * currentVelocityMag, 32.0f * Time.fixedDeltaTime);
                }

                // Set the new velocity.
                bodyRigidbody.velocity = currentVelocity;
            }
        }


        void Support_Brake_Turn()
        {
            bodyRigidbody.AddRelativeTorque(Vector3.up * moveAxis.x * supportBrakeTurnTorque * Mathf.Abs(moveAxis.y));
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            StartCoroutine("Disable_Constraints");
        }


        IEnumerator Disable_Constraints()
        {
            // Disable constraints of MainBody's rigidbody.
            yield return new WaitForFixedUpdate(); // This wait is required for PhysX.
            bodyRigidbody.constraints = RigidbodyConstraints.None;
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }


        void Selected(bool isSelected)
        { // Called from "ID_Control_CS".

            this.isSelected = isSelected;
        }
    }

}
