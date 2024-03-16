using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Rotate_Input_88_Demo_CS : Camera_Rotate_Input_00_Base_CS
    {

        public override void Get_Input()
        {
            // Set the target angles.
            cameraRotateScript.rotationInput.y = 0.1f;
            cameraRotateScript.rotationInput.z = Mathf.Sin(Time.time * 0.5f) * 0.1f;
        }

    }

}