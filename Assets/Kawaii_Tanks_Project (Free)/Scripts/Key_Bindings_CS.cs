using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Key_Bindings_CS
    {
        /*
         * You can change the key bindings form this script.
        */


#if !UNITY_ANDROID && !UNITY_IPHONE
        // Move axes
        public static string moveVerticalAxisName = "Vertical";
        public static string moveHorizontalAxisName = "Horizontal";
        public static KeyCode moveStopKeyCode = KeyCode.X;

        // Switch aiming
        public static KeyCode switchAimingModeKeyCode = KeyCode.Mouse2;

        // Aiming key
        public static KeyCode aimingKeyCode = KeyCode.Mouse1;

        // Aiming axes
        public static string aimingHorizontalAxisName = "Mouse X";
        public static string aimingVerticalAxisName = "Mouse Y";

        // Fire key
        public static KeyCode fireKeyCode = KeyCode.Mouse0;

        // Camera rotation axes
        public static string cameraHorizontalAxisName = "Mouse X";
        public static string cameraVerticalAxisName = "Mouse Y";

        // Camera zooming axes
        public static string cameraZoomingAxisName = "Mouse ScrollWheel";

        // Switch key
        public static KeyCode switchKeyCode = KeyCode.Tab;

        // Quit key
        public static KeyCode quitKeyCode = KeyCode.Escape;

        // Restart keys
        public static KeyCode restartKeyCode = KeyCode.Backspace;

        // Pause key
        public static KeyCode pauseKeyCode = KeyCode.P;

        // Switch cursor mode key
        public static KeyCode switchCursorModeKeyCode = KeyCode.LeftShift;

#else
		// Quit key
		public static KeyCode quitKeyCode = KeyCode.Escape; // Return button on Android.
#endif


#if !UNITY_ANDROID && !UNITY_IPHONE
        // Move axis
        public static Vector2 GetMoveAxis()
        {
            Vector2 axis;
            axis.x = Input.GetAxisRaw(moveHorizontalAxisName);
            axis.y = Input.GetAxisRaw(moveVerticalAxisName);
            axis.y = Mathf.Clamp(axis.y, -0.5f, 1.0f);
            return axis;
        }
        
        // Switch aiming mode (Down)
        public static bool IsSwitchAimingModeKeyDown()
        {
            return Input.GetKeyDown(switchAimingModeKeyCode);
        }

        // Aiming (Pressing, Down, Up)
        public static bool IsAimingKeyPressing()
        {
            return (Input.GetKey(aimingKeyCode));
        }
        public static bool IsAimingKeyDown()
        {
            return (Input.GetKeyDown(aimingKeyCode));
        }
        public static bool IsAimingKeyUp()
        {
            return (Input.GetKeyUp(aimingKeyCode));
        }

        // Aiming axis
        public static Vector3 GetAimingAxis()
        {
            Vector3 axis;
            axis.x = Input.GetAxis(aimingHorizontalAxisName);
            axis.y = Input.GetAxis(aimingVerticalAxisName);
            axis.z = 0.0f;
            return axis;
        }

        // Fire (Pressing)
        public static bool IsFireKeyPressing()
        {
            return (Input.GetKey(fireKeyCode));
        }

        // Camera rotation axis
        public static Vector3 GetCameraRotationAxis()
        {
            Vector3 axis;
            axis.x = 0.0f;
            axis.y = Input.GetAxis(cameraHorizontalAxisName);
            axis.z = -Input.GetAxis(cameraVerticalAxisName) * 0.5f;
            return axis;
        }

        // Camera zooming axis
        public static float GetCameraZoomingAxis()
        {
            return Input.GetAxis(cameraZoomingAxisName);
        }

        // Switch key (Down)
        public static bool IsSwitchKeyDown()
        {
            return Input.GetKeyDown(switchKeyCode);
        }

        // Quit key (Down)
        public static bool IsQuitKeyDown()
        {
            return Input.GetKeyDown(quitKeyCode);
        }

        // Restart keys (Down)
        public static bool IsRestartKeyDown()
        {
            return Input.GetKeyDown(restartKeyCode);
        }

        // Pause key (Down)
        public static bool IsPauseKeyDown()
        {
            return Input.GetKeyDown(pauseKeyCode);
        }

        // Switch cursor mode key (Down)
        public static bool IsSwitchCursorModeKeyDown()
        {
            return Input.GetKeyDown(switchCursorModeKeyCode);
        }


#else
        // Move buttons.
        public static bool IsForwardButtonPressing;
        public static bool IsBackwardButtonPressing;
        public static bool IsLeftButtonPressing;
        public static bool IsRightButtonPressing;
        public static bool IsStopButtonPressing;

        // Lock button.
        public static bool IsLockButtonPressing;
        public static Vector2 LockButtonUpPosition;

        // Aiming button.
        public static bool IsAimButtonPressing;
        public static Vector2 AimButtonStartPosition;
        public static int AimButtonFingerID;

        // Fire button.
        public static bool IsFireButtonPressing;

        // Camera button.
        public static bool IsCameraButtonPressing;
        public static Vector2 CameraButtonStartPosition;
        public static int CameraButtonFingerID;

        // Zoom button.
        public static bool IsZoomButtonPressing;
        public static Vector2 ZoomButtonStartPosition;
        public static int ZoomButtonFingerID;

        // Switch button.
        public static bool IsSwitchButtonPressing;

        // Quit key.
        public static bool IsQuitKeyDown()
        {
            return Input.GetKeyDown(quitKeyCode);
        }

        // Pause button.
        public static bool IsPauseButtonPressing;
#endif

    }
}
