using UnityEngine;
using System.Collections;


namespace ChobiAssets.KTP
{

    public class Wheel_Sync_CS : MonoBehaviour
    {
        /*
         * This script is attached to all the "Apparent_Wheel" in the tank.
         * This script rotates the wheel synchronizing with the track speed.
         * This script works in cooperation with "Track_Scroll_CS" on the same side.
        */

        [Header("Synchronizing settings")]
        [Tooltip("Offset for the wheel size.")] public float radiusOffset = 0.0f;


        // Set by "Track_Scroll_CS" in the tank.
        [HideInInspector] public Track_Scroll_CS scrollTrackScript;

        Transform thisTransform;
        Vector3 currentAng;
        float radiusRate;


        void Start()
        {
            thisTransform = transform;
            currentAng = thisTransform.localEulerAngles;
        }


        void Update()
        {
            if (scrollTrackScript == null || scrollTrackScript.enabled == false)
            {
                return;
            }

            Rotate();
        }


        void Rotate()
        {
            currentAng.y -= scrollTrackScript.deltaAng * radiusRate;
            thisTransform.localEulerAngles = currentAng;
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }


        public void Get_Reference(bool isLeft, Track_Scroll_CS tempSrollTrackScript, float referenceWheelRadius)
        { // Called from "Track_Scroll_CS".
            if (transform.localPosition.y > 0.0f == isLeft)
            { // On the same side.
                scrollTrackScript = tempSrollTrackScript;
                var thisRadius = GetComponent<MeshFilter>().sharedMesh.bounds.extents.x + radiusOffset;
                radiusRate = referenceWheelRadius / thisRadius;
            }
        }

    }

}
