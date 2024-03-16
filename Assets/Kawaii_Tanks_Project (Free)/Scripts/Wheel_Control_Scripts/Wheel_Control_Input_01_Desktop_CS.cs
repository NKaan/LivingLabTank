using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Wheel_Control_Input_01_Desktop_CS : Wheel_Control_Input_00_Base_CS
    {
#if !UNITY_ANDROID && !UNITY_IPHONE

        Vector2 currentAxis;
        Vector2 targetAxis;
        float turnVelocity;

        public override void Get_Input()
        {
            // Get the input.
            if (wheelControlScript.isSelected)
            {
                targetAxis = Key_Bindings_CS.GetMoveAxis();
            }
            else
            {
                targetAxis = Vector2.zero;
            }

            // Turn.
            currentAxis.x = Mathf.SmoothDamp(currentAxis.x, targetAxis.x, ref turnVelocity, 0.2f * Time.deltaTime);

            // Forward and Backward.
            var changeRate = 2.0f;
            if (targetAxis.y == 0.0f)
            { // No input for forward and backward.

                if (Input.GetKey(Key_Bindings_CS.moveStopKeyCode))
                {
                    // Stop quickly.
                    targetAxis.y = 0.0f;
                    changeRate = 4.0f;
                }
                else
                {
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
                        if (currentAxis.y > 0.0f)
                        { // Going forward now.
                            // Slow down gently.
                            changeRate = 0.2f;
                        }
                    }
                }
            }
            currentAxis.y = Mathf.MoveTowards(currentAxis.y, targetAxis.y, changeRate * Time.deltaTime);

            // Set the aixs.
            wheelControlScript.moveAxis = currentAxis;
        }
#endif
    }

}