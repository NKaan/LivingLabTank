using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Rotate_Input_02_Mobile_CS : Camera_Rotate_Input_00_Base_CS
    {
#if UNITY_ANDROID || UNITY_IPHONE

        bool isCameraButtonDown;
        Vector3 previousTouchPos;


        public override void Get_Input()
        {
            // Start camera rotation.
            if (isCameraButtonDown == false && Key_Bindings_CS.IsCameraButtonPressing)
            { // Camera button is pressed.
                isCameraButtonDown = true;
#if UNITY_EDITOR
                previousTouchPos = Input.mousePosition;
#else
				previousTouchPos = Key_Bindings_CS.CameraButtonStartPosition;
#endif
                return;
            }

            // Camera rotation.
            if (isCameraButtonDown && Key_Bindings_CS.IsCameraButtonPressing)
            { // Camera button is being pressed.
#if UNITY_EDITOR
                var currentTouchPos = Input.mousePosition;
#else
                var currentTouch = new Touch();
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    if (Key_Bindings_CS.CameraButtonFingerID == Input.touches[i].fingerId)
                    {
                        currentTouch = Input.touches[i];
                        break;
                    }
                }
                var currentTouchPos = currentTouch.position;
#endif
                var deltaX = (currentTouchPos.x - previousTouchPos.x) / Screen.width;
                cameraRotateScript.rotationInput.y = deltaX * General_Settings_CS.cameraRotationSensibilityMobile;
                // If you need to rotate the camera vertically, uncomment the following lines.
                // var deltaY = (currentTouchPos.y - previousTouchPos.y) / Screen.height;
                // cameraRotateScript.rotationInput.z = -deltaY * General_Settings_CS.cameraRotationSensibilityMobile * 0.5f;
                previousTouchPos = currentTouchPos;
                return;
            }

            // Finish camera rotation.
            if (isCameraButtonDown && Key_Bindings_CS.IsCameraButtonPressing == false)
            { // Camera button is released.
                isCameraButtonDown = false;
                cameraRotateScript.rotationInput = Vector3.zero;
            }
        }
#endif
    }

}