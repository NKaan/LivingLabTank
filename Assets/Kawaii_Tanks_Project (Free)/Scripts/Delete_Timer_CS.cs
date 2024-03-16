using UnityEngine;
using System.Collections;

namespace ChobiAssets.KTP
{
    public class Delete_Timer_CS : MonoBehaviour
    {
        /*
         * This script is attached to the visual effect prefabs such as "MuzzleFire" and "Destroyed_Effect".
         * This script destroys this gameobject after the specified time passed.
        */

        [Header("Life time settings")]
        [Tooltip("Life time of this bject. (Sec)")] public float lifeTime = 2.0f;

        void Awake()
        {
            Destroy(this.gameObject, lifeTime);
        }

    }

}
