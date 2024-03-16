using UnityEngine;
using System.Collections;


namespace ChobiAssets.KTP
{

    public class Camera_Rotate_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Camera_Pivot" in the "Camera_Manager" object in the scene.
		 * This script rotates the camera pivot.
		*/

        [Header("Main camera settings")]
        [Tooltip("Set the main camera.")] public Camera mainCamera;
        [Tooltip("Use this for demonstration.")] public bool useDemoCamera;


        // Set by "Camera_Rotate_Input_##_###_CS".
        [HideInInspector] public Vector3 rotationInput;

        Transform thisTransform;
        Transform parentTransform;
        Vector3 initialAngles;
        Vector3 currentAngles;
        Vector3 targetAngles;
        Camera_Rotate_Input_00_Base_CS inputScript;

        [HideInInspector] public static Camera_Rotate_CS instance;


        void Awake()
        { // The camera components must be setup before Start().
            Initialize();
        }


        void Initialize()
        {
            instance = this;
            thisTransform = transform;
            parentTransform = thisTransform.parent;
            initialAngles = thisTransform.localEulerAngles;
            currentAngles = initialAngles;
            targetAngles = currentAngles;

            // Get the main camera.
            if (mainCamera == null)
            {
                mainCamera = GetComponentInChildren<Camera>();
            }

            // Set the input script.
            Set_Input_Script();

            // Prepare the input script.
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
            if (useDemoCamera)
            { // In the case of demo camera.
                // Set the script for demo camera.
                inputScript = gameObject.AddComponent<Camera_Rotate_Input_88_Demo_CS>();
            }
            else
            { // In the case of player's camera.
#if !UNITY_ANDROID && !UNITY_IPHONE
                // Set the script for desktop input.
                inputScript = gameObject.AddComponent<Camera_Rotate_Input_01_Desktop_CS>();
#else
                // Set the script for mobile input.
                inputScript = gameObject.AddComponent<Camera_Rotate_Input_02_Mobile_CS>();
#endif
            }
        }


        void Update()
        {
            // Check the camera.
            if (mainCamera.enabled == false)
            { // It should be aiming now.
                return;
            }

            // Get the input.
            inputScript.Get_Input();
        }


        void FixedUpdate()
        {
            // Check the camera.
            if (mainCamera.enabled == false)
            { // It should be aiming now.
                return;
            }

            // Rotate the camera pivot(this object).
            Rotate();
        }


        Vector3 currentRotationVelocity;
        void Rotate()
        {
            // Set the target angles.
            targetAngles += rotationInput;

            // Clamp the angles.
            targetAngles.z = Mathf.Clamp(targetAngles.z, -10.0f, 90.0f);

            // Rotate smoothly.
            currentAngles.y = Mathf.SmoothDampAngle(currentAngles.y, targetAngles.y, ref currentRotationVelocity.y, 2.0f * Time.fixedDeltaTime);
            currentAngles.z = Mathf.SmoothDampAngle(currentAngles.z, targetAngles.z, ref currentRotationVelocity.z, 2.0f * Time.fixedDeltaTime);
            thisTransform.localEulerAngles = currentAngles;
        }


        public void Look_At_Target(Vector3 targetPos)
        { // Called from "Aiming_Control_CS" in the player's tank.
            var targetDir = (targetPos - thisTransform.position).normalized;
            targetDir.y = 0.0f;
            targetAngles.y = Vector3.Angle(Vector3.forward, targetDir) * Mathf.Sign(targetDir.x) - (parentTransform.eulerAngles.y - initialAngles.y);
        }


        public void Reset()
        { // Called from "Camera_Manager_CS".
            currentAngles = initialAngles;
            targetAngles = currentAngles;
            FixedUpdate();
        }


        public void Enable_Camera(Vector3 targetPos)
        { // Called from "GunCamera_Control_CS".

            // Look at the same direction as the gun camera quickly.
            Look_At_Target(targetPos);
            currentAngles = targetAngles;
            FixedUpdate();
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}