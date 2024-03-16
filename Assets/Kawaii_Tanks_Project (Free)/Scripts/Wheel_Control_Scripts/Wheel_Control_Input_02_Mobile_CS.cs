using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Wheel_Control_Input_02_Mobile_CS : Wheel_Control_Input_00_Base_CS
    {
#if UNITY_ANDROID || UNITY_IPHONE
     
        Vector2 currentAxis;
        Vector2 targetAxis;
        float turnVelocity;

        public override void Get_Input()
        {
            // Turn.
            if (wheelControlScript.isSelected)
            {
                if (Key_Bindings_CS.IsLeftButtonPressing || Input.GetKey(KeyCode.A))
                {
                    targetAxis.x = -1.0f;
                }
                else if (Key_Bindings_CS.IsRightButtonPressing || Input.GetKey(KeyCode.D))
                {
                    targetAxis.x = 1.0f;
                }
                else
                {
                    targetAxis.x = 0.0f;
                }
            }
            else
            {
                targetAxis.x = 0.0f;
            }
            currentAxis.x = Mathf.SmoothDamp(currentAxis.x, targetAxis.x, ref turnVelocity, 0.2f * Time.deltaTime);


            // Forward and Backward.
            var changeRate = 2.0f;
            if (wheelControlScript.isSelected)
            {
                if (Key_Bindings_CS.IsForwardButtonPressing || Input.GetKey(KeyCode.W))
                {
                    targetAxis.y = 1.0f;
                }
                else if (Key_Bindings_CS.IsBackwardButtonPressing || Input.GetKey(KeyCode.S))
                {
                    targetAxis.y = -0.5f;
                }
                else if (Key_Bindings_CS.IsStopButtonPressing || Input.GetKey(KeyCode.X))
                {
                    // Stop quickly.
                    targetAxis.y = 0.0f;
                    changeRate = 4.0f;
                }
                else
                { // Forward, Backward, Stop buttons are not pressed.
                    if (currentAxis.x != 0.0f)
                    { // Turning now.
                        if (currentAxis.y != 0.0f)
                        { // Not pivot-turning now.
                          // Keep the minimum speed to turn smoothly.
                            var tempAxisY = Mathf.Abs(currentAxis.y);
                            tempAxisY = Mathf.Clamp(tempAxisY, 0.25f, 1.0f);
                            targetAxis.y = tempAxisY * Mathf.Sign(currentAxis.y);
                        }
                    }
                    else
                    { // Not turning now.
                      // Slow dowm gently.
                        changeRate = 0.2f;
                        targetAxis.y = 0.0f;
                    }
                }
            }
            else
            {
                targetAxis.y = 0.0f;
            }
            currentAxis.y = Mathf.MoveTowards(currentAxis.y, targetAxis.y, changeRate * Time.deltaTime);

            // Set the aixs.
            wheelControlScript.moveAxis = currentAxis;
        }  

#endif
    }

}