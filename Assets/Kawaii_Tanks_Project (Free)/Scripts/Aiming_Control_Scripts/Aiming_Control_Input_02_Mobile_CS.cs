using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Aiming_Control_Input_02_Mobile_CS : Aiming_Control_Input_00_Base_CS
    {
#if UNITY_ANDROID || UNITY_IPHONE

        bool isLockButtonDown;
        bool isAutoLockButtonDown;
        bool isAimButtonDown;
        Vector3 aimingPreviousTouchPos;


        public override void Prepare(Aiming_Control_CS aimingScript)
        {
            this.aimingScript = aimingScript;

            // Set the "useAutoTurn".
            aimingScript.useAutoTurn = true;
        }


        public override void Get_Input()
        {
            Lock_On();
            Aiming();
        }

        
        void Lock_On()
        {
            // Check the lock button is down.
            if (isLockButtonDown == false && Key_Bindings_CS.IsLockButtonPressing)
            {
                isLockButtonDown = true;
                return;
            }

            // Check the lock button is up.
            if (isLockButtonDown && Key_Bindings_CS.IsLockButtonPressing == false)
            {
#if UNITY_EDITOR
                aimingScript.Touch_Lock(Input.mousePosition);
#else
                aimingScript.Touch_Lock(Key_Bindings_CS.LockButtonUpPosition);
#endif

                isLockButtonDown = false;
            }
        }


        void Aiming()
        {
            // Start aiming process.
            if (isAimButtonDown == false && Key_Bindings_CS.IsAimButtonPressing)
            { // Aiming button is pressed.
                isAimButtonDown = true;

#if UNITY_EDITOR
                aimingPreviousTouchPos = Input.mousePosition;
#else
				aimingPreviousTouchPos = Key_Bindings_CS.AimButtonStartPosition;
#endif
                return;
            }

            // Control aiming.
            if (isAimButtonDown && Key_Bindings_CS.IsAimButtonPressing)
            { // Aiming button is being pressed.
                // Control adjusting angle.
#if UNITY_EDITOR
                var currentTouchPos = Input.mousePosition;
#else
                var currentTouch = new Touch();
                for (int i = 0; i < Input.touches.Length; i++)
                {
                    if (Key_Bindings_CS.AimButtonFingerID == Input.touches[i].fingerId)
                    {
                        currentTouch = Input.touches[i];
                        break;
                    }
                }
                var currentTouchPos = currentTouch.position;
#endif
                Vector3 delta;
                delta.x = (currentTouchPos.x - aimingPreviousTouchPos.x) / Screen.width;
                delta.y = (currentTouchPos.y - aimingPreviousTouchPos.y) / Screen.height;
                delta.z = 0.0f;
                aimingScript.adjustAngle += delta * General_Settings_CS.aimingSensibilityMobile;
                aimingPreviousTouchPos = currentTouchPos;
                return;
            }

            // Finish aiming.
            if (isAimButtonDown && Key_Bindings_CS.IsAimButtonPressing == false)
            { // Aiming button is released.
                isAimButtonDown = false;
                aimingScript.adjustAngle = Vector3.zero;
                return;
            }
        }
#endif
    }

}