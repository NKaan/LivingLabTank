using UnityEngine;
using System.Collections;
using UnityEngine.UI;


namespace ChobiAssets.KTP
{

    public class Damage_Control_CS : MonoBehaviour
    {
        /*
         * This script is attached to the top object of the tank.
         * This script controls the damage system of the tank.
         * The durability (Hit points) and the damaged visual effects are controlled by this script.
         * 
        */ 
        
        [Header("Damage settings")]
        [Tooltip("Durability (Hit points) of the tank.")] public float durability = 300.0f;
        [Tooltip("Prefab for destroyed effects.")] public GameObject destroyedPrefab;
        [Tooltip("Prefab for displaying the durability.")] public GameObject textPrefab;
        [Tooltip("Name of the canvas for 'Text Prefab'.")] public string canvasName = "Canvas_Texts";


        [HideInInspector] public Transform bodyTransform; // Referred to from "Damage_Display_CS".
        [HideInInspector] public bool isSelected; // Referred to from "Damage_Display_CS".

        float killHight = -64.0f;
        Damage_Display_CS displayScript;
        float initialDurability;
        float currentDurability;
        GameObject dyingObject;
        bool isDead;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            // Get the references via "ID_Control_CS".
            var idScript = GetComponentInParent<ID_Control_CS>();
            bodyTransform = idScript.bodyTransform;

            // Store the initial durability.
            initialDurability = durability;

            // Set the current durability.
            currentDurability = initialDurability;

            // Setup the damage text.
            Set_Damage_Text();
        }


        void Set_Damage_Text()
        {
            if (textPrefab == null || string.IsNullOrEmpty(canvasName) || initialDurability == Mathf.Infinity)
            {
                return;
            }

            // Find the canvas.
            var canvasObject = GameObject.Find(canvasName);
            if (canvasObject == null)
            {
                Debug.LogWarning(canvasName + " cannot be found in the scene.");
                return;
            }

            // Instantiate the text prefab.
            var textObject = Instantiate(textPrefab, Vector3.zero, Quaternion.identity, canvasObject.transform) as GameObject;

            // Setup the "Damage_Display_CS" script in the text object.
            displayScript = textObject.GetComponent<Damage_Display_CS>();
            displayScript.damageControlScript = this;
        }


        void Update()
        {
            // Check the hight and the rotation.
            Check_Height_And_Rotation();
        }


        void Check_Height_And_Rotation()
        {
            if (bodyTransform.position.y < killHight)
            { // The tank is under the kill hight.
                Start_Destroying();
                return;
            }

            if (Mathf.Abs(Mathf.DeltaAngle(0.0f, bodyTransform.localEulerAngles.z)) > 90.0f)
            { // The tank has rolled over.
                Start_Destroying();
                return;
            }
        }


        public void Get_Damage(float damageValue)
        { // Called from "Bullet_Nav_CS".
            if (isDead)
            { // The tank has already destroyed.
                return;
            }

            // Reduce the current durability.
            currentDurability -= damageValue;
            currentDurability = Mathf.Clamp(currentDurability, 0.0f, initialDurability);

            // Check the tank is alive or not.
            if (currentDurability > 0.0f)
            { // Alive.
                // Display the damage text
                if (displayScript)
                {
                    displayScript.Get_Damage(currentDurability, initialDurability);
                }
            }
            else
            { // Dead
                Start_Destroying();
            }
        }


        public void Get_Recovery(float recoveryValue)
        {
            // Increase the current durability.
            currentDurability += recoveryValue;
            currentDurability = Mathf.Clamp(currentDurability, 0.0f, initialDurability);

            // Display the damage text
            if (displayScript)
            {
                displayScript.Get_Damage(currentDurability, initialDurability);
            }
        }


        void Start_Destroying()
        {
            // Set the dead flag.
            isDead = true;

            // Send message to all the parts.
            transform.root.BroadcastMessage("Destroyed_Linkage", SendMessageOptions.DontRequireReceiver);
        }


        void Destroyed_Linkage()
        {
            // Spawn the destroyed effects.
            if (destroyedPrefab)
            {
                Instantiate(destroyedPrefab, bodyTransform.position, Quaternion.identity, bodyTransform);
            }

            // Remove the damage text.
            if (displayScript)
            {
                Destroy(displayScript.gameObject);
            }

            // Remove the dying effect.
            if (dyingObject)
            {
                Destroy(dyingObject);
            }

            // Remove this script.
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