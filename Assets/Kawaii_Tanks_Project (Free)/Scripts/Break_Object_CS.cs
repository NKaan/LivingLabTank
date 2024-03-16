using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace ChobiAssets.KTP
{

    public class Break_Object_CS : MonoBehaviour
    {
        /*
         * This script is attached to the breakable objects in the scene, such as hedges ans trees.
         * When any object touches this collider, this script instantiates the broken prefab and destroy this gameobject.
        */

        [Header("Broken object settings")]
        [Tooltip("Prefab of the broken object.")] public GameObject brokenPrefab;
        [Tooltip("Lag time for breaking. (Sec)")] public float lagTime = 1.0f;


        Transform thisTransform;
        bool isLiving = true;


        void Start()
        {
            thisTransform = transform;
        }


        void OnTriggerEnter(Collider collider)
        {
            if (isLiving && collider.isTrigger == false)
            {
                StartCoroutine("Broken");
            }
        }


        IEnumerator Broken()
        {
            isLiving = false;

            yield return new WaitForSeconds(lagTime);

            if (brokenPrefab)
            {
                Instantiate(brokenPrefab, thisTransform.position, thisTransform.rotation);
            }

            Destroy(gameObject);
        }

    }

}
