using UnityEngine;
using System.Collections;

// This script must be attached to Tracks.
namespace ChobiAssets.KTP
{

    public class Track_Scroll_CS : MonoBehaviour
    {
        /*
         * This script is attached to each track in the tank.
         * This script controls the scrolling animation of the track.
        */

        public bool isLeft = true;
        public Transform referenceWheel;
        public string referenceName;
        public string referenceParentName;
        public float scrollRate = 0.0005f;
        public bool scrollYAxis;

        // For editor script.
        public bool hasChanged;


        // Referred to from "Wheel_Sync_CS".
        [HideInInspector] public float deltaAng;

        Material thisMaterial;
        float previousAng;
        Vector2 offset;


        void Start()
        {
            Initialize();
        }


        void Initialize()
        {
            thisMaterial = GetComponent<Renderer>().material;

            // Find the reference wheels.
            if (referenceWheel == null)
            {
                if (string.IsNullOrEmpty(referenceName) == false && string.IsNullOrEmpty(referenceParentName) == false)
                {
                    referenceWheel = transform.parent.Find(referenceParentName + "/" + referenceName);
                }
            }
            if (referenceWheel == null)
            {
                Debug.LogError("'Reference wheel' for the Track cannot be found.");
                Destroy(this);
                return;
            }

            // Send the reference values to all the "Wheel_Sync_CS" in the tank.
            Send_Reference_Values();
        }


        void Send_Reference_Values()
        {
            // Get the direction.
            var isLeft = (referenceWheel.localPosition.y > 0.0f);
            
            // Get the radius of the reference wheel.
            var referenceWheelRadius = referenceWheel.GetComponent<MeshFilter>().sharedMesh.bounds.extents.x;

            // Send the reference values to all the "Wheel_Sync_CS" in the tank.
            var wheelSyncScripts = transform.parent.GetComponentsInChildren<Wheel_Sync_CS>();
            for (int i = 0; i < wheelSyncScripts.Length; i++)
            {
                wheelSyncScripts[i].Get_Reference(isLeft, this, referenceWheelRadius);
            }
        }


        void Update()
        {
            Calculate_Delta_Angle();
        }


        void Calculate_Delta_Angle()
        {
            var currentAng = referenceWheel.localEulerAngles.y;
            deltaAng = Mathf.DeltaAngle(currentAng, previousAng);
            if (scrollYAxis)
            {
                offset.y += scrollRate * deltaAng;
            }
            else
            {
                offset.x += scrollRate * deltaAng;
            }
            thisMaterial.SetTextureOffset(General_Settings_CS.mainTextureName, offset);
            previousAng = currentAng;
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }


        void OnDestroy()
        { // Avoid memory leak.
            if (thisMaterial)
            {
                Destroy(thisMaterial);
            }
        }
    }

}
