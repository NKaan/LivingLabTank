using UnityEngine;
using System.Collections;

// This script must be attached to "Barrel_Base".
namespace ChobiAssets.KTP
{
	
	public class Barrel_Control_CS : MonoBehaviour
	{
        /*
		 * This script is attached to the "Barrel_base" in the tank.
		 * This script controls the barrel motion in the tank.
		 * When firing, this script moves the barrel backward and forward like a recoil brake.
		*/

        [Header ("Recoil Brake settings")]
		[ Tooltip ("Time it takes to push back the barrel. (Sec)")] public float recoilTime = 0.2f;
		[ Tooltip ("Time it takes to to return the barrel. (Sec)")] public float returnTime = 1.0f;
		[ Tooltip ("Movable length for the recoil brake. (Meter)")] public float length = 0.3f;


		Transform thisTransform;
		bool isReady = true;
		Vector3 initialPos;
		Vector3 currentPos;
		const float halfPI = Mathf.PI * 0.5f;


        void Start()
        {
            Initialize();
        }


        void Initialize()
		{
			thisTransform = transform;
			initialPos = thisTransform.localPosition;
			currentPos = initialPos;
		}


        public void Fire_Linkage()
        { // Called from "Fire_Control_CS".
            if (isReady == false)
            {
                return;
            }

            isReady = false;
            StartCoroutine("Recoil_Brake");
        }


        IEnumerator Recoil_Brake()
        {
            // Move backward.
            float count = 0.0f;
            while (count < recoilTime)
            {
                float rate = Mathf.Sin(halfPI * (count / recoilTime));
                currentPos.z = initialPos.z - (rate * length);
                thisTransform.localPosition = currentPos;
                count += Time.deltaTime;
                yield return null;
            }

            // Return to the initial position.
            count = 0.0f;
            while (count < returnTime)
            {
                float rate = Mathf.Sin(halfPI * (count / returnTime) + halfPI);
                currentPos.z = initialPos.z - (rate * length);
                thisTransform.localPosition = currentPos;
                count += Time.deltaTime;
                yield return null;
            }

            // Finish.
            thisTransform.localPosition = initialPos;
            isReady = true;
        }

	}

}
