using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder(+1)] // (Note.) This script is executed after other scripts, in order to detect the target certainly.
    public class Aiming_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "MainBody" of the tank.
		 * This script controls the aiming of the tank.
		 * "Turret_Control_CS" and "Cannon_Control_CS" scripts rotate the turret and cannon referring to this variables.
		*/

		
        Turret_Control_CS turretControlScript;
        Cannon_Control_CS cannonControlScript;
        [HideInInspector] public bool useAutoTurn; // Referred to from "Turret_Control_CS" and "Cannon_Control_CS".

        // For auto-turn.
        [HideInInspector] public int mode; // Referred to from "AimMarker_Control_CS". // 0 => Keep the initial positon, 1 => Free aiming,  2 => Locking on.
        Transform rootTransform;
        Rigidbody bodyRigidbody;
        [HideInInspector] public Vector3 targetPosition; // Referred to from "Turret_Control_CS", "Cannon_Control_CS", "AimMarker_Control_CS".
        [HideInInspector] public Transform targetTransform; // Referred to from "AimMarker_Control_CS".
        [HideInInspector] public Vector3 adjustAngle; // Referred to from "Turret_Control_CS" and "Cannon_Control_CS".
        const float spherecastRadius = 3.0f;
        const float angleRange = 10.0f;

        // For manual-turn.
        [HideInInspector] public float turretTurnRate; // Referred to from "Turret_Control_CS".
        [HideInInspector] public float cannonTurnRate; // Referred to from "Cannon_Control_CS".

        Aiming_Control_Input_00_Base_CS inputScript;
        [HideInInspector] public bool isSelected; // Referred to from "AimMarker_Control_CS".


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            rootTransform = idScript.transform;
            bodyRigidbody = idScript.bodyRigidbody;

            // Get the "Turret_Horizontal_CS" and "Cannon_Vertical_CS" scripts in the tank.
            turretControlScript = GetComponentInChildren<Turret_Control_CS>();
            cannonControlScript = GetComponentInChildren<Cannon_Control_CS>();

            // Set the initial target position.
            targetPosition = transform.position + transform.forward * 100.0f;

            // Set the input script.
            Set_Input_Script();

            // Prepare the input script.
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            inputScript = gameObject.AddComponent<Aiming_Control_Input_01_Desktop_CS>();
#else
            inputScript = gameObject.AddComponent<Aiming_Control_Input_02_Mobile_CS>();
#endif
        }


        void Update()
        {
            if (isSelected && inputScript)
            {
                inputScript.Get_Input();
            }
        }


        void FixedUpdate()
        {
            // Update the target position.
            if (targetTransform)
            {
                Update_Target_Position();
            }
            else if (mode == 1)
            { // Free aiming.
                targetPosition += bodyRigidbody.velocity * Time.fixedDeltaTime;
            }
        }


        void Update_Target_Position()
        {
            // Check the target is living.
            if (targetTransform.root.tag == "Finish")
            { // The target has been destroyed.
                targetTransform = null;
                return;
            }

            // Update the target position.
            targetPosition = targetTransform.position;
        }


        public void Switch_Mode()
        { // Called also from "Aiming_Control_Input_##_###".
            switch (mode)
            {
                case 0: // Keep the initial positon.
                    targetTransform = null;
                    turretControlScript.Stop_Tracking();
                    cannonControlScript.Stop_Tracking();
                    break;

                case 1: // Free aiming.
                    targetTransform = null;
                    turretControlScript.Start_Tracking();
                    cannonControlScript.Start_Tracking();
                    break;

                case 2: // Locking on.
                    turretControlScript.Start_Tracking();
                    cannonControlScript.Start_Tracking();
                    break;
            }
        }


        public void Cast_Ray_Free(Vector3 screenPos)
        { // Called from "Aiming_Control_Input_##_###".
            // Find a target by casting a ray from the camera.
            var mainCamera = Camera.main;
            var ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit raycastHit, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask))
            {
                var colliderTransform = raycastHit.collider.transform;
                if (colliderTransform.root != rootTransform && colliderTransform.root.tag != "Finish")
                { // The hit collider is not itself, and is not destroyed.

                    // Check the rigidbody.
                    // (Note.) When the hit collider has no rigidbody and its parent has a rigidbody, the parent's transform is set as 'RaycastHit.transform'.
                    if (raycastHit.rigidbody)
                    { // The target has a rigidbody.
                        // Set the hit collider as the target.
                        targetTransform = colliderTransform;
                        targetPosition = raycastHit.point;
                        return;
                    } // The target does not have rigidbody.
                } // The ray hits itself, or the target is already dead.
            } // The ray does not hit anythig.

            // Set the position through this tank.
            targetTransform = null;
            screenPos.z = 128.0f;
            targetPosition = mainCamera.ScreenToWorldPoint(screenPos);
        }


        public void Reticle_Aiming(Vector3 screenPos)
        { // Called from "Aiming_Control_Input_##_###".

            // Find a target by casting a sphere from the camera.
            var ray = Camera.main.ScreenPointToRay(screenPos);
            var raycastHits = Physics.SphereCastAll(ray, spherecastRadius, 2048.0f, Layer_Settings_CS.Aiming_Layer_Mask);
            for (int i = 0; i < raycastHits.Length; i++)
            {
                Transform colliderTransform = raycastHits[i].collider.transform;
                if (colliderTransform.root != rootTransform && colliderTransform.root.tag != "Finish")
                { // The hit collider is not itself, and is not destroyed.

                    // Check the rigidbody.
                    // (Note.) When the hit collider has no rigidbody and its parent has a rigidbody, the parent's transform is set as 'RaycastHits.transform'.
                    var tempRigidbody = raycastHits[i].rigidbody;
                    if (tempRigidbody == null)
                    {
                        continue;
                    }

                    // Check the layer.
                    if (tempRigidbody.gameObject.layer != Layer_Settings_CS.Body_Layer)
                    { // The target is not a MainBody.
                        continue;
                    }

                    // Check the obstacle.
                    if (Physics.Linecast(ray.origin, raycastHits[i].point, out RaycastHit raycastHit, Layer_Settings_CS.Aiming_Layer_Mask))
                    { // The target is obstructed by anything.
                        if (raycastHit.transform.root != rootTransform)
                        { // The obstacle is not itself.
                            continue;
                        }
                    }

                    // Set the MainBody as the target.
                    targetTransform = raycastHits[i].transform;
                    targetPosition = raycastHits[i].point;
                    adjustAngle = Vector3.zero;
                    return;
                }
            } // Target with a rigidbody cannot be found.
        }


        public void Touch_Lock(Vector3 screenPos)
        { // Find a target near the touch position.

            // Check the "Game_Controller_CS" is set in the scene.
            if (Game_Controller_CS.instance == null)
            {
                return;
            }

            // Get the tank list from the "Game_Controller_CS".
            List<ID_Control_CS> idScriptsList = Game_Controller_CS.instance.idScriptsList;

            // Find a new target.
            var mainCamera = Camera.main;
            var ray = mainCamera.ScreenPointToRay(screenPos);
            var camVector = ray.direction;
            var camPos = mainCamera.transform.position;
            var targetIndex = 0;
            var minAng = angleRange;
            for (int i = 0; i < idScriptsList.Count; i++)
            {
                if (idScriptsList[i].bodyTransform == null)
                { // The tank has been removed from the scene.
                    continue;
                }

                if (idScriptsList[i].bodyTransform.root.tag == "Finish" || (targetTransform && targetTransform == idScriptsList[i].bodyTransform))
                { // The tank is dead, or the tank is the same as the current target.
                    continue;
                }

                if (idScriptsList[i].bodyTransform.root == rootTransform)
                { // The tank is itself.
                    continue;
                }

                // Get the angle of the both vectors.
                var tankVector = idScriptsList[i].bodyTransform.position - camPos;
                var deltaAng = Mathf.Acos(Vector3.Dot(tankVector, camVector) / (tankVector.magnitude * camVector.magnitude)) * Mathf.Rad2Deg;
                if (deltaAng < minAng)
                {
                    targetIndex = i;
                    minAng = deltaAng;
                }
            }

            if (minAng != angleRange)
            { // Target is found.
                targetTransform = idScriptsList[targetIndex].bodyTransform;
                mode = 2; // Lock on.
                Switch_Mode();
                StartCoroutine("Send_Target_Position");
            }
            else
            { // Target cannot be found.
                // Get the angle to this tank.
                var tankVector = transform.position - camPos;
                var deltaAng = Mathf.Acos(Vector3.Dot(tankVector, camVector) / (tankVector.magnitude * camVector.magnitude)) * Mathf.Rad2Deg;
                if (deltaAng < minAng)
                { // This tank is near the touch position.
                    // Lock off.
                    mode = 0; // Keep the initial position.
                    Switch_Mode();
                }
            }
        }


        public IEnumerator Send_Target_Position()
        {
            // Send the target position to the "Camera_Rotation_CS" in the "Camera_Manager" object in the scene.
            yield return new WaitForFixedUpdate();
            if (Camera_Rotate_CS.instance)
            {
                Camera_Rotate_CS.instance.Look_At_Target(targetTransform.position);
            }
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            Destroy(inputScript as Object);
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }


        void Selected(bool isSelected)
        { // Called from "ID_Control_CS".

            this.isSelected = isSelected;

            if (isSelected == false)
            {
                return;
            }

            // Call the "AimMarker_Control_CS" in the scene.
            if (AimMarker_Control_CS.instance)
            {
                AimMarker_Control_CS.instance.Get_Aiming_Control_Script(this);
            }

        }
    }

}