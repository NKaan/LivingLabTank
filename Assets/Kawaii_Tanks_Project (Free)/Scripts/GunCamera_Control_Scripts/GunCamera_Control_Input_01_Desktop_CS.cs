using UnityEngine;


namespace ChobiAssets.KTP
{

    public class GunCamera_Control_Input_01_Desktop_CS : GunCamera_Control_Input_00_Base_CS
    {
#if !UNITY_ANDROID && !UNITY_IPHONE


        public override void Get_Input()
        {
            // Switch on.
            if (gunCameraScript.gunCamera.enabled == false && Key_Bindings_CS.IsAimingKeyDown())
            {
                gunCameraScript.Switch_Mode(1); // On
                return;
            }

            // Zooming.
            if (gunCameraScript.gunCamera.enabled && Key_Bindings_CS.IsAimingKeyPressing())
            {
                gunCameraScript.zoomInput = -Key_Bindings_CS.GetCameraZoomingAxis() * General_Settings_CS.gunCameraZoomSensibility;
                return;
            }

            // Switch off.
            if (gunCameraScript.gunCamera.enabled && Key_Bindings_CS.IsAimingKeyUp())
            {
                gunCameraScript.Switch_Mode(0); // Off
                return;
            }
        }
#endif
    }

}