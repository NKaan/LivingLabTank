using UnityEngine;


namespace ChobiAssets.KTP
{

    [DefaultExecutionOrder (-1)] // This script must be executed before all the scripts executed the initializing.
    public class ID_Control_CS : MonoBehaviour
    {
        /* 
		 * This script is attached to the top object of the tank.
		 * This script controls the identification of the tank.
		*/

        [Header("ID settings")]
        [Tooltip("ID number")] public int id = 0;

        [Header("Camera offset settings")]
        [Tooltip("Offset height for the main camera pivot.")] public float cameraOffset = 3.0f;


        // Referred to from the scripts in the tank parts.
        [HideInInspector] public Rigidbody bodyRigidbody;
        [HideInInspector] public Transform bodyTransform;
        [HideInInspector] public Aiming_Control_CS aimingScript;
        [HideInInspector] public Wheel_Control_CS wheelControlScript;
        [HideInInspector] public Fire_Spawn_CS fireSpawnScript;


        bool isSelected;


        void Start()
        {
            // Store components of the tank.
            Store_Components();

            // Send message to the "Game_Controller" in the scene to add this tank to the list.
            Game_Controller_CS.instance.SendMessage("Receive_ID_Script", this, SendMessageOptions.DontRequireReceiver);

            // Set the initial selection condition.
            isSelected = (Game_Controller_CS.instance.currentID == id);
            Broadcast_Selection_Condition();
        }


        void Store_Components()
        {
            bodyRigidbody = GetComponentInChildren<Rigidbody>();
            bodyTransform = bodyRigidbody.transform;
            aimingScript = bodyTransform.GetComponent<Aiming_Control_CS>();
            wheelControlScript = bodyTransform.GetComponent<Wheel_Control_CS>();
            fireSpawnScript = bodyTransform.GetComponentInChildren<Fire_Spawn_CS>();
        }


        public void Receive_Current_ID(int currentID)
        { // Called from "Game_Controller_CS" in the scene, when the player changes the tank.
            isSelected = (id == currentID);
            Broadcast_Selection_Condition();
        }


        void Broadcast_Selection_Condition()
        {
            // Broadcast whether this tank is selected or not to all the children.
            BroadcastMessage("Selected", isSelected, SendMessageOptions.DontRequireReceiver);
        }


        void Selected(bool isSelected)
        {
            if (isSelected == false)
            {
                return;
            }

            // Call the "Camera_Manager_CS" in the scene.
            if (Camera_Manager_CS.instance)
            {
                Camera_Manager_CS.instance.Set_Follow_Target(bodyTransform, cameraOffset);
            }
        }


        void Destroyed_Linkage()
        { // Called from "Damage_Control_CS".

            // Change the tag.
            gameObject.tag = "Finish";
        }


        void OnDestroy()
        { // Called when the tank is removed from the scene.
            
            // Send message to the "Game_Controller" in the scene to remove this tank from the lists.
            if (Game_Controller_CS.instance)
            {
                Game_Controller_CS.instance.SendMessage("Remove_ID", this, SendMessageOptions.DontRequireReceiver);
            }
        }

    }

}
