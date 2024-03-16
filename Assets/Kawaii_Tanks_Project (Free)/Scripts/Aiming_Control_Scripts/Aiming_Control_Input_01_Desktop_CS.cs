using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Aiming_Control_Input_01_Desktop_CS : Aiming_Control_Input_00_Base_CS
    {
#if !UNITY_ANDROID && !UNITY_IPHONE

        GunCamera_Control_CS gunCameraScript;


        public override void Prepare(Aiming_Control_CS aimingScript)
        {
            this.aimingScript = aimingScript;

            // Get "GunCamera_Control_CS".
            gunCameraScript = GetComponentInChildren<GunCamera_Control_CS>();

            // Set the "useAutoTurn".
            aimingScript.useAutoTurn = true;

            // Set the initial aiming mode.
            aimingScript.mode = 1; // Free aiming.
            aimingScript.Switch_Mode();
        }


        public override void Get_Input()
        {
            // Switch the aiming mode.
            if (Key_Bindings_CS.IsSwitchAimingModeKeyDown())
            {
                if (aimingScript.mode == 0 || aimingScript.mode == 2)
                {
                    aimingScript.mode = 1; // Free aiming.
                }
                else
                {
                    aimingScript.mode = 0; // Keep the initial positon.
                }
                aimingScript.Switch_Mode();
            }


            // Adjust aiming.
            if (gunCameraScript && gunCameraScript.gunCamera.enabled)
            { // The gun camera is enabled now.

                // Set the adjust angle.
                aimingScript.adjustAngle += Key_Bindings_CS.GetAimingAxis() * General_Settings_CS.aimingSensibility;

                // Check it is locking-on now.
                if (aimingScript.targetTransform == null)
                { // Now not locking-on.
                    // Try to find a new target.
                    screenCenter.x = Screen.width * 0.5f;
                    screenCenter.y = Screen.height * 0.5f;
                    aimingScript.Reticle_Aiming(screenCenter);
                }
            }
            else
            { // The gun camera is disabled now.
                // Reset the adjust angle.
                aimingScript.adjustAngle = Vector3.zero;

                // Free aiming.
                if (aimingScript.mode == 1)
                { // Free aiming.

                    // Find the target.
                    screenCenter.x = Screen.width * 0.5f;
                    screenCenter.y = Screen.height * 0.5f;
                    aimingScript.Cast_Ray_Free(screenCenter);
                }
            }
            
        }
#endif
    }

}