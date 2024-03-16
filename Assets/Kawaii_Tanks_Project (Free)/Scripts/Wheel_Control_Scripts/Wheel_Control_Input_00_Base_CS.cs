using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Wheel_Control_Input_00_Base_CS : MonoBehaviour
    {

        protected Wheel_Control_CS wheelControlScript;


        public virtual void Prepare(Wheel_Control_CS wheelControlScript)
        {
            this.wheelControlScript = wheelControlScript;
        }


        public virtual void Get_Input()
        {
        }

    }

}