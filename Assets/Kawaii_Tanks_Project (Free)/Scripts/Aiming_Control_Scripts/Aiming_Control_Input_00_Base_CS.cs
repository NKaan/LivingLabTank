using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Aiming_Control_Input_00_Base_CS : MonoBehaviour
    {
        protected Aiming_Control_CS aimingScript;
        protected Vector3 screenCenter = Vector3.zero;


        public virtual void Prepare(Aiming_Control_CS aimingScript)
        {
        }


        public virtual void Get_Input()
        {
        }

    }

}