using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Zoom_Input_01_Desktop_CS : Camera_Zoom_Input_00_Base_CS
    {
#if !UNITY_ANDROID && !UNITY_IPHONE

        public override void Get_Input()
        {
            cameraZoomScript.zoomInput = -Key_Bindings_CS.GetCameraZoomingAxis() * General_Settings_CS.cameraZoomSensibility;
        }

#endif
    }

}