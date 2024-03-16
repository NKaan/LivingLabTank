using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Zoom_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the main camera in "Camera_Manager" object in the scene..
		 * This script controls the FOV (Field Of View) of the main camera.
		*/

        [Header("Main Camera settings")]
        [Tooltip("Set the main camera.")] public Camera mainCamera;
        [Tooltip("Set the minimum Field Of View.")] public float Min_FOV = 30.0f;
        [Tooltip("Set the maximum Field Of View.")] public float Max_FOV = 80.0f;


        // Set by "Camera_Zoom_Input_##_###_CS".
        [HideInInspector] public float zoomInput;

        float targetFOV;
        float currentFOV;

        Camera_Zoom_Input_00_Base_CS inputScript;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the camera.
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
            }
            currentFOV = mainCamera.fieldOfView;
            targetFOV = currentFOV;

            // Set the "inputScript".
            Set_Input_Script();

            // Prepare the input script.
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            inputScript = gameObject.AddComponent<Camera_Zoom_Input_01_Desktop_CS>();
#else
            inputScript = gameObject.AddComponent<Camera_Zoom_Input_02_Mobile_CS>();
#endif
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

            // Zoom the main camera.
            Zoom();
        }


        float currentZoomVelocity;
        void Zoom()
        {
            targetFOV *= 1.0f + zoomInput;
            targetFOV = Mathf.Clamp(targetFOV, Min_FOV, Max_FOV);

            if (currentFOV != targetFOV)
            {
                currentFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref currentZoomVelocity, 2.0f * Time.deltaTime);
                mainCamera.fieldOfView = currentFOV;
            }
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }

    }

}
