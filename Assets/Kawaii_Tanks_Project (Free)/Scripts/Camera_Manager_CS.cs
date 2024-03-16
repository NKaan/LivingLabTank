using UnityEngine;


namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder(+1)] // (Note.) This script is executed after other scripts, in order to move the camera smoothly.
    public class Camera_Manager_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Camera_Manager" object in the scene.
		 * This script moves this object following the player's tank.
         * (Note.) There should be only one "Camera_Manager" in the scene. 
		*/

        [Header("Main camera settings")]
        [Tooltip("Set the main camera.")] public Camera mainCamera;


        Transform thisTransform;
        Transform targetTranform;
        float offset;


        [HideInInspector] public static Camera_Manager_CS instance;


        void Awake()
        { // The camera components must be setup before Start().
            Initialize();
        }


        void Initialize()
        {
            instance = this;
            thisTransform = transform;

            // Setup the main camera.
            if (mainCamera == null)
            {
                mainCamera = GetComponentInChildren<Camera>();
            }
            mainCamera.enabled = true;
        }


        void FixedUpdate()
        {
            // Check the target exists.
            if (targetTranform == null)
            { // The target has been lost.
                return;
            }

            // Follow the target.
            thisTransform.position = targetTranform.position + (Vector3.up * offset);
        }


        public void Set_Follow_Target(Transform targetTranform, float offset)
        { // Called from "ID_Control_CS".

            // Store the value.
            this.targetTranform = targetTranform;
            this.offset = offset;

            // Set the angles.
            var currentAngles = thisTransform.eulerAngles;
            currentAngles.y = targetTranform.eulerAngles.y;
            thisTransform.eulerAngles = currentAngles;

            // Call the "Camera_Rotate_CS" to reset the angles.
            if (Camera_Rotate_CS.instance)
            {
                Camera_Rotate_CS.instance.Reset();
            }

            // Call the "Camera_Avoid_Obstacle_CS" to send the target reference.
            if (Camera_Avoid_Obstacle_CS.instance)
            {
                Camera_Avoid_Obstacle_CS.instance.Get_Target(targetTranform);
            }

            FixedUpdate();
        }


        public void Control_Main_Camera(bool isEnabled)
        { // Called from "GunCamera_Control_CS".

            // Turn on/off the main camera.
            mainCamera.enabled = isEnabled;
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }
    }

}