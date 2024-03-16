using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Zoom_Input_02_Mobile_CS : Camera_Zoom_Input_00_Base_CS
    {
#if UNITY_ANDROID || UNITY_IPHONE

        bool isZoomButtonDown = false;
        Vector2 previousTouchPos;


        public override void Get_Input()
        {
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
                cameraZoomScript.zoomInput = deltaX * General_Settings_CS.cameraZoomSensibilityMobile;
                previousTouchPos = currentTouchPos;
                return;
            }

            // Finish zooming.
            if (isZoomButtonDown && Key_Bindings_CS.IsZoomButtonPressing == false)
            { // Zoom button is released.
                isZoomButtonDown = false;
                cameraZoomScript.zoomInput = 0.0f;
            }
        }
#endif
    }

}