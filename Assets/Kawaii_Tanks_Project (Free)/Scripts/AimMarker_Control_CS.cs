using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder(+2)] // (Note.) This script is executed after the "Aiming_Control_CS", in order to move the marker smoothly.
    public class AimMarker_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to "Aim_Marker" in the scene.
		 * The appearance and position of the marker are controlled by this script.
		 * This script works in combination with the "Aiming_Control_CS" in the tank.
		*/


        Transform thisTransform;
        Image thisImage;
        Aiming_Control_CS aimingScript;


        [HideInInspector] public static AimMarker_Control_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = GetComponent<Transform>();
            thisImage = GetComponent<Image>();
        }


        public void Get_Aiming_Control_Script(Aiming_Control_CS aimingScript)
        { // Called from "Aiming_Control_CS" in the player's tank.
            this.aimingScript = aimingScript;
        }


        void Update()
        {
            Marker_Control();
        }


        void Marker_Control()
        {
            if (aimingScript == null)
            { // The tank has been destroyed.
                thisImage.enabled = false;
                return;
            }

            if (aimingScript.isSelected == false)
            { // The tank is not selected now.
                thisImage.enabled = false;
                return;
            }


            // Set the appearance.
            switch (aimingScript.mode)
            {
                case 0: // Keep the initial positon.
                    thisImage.enabled = false;
                    return;
                case 1: // Free aiming.
                case 2: // Locking on.
                    thisImage.enabled = true;
                    if (aimingScript.targetTransform)
                    {
                        thisImage.color = Color.red;
                    }
                    else
                    {
                        thisImage.color = Color.white;
                    }
                    break;
            }

            // Set the position.
            var currentPosition = Camera.main.WorldToScreenPoint(aimingScript.targetPosition);
            if (currentPosition.z < 0.0f)
            { // Behind of the camera.
                thisImage.enabled = false;
            }
            else
            { // Front of the camera.
                currentPosition.z = 128.0f;
            }
            thisTransform.position = currentPosition;
        }

    }

}