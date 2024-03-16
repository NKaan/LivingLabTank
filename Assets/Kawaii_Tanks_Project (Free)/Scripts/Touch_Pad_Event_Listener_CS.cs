using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.EventSystems;


namespace ChobiAssets.KTP
{

    public class Touch_Pad_Event_Listener_CS : MonoBehaviour
    {

#if UNITY_ANDROID || UNITY_IPHONE

        // Move buttons.
        public void ForwardButtonDown()
        {
            Key_Bindings_CS.IsForwardButtonPressing = true;
        }
        public void ForwardButtonUp()
        {
            Key_Bindings_CS.IsForwardButtonPressing = false;
        }

        public void BackwardButtonDown()
        {
            Key_Bindings_CS.IsBackwardButtonPressing = true;
        }
        public void BackwardButtonUp()
        {
            Key_Bindings_CS.IsBackwardButtonPressing = false;
        }

        public void LeftButtonDown()
        {
            Key_Bindings_CS.IsLeftButtonPressing = true;
        }
        public void LeftButtonUp()
        {
            Key_Bindings_CS.IsLeftButtonPressing = false;
        }

        public void RightButtonDown()
        {
            Key_Bindings_CS.IsRightButtonPressing = true;
        }
        public void RightButtonUp()
        {
            Key_Bindings_CS.IsRightButtonPressing = false;
        }

        public void StopButtonDown()
        {
            Key_Bindings_CS.IsStopButtonPressing = true;
        }
        public void StopButtonUp()
        {
            Key_Bindings_CS.IsStopButtonPressing = false;
        }


        // Lock button.
        public void LockButtonDown()
        {
            Key_Bindings_CS.IsLockButtonPressing = true;
        }
        public void LockButtonUp(BaseEventData eventData)
        {
            Key_Bindings_CS.IsLockButtonPressing = false;
            var pointerEventData = eventData as PointerEventData;
            Key_Bindings_CS.LockButtonUpPosition = pointerEventData.position;
        }


        // Aim button.
        public void AimButtonDown(BaseEventData eventData)
        {
            Key_Bindings_CS.IsAimButtonPressing = true;
            var pointerEventData = eventData as PointerEventData;
            Key_Bindings_CS.AimButtonStartPosition = pointerEventData.position;
            Key_Bindings_CS.AimButtonFingerID = pointerEventData.pointerId;
        }
        public void AimButtonUp()
        {
            Key_Bindings_CS.IsAimButtonPressing = false;
        }


        // Aim button.
        public void FireButtonDown()
        {
            Key_Bindings_CS.IsFireButtonPressing = true;
        }
        public void FireButtonUp()
        {
            Key_Bindings_CS.IsFireButtonPressing = false;
        }


        // Camera button.
        public void CameraButtonDown(BaseEventData eventData)
        {
            Key_Bindings_CS.IsCameraButtonPressing = true;
            var pointerEventData = eventData as PointerEventData;
            Key_Bindings_CS.CameraButtonStartPosition = pointerEventData.position;
            Key_Bindings_CS.CameraButtonFingerID = pointerEventData.pointerId;
        }
        public void CameraButtonUp()
        {
            Key_Bindings_CS.IsCameraButtonPressing = false;
        }


        // Zoom button.
        public void ZoomButtonDown(BaseEventData eventData)
        {
            Key_Bindings_CS.IsZoomButtonPressing = true;
            var pointerEventData = eventData as PointerEventData;
            Key_Bindings_CS.ZoomButtonStartPosition = pointerEventData.position;
            Key_Bindings_CS.ZoomButtonFingerID = pointerEventData.pointerId;
        }
        public void ZoomButtonUp()
        {
            Key_Bindings_CS.IsZoomButtonPressing = false;
        }


        // Pause button.
        public void PauseButtonDown()
        {
            Key_Bindings_CS.IsPauseButtonPressing = true;
        }
        public void PauseButtonUp()
        {
            Key_Bindings_CS.IsPauseButtonPressing = false;
        }


        // Switch button.
        public void SwitchButtonDown()
        {
            Key_Bindings_CS.IsSwitchButtonPressing = true;
        }
        public void SwitchButtonUp()
        {
            Key_Bindings_CS.IsSwitchButtonPressing = false;
        }

#endif

    }

}
