using UnityEngine;


namespace ChobiAssets.KTP
{

    public class GunCamera_Control_Input_00_Base_CS : MonoBehaviour
    {

        protected GunCamera_Control_CS gunCameraScript;


        public void Prepare(GunCamera_Control_CS gunCameraScript)
        {
            this.gunCameraScript = gunCameraScript;
        }


        public virtual void Get_Input()
        {
        }

    }

}