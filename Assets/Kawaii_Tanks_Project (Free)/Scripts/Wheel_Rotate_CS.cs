using UnityEngine;
using System.Collections;


namespace ChobiAssets.KTP
{

    public class Wheel_Rotate_CS : MonoBehaviour
    {

        /*
         * This script is attached to all the driving wheels in the tank.
         * This script controls the rotation speed and the torque of the wheel.
         * This script works in cooperation with "Wheel_Control_CS" in the tank.
        */

        bool isLeft;
        Rigidbody thisRigidbody;
        float maxAngVelocity;
        Wheel_Control_CS controlScript;
        Transform thisTransform;
        Vector3 initialAngles;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            thisRigidbody = GetComponent<Rigidbody>();

            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            controlScript = idScript.wheelControlScript;

            // Set the layer.
            gameObject.layer = Layer_Settings_CS.Wheels_Layer;

            // Get the direction.
            if (transform.localPosition.y > 0.0f)
            {
                isLeft = true;
            }
            else
            {
                isLeft = false;
            }

            // Store the initial angles.
            initialAngles = thisTransform.localEulerAngles;

            // Set the maximum angualr velocity.
            var radius = GetComponent<SphereCollider>().radius;
            maxAngVelocity = Mathf.Deg2Rad * ((controlScript.maxSpeed / (2.0f * Mathf.PI * radius)) * 360.0f);
        }


        void Update()
        {
            Stabilize_Wheel();
        }


        void FixedUpdate()
        {
            Drive_Wheel();
        }


        void Drive_Wheel()
        {
            // Get the speed rate.
            float rate;
            if (isLeft)
            {
                rate = controlScript.wheelLeftRate;
            }
            else
            {
                rate = controlScript.wheelRightRate;
            }

            // Add torque.
            thisRigidbody.AddRelativeTorque(0.0f, Mathf.Sign(rate) * controlScript.wheelTorque, 0.0f);

            // Set the maximum angular velocity.
            thisRigidbody.maxAngularVelocity = Mathf.Abs(maxAngVelocity * rate);
        }


        void Stabilize_Wheel()
        {
            // Stabilize the angle.
            initialAngles.y = thisTransform.localEulerAngles.y;
            thisTransform.localEulerAngles = initialAngles;
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            thisRigidbody.angularDrag = Mathf.Infinity;
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}
