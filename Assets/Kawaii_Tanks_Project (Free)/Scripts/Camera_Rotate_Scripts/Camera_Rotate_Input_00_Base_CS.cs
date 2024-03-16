using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Rotate_Input_00_Base_CS : MonoBehaviour
    {

        protected Camera_Rotate_CS cameraRotateScript;


        public virtual void Prepare(Camera_Rotate_CS cameraRotateScript)
        {
            this.cameraRotateScript = cameraRotateScript;
        }


        public virtual void Get_Input()
        {
        }

    }

}