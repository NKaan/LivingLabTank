using UnityEngine;


namespace ChobiAssets.KTP
{

    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(Rigidbody))]

    public class MainBody_Settings_CS : MonoBehaviour
    {
        /*
         * This script is attached to MainBody of the tank.
         * These variables are used for setting the physics of the MainBody.
         * 
        */

        // User options >>
        public int solverIterationCount = 7;
        // << User options


        void Start()
        {
            // Set the Solver Iterations Count of the rigidbody.
            var thisRigidbody = GetComponent<Rigidbody>();
            thisRigidbody.solverIterations = solverIterationCount; // (Note.) The "solverIterations" must be set in the runtime.

            // Set the Layer.
            gameObject.layer = Layer_Settings_CS.Body_Layer;

            Destroy(this);
        }
        
    }

}