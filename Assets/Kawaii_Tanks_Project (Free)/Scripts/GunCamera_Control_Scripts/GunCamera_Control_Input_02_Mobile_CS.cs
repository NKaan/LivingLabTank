using UnityEngine;


namespace ChobiAssets.KTP
{

    public class GunCamera_Control_Input_02_Mobile_CS : GunCamera_Control_Input_00_Base_CS
    {

#if UNITY_ANDROID || UNITY_IPHONE

        bool isAimButtonDown;
        float pressingCount;

        bool isZoomButtonDown = false;
        Vector2 previousTouchPos;


        public override void Get_Input()
        {
            Switch();
            Zooming();
        }


        void Switch()
        {
            // Start switching process.
            if (isAimButtonDown == false && Key_Bindings_CS.IsAimButtonPressing)
            { // Aiming button is pressed.
                isAimButtonDown = true;
                pressingCount = 0.0f;
                return;
            }

            // Switch on the gun camera according to the pressing time.
            if (isAimButtonDown && Key_Bindings_CS.IsAimButtonPressing)
            { // Aiming button is being pressed.
                if (gunCameraScript.gunCamera.enabled)
                { // The gun camera has already been switched on.
                    return;
                }

                if (pressingCount < 0.25f)
                {
                    pressingCount += Time.deltaTime;
                    return;
                }

                // Switch on the gun camera.
                gunCameraScript.Switch_Mode(1); // On
                return;
            }

            // Switch off the gun camera.
            if (isAimButtonDown && Key_Bindings_CS.IsAimButtonPressing == false)
            { // Aiming button is released.
                pressingCount = 0.0f;
                isAimButtonDown = false;
                isZoomButtonDown = false;
                gunCameraScript.zoomInput = 0.0f;

                // Switch off the gun camera.
                if (gunCameraScript.gunCamera.enabled)
                { // The gun camera is enabled.
                    gunCameraScript.Switch_Mode(0); // Off
                }
                return;
            }
        }


        void Zooming()
        {
            if (gunCameraScript.gunCamera.enabled == false)
            { // The gun camera is disabled.
                return;
            }

            // Start zooming.
            if (isZoomButtonDown == false && Key_Bindings_CS.IsZoomButtonPressing)
            { // Zoom button is pressed.
                isZoomButtonDown = true;
#if UNITY_EDITOR
                previousTouchPos = Input.mousePosition;
#else				
                previousTouchPos = Key_Bindings_CS.ZoomButtonStartPosition;
#endif
                return;
            }

            // Zooming.
            if (isZoomButtonDown && Key_Bindings_CS.IsZoomButtonPressing)
            { // Zoom button is being pressed.
#if UNITY_EDITOR
                var currentTouchPos = Input.mousePosition;
#else
				var currentTouch = new Touch();
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    if (Key_Bindings_CS.ZoomButtonFingerID == Input.touches[i].fingerId)
                    {
                        currentTouch = Input.touches[i];
                        break;
                    }
                }
                var currentTouchPos = currentTouch.position;
#endif
                var deltaX = (currentTouchPos.x - previousTouchPos.x) / Screen.width;
                gunCameraScript.zoomInput = deltaX * General_Settings_CS.cameraZoomSensibilityMobile;

                previousTouchPos = currentTouchPos;
                return;
            }

            // Finish zooming.
            if (isZoomButtonDown && Key_Bindings_CS.IsZoomButtonPressing == false)
            { // Zoom button is released.
                isZoomButtonDown = false;
                gunCameraScript.zoomInput = 0.0f;
            }
        }
#endif

    }

}