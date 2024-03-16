using UnityEngine;


namespace ChobiAssets.KTP
{

	public class Extra_Collider_CS : MonoBehaviour
	{
        /*
		 * This script is attached to "Extra_Collier" in the tank.
		 * This script only sets the Layer of this gameobject.
		*/


        void Start()
        {
            gameObject.layer = Layer_Settings_CS.Extra_Collider_Layer;

            Destroy(this);
        }

    }

}