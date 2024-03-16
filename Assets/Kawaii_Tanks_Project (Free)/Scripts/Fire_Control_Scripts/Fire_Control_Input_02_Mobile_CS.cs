using UnityEngine;

namespace ChobiAssets.KTP
{

    public class Fire_Control_Input_02_Mobile_CS : Fire_Control_Input_00_Base_CS
    {
#if UNITY_ANDROID || UNITY_IPHONE

        bool isAimButtonDown;


        public override void Get_Input()
        {
            // There are two ways to fire for touch controls, releasing the Aim botton or pressing the Fire button.
            
            // Start aiming process.
            if (isAimButtonDown == false && Key_Bindings_CS.IsAimButtonPressing)
            { // Aiming button is pressed.
                isAimButtonDown = true;
                return;
            }

            // Finish aiming.
            if (isAimButtonDown && Key_Bindings_CS.IsAimButtonPressing == false)
            { // Aiming button is released.
                isAimButtonDown = false;
                fireControlScript.Fire();
                return;
            }

            // Rapid Fire.
            if (Key_Bindings_CS.IsFireButtonPressing)
            {
                fireControlScript.Fire();
            }

        }
#endif
    }

}