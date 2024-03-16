using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Zoom_Input_00_Base_CS : MonoBehaviour
    {

        protected Camera_Zoom_CS cameraZoomScript;


        public void Prepare(Camera_Zoom_CS cameraZoomScript)
        {
            this.cameraZoomScript = cameraZoomScript;
        }


        public virtual void Get_Input()
        {
        }

    }

}