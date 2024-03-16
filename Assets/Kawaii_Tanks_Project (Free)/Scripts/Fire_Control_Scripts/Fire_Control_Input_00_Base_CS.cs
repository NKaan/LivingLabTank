using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Fire_Control_Input_00_Base_CS : MonoBehaviour
    {

        protected Fire_Control_CS fireControlScript;


        public virtual void Prepare(Fire_Control_CS fireControlScript)
        {
            this.fireControlScript = fireControlScript;
        }


        public virtual void Get_Input()
        {
        }

    }

}