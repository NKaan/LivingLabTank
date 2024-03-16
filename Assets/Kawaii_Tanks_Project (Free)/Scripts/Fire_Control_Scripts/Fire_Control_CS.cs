using UnityEngine;
using System.Collections;
using UnityEngine.UI;


// This script must be attached to "Cannon_Base".
namespace ChobiAssets.KTP
{

    public class Fire_Control_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the "Cannon_Base" in the tank.
		 * This script controls the firining of the tank.
		 * When firing, this script calls "Fire_Spawn_CS" and "Barrel_Control_CS" scripts placed under this object in the hierarchy.
		*/

        [Header("Fire control settings")]
        [Tooltip("Loading time. (Sec)")] public float reloadTime = 4.0f; // Referred to from "Reloading_Circle_CS".
        [Tooltip("Recoil force with firing.")] public float recoilForce = 5000.0f;


        // Referred to from "Reloading_Circle_CS".
        [HideInInspector] public bool isLoaded = true;
        [HideInInspector] public float loadingCount;

        Rigidbody bodyRigidbody;
        Transform thisTransform;
        Barrel_Control_CS barrelScript;
        Fire_Spawn_CS fireSpawnScript;


        Fire_Control_Input_00_Base_CS inputScript;
        bool isSelected;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisTransform = transform;
            barrelScript = thisTransform.parent.GetComponentInChildren<Barrel_Control_CS>();

            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            bodyRigidbody = idScript.bodyRigidbody;
            fireSpawnScript = idScript.fireSpawnScript;

            // Set the input script.
            Set_Input_Script();

            // Prepare the "inputScript".
            inputScript.Prepare(this);
        }


        protected virtual void Set_Input_Script()
        {
#if !UNITY_ANDROID && !UNITY_IPHONE
            inputScript = gameObject.AddComponent<Fire_Control_Input_01_Desktop_CS>();
#else
            inputScript = gameObject.AddComponent<Fire_Control_Input_02_Mobile_CS>();
#endif
        }


        void Update()
        {
            if (isLoaded == false)
            {
                return;
            }

            if (isSelected)
            {
                inputScript.Get_Input();
            }
        }


        public void Fire()
        {
            // Call the "Fire_Spawn_CS".
            fireSpawnScript.Fire_Linkage();

            // Call the "Barrel_Control_CS".
            barrelScript.Fire_Linkage();

            // Add recoil shock force to the MainBody.
            bodyRigidbody.AddForceAtPosition(-thisTransform.forward * recoilForce, thisTransform.position, ForceMode.Impulse);

            // Reload.
            StartCoroutine("Reload");
        }


        IEnumerator Reload()
        {
            isLoaded = false;
            loadingCount = 0.0f;

            while (loadingCount < reloadTime)
            {
                loadingCount += Time.deltaTime;
                yield return null;
            }

            isLoaded = true;
            loadingCount = reloadTime;
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".
            Destroy(this);
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }


        void Selected(bool isSelected)
        { // Called from "ID_Control_CS".

            this.isSelected = isSelected;
        }
    }

}
