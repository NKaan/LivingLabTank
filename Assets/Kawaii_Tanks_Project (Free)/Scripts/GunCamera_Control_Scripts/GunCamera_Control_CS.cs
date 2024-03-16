using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    public class GunCamera_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Gun_Camera" under the "Barrel_Base" in the tank.
		 * This script controls the gun camera used for aiming the target.
		 * The main camera and the gun camera are switched by this script.
		*/

        [Header("Gun Camera settings")]
        [Tooltip("'Set the gun camera.")] public Camera gunCamera;
        [Tooltip("Minimum FOV")] public float minFOV = 5.0f;
        [Tooltip("Maximum FOV")] public float maxFOV = 50.0f;
        [Tooltip("Name of the reticle image in the scene.")] public string reticleName = "Reticle";


        //Set by "Guncamera_Control_Input_##_###_CS" scripts.
        [HideInInspector] public float zoomInput;

        Transform thisTransform;
        float targetFOV;
        float currentFOV;
        Image reticleImage;
        GunCamera_Control_Input_00_Base_CS inputScript;


        bool isSelected;


        void Awake()
        { // (Note.) These variables must be set before Start().
            thisTransform = transform;

            // Find the reticle image.
            if (string.IsNullOrEmpty(reticleName) == false)
            {
                var reticleObject = GameObject.Find(reticleName);
                if (reticleObject)
                {
                    reticleImage = reticleObject.GetComponent<Image>();
                }
            }
            if (reticleImage == null)
            {
                Debug.LogWarning(reticleName + " (Image for Reticle) cannot be found in the scene.");
            }
        }


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {

            // Check the "Camera_Manager_CS" exists in the scene.
            if (Camera_Manager_CS.instance == null)
            {
                Debug.LogError("'Camera_Manager_CS' cannot be found in the scene.");
                Destroy(this);
                return;
            }

            // Check the "Camera_Rotate_CS" exists in the scene.
            if (Camera_Rotate_CS.instance == null)
            {
                Debug.LogError("'Camera_Rotate_CS' cannot be found in the scene.");
                Destroy(this);
                return;
            }

            // Setup the gun camera.
            this.tag = "MainCamera";
            if (gunCamera == null)
            {
                gunCamera = GetComponent<Camera>();
            }
            gunCamera.enabled = false;
            currentFOV = gunCamera.fieldOfView;
            targetFOV = currentFOV;

            // Set the input script.
            Set_Input_Script();

            // Prepare the input script.
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            inputScript = gameObject.AddComponent<GunCamera_Control_Input_01_Desktop_CS>();
#else
            inputScript = gameObject.AddComponent<GunCamera_Control_Input_02_Mobile_CS>();
#endif
        }


        void Update()
        {
            if (isSelected == false)
            {
                return;
            }


            inputScript.Get_Input();

            if (gunCamera.enabled)
            {
                Zoom();
            }
        }


        public void Switch_Mode(int mode)
        { // Called from "GunCamera_Control_Input_##_###_CS" scripts.
            switch (mode)
            {
                case 0: // Off.
                    Camera_Manager_CS.instance.Control_Main_Camera(true);
                    if (isSelected)
                    {
                        Camera_Rotate_CS.instance.Enable_Camera(thisTransform.position + thisTransform.forward * 64.0f);
                    }
                    gunCamera.enabled = false;
                    this.tag = "Untagged";
                    reticleImage.enabled = false;
                    break;

                case 1: // On.
                    Camera_Manager_CS.instance.Control_Main_Camera(false);
                    gunCamera.enabled = true;
                    this.tag = "MainCamera";
                    reticleImage.enabled = true;
                    break;
            }
        }


        float currentZoomVelocity;
        void Zoom()
        {
            targetFOV *= 1.0f + zoomInput;
            targetFOV = Mathf.Clamp(targetFOV, minFOV, maxFOV);

            if (currentFOV != targetFOV)
            {
                currentFOV = Mathf.SmoothDamp(currentFOV, targetFOV, ref currentZoomVelocity, 2.0f * Time.deltaTime);
                gunCamera.fieldOfView = currentFOV;
            }
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            if (gunCamera.enabled)
            {
                Switch_Mode(0); // Off
            }
            Destroy(this.gameObject);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }


        void Selected(bool isSelected)
        { // Called from "ID_Control_CS".

            this.isSelected = isSelected;

            if (isSelected == false && gunCamera.enabled)
            {
                // Turn off the gun camera.
                Switch_Mode(0);
            }
        }
    }

}
