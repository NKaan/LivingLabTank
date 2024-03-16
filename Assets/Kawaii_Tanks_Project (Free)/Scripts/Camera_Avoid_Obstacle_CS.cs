using UnityEngine;


namespace ChobiAssets.KTP
{

    public class Camera_Avoid_Obstacle_CS : MonoBehaviour
    {
        /*
		 * This script is attached to the main camera in "Camera_Manager" object in the scene..
		 * This script moves the camera forward and backward to avoid an obstacle between the camera and the tank.
		*/

        [Header("Main camera settings")]
        [Tooltip("Set the main camera.")] public Camera mainCamera;

        Transform thisTransform;
        Transform parentTransform;
        Vector3 currentLocalPos;
        float currentDistance;
        float targetDistance;
        bool isAvoidingObstacle;
        float hittingTime;
        float storedDistance;
        Transform targetRoot;

        [HideInInspector] public static Camera_Avoid_Obstacle_CS instance;


        void Awake()
        {
            instance = this;
        }


        void Start()
        {
            Initialize();
        }


        public void Get_Target(Transform targetTranform)
        { // Called from "Camera_Manager_CS".
            targetRoot = targetTranform.root;
        }


        void Initialize()
        {
            if (mainCamera == null)
            {
                mainCamera = GetComponent<Camera>();
            }
            thisTransform = transform;
            parentTransform = thisTransform.parent;
            currentLocalPos = thisTransform.localPosition;
            currentDistance = currentLocalPos.x;
            targetDistance = currentDistance;
        }


        void FixedUpdate()
        {
            // Check the camera.
            if (mainCamera.enabled == false)
            { // It should be aiming now.
                return;
            }

            // Check the target exists.
            if (targetRoot == null)
            { // The target has been lost.
                return;
            }

            Avoid_Obstacle();
            Move();
        }


        void Avoid_Obstacle()
        {
            // Detect an obstacle by casting a ray from the camera pivot to the camera.
            Ray ray = new Ray();
            ray.origin = parentTransform.position;
            ray.direction = (thisTransform.position - parentTransform.position);
            RaycastHit[] raycastHits = Physics.RaycastAll(ray, thisTransform.localPosition.x + 1.0f, Layer_Settings_CS.Layer_Mask);
            for (int i = 0; i < raycastHits.Length; i++)
            {
                if (raycastHits[i].transform.root != targetRoot)
                { // The hit object is not the target.
                    if (isAvoidingObstacle == false)
                    { // Not avoiding now.
                        hittingTime += Time.deltaTime;
                        if (hittingTime > General_Settings_CS.cameraAvoidLag)
                        {
                            // Start avoiding the obstacle.
                            hittingTime = 0.0f;
                            isAvoidingObstacle = true;
                            storedDistance = targetDistance;
                            targetDistance = raycastHits[i].distance;
                            targetDistance = Mathf.Clamp(targetDistance, General_Settings_CS.cameraAvoidMinDist, General_Settings_CS.cameraAvoidMaxDist);
                        }
                    } // Avoiding now.
                    else if (raycastHits[i].distance < storedDistance)
                    { // Find a new obstacle that is closer to the target.
                        targetDistance = raycastHits[i].distance;
                        targetDistance = Mathf.Clamp(targetDistance, General_Settings_CS.cameraAvoidMinDist, General_Settings_CS.cameraAvoidMaxDist);
                    }
                    return;
                } // The hit object is the target.
            } // The ray does not hit anything.

            // Return the camera to the stored position.
            if (isAvoidingObstacle)
            {
                isAvoidingObstacle = false;
                targetDistance = storedDistance;
            }
        }


        void Move()
        {
            // Move the camera forward and backward.
            if (currentDistance != targetDistance)
            {
                currentDistance = Mathf.MoveTowards(currentDistance, targetDistance, General_Settings_CS.cameraAvoidMoveSpeed * Time.fixedDeltaTime);
                currentLocalPos.x = currentDistance;
                thisTransform.localPosition = currentLocalPos;
            }
        }


        void Pause(bool isPaused)
        { // Called from "Game_Controller_CS".
            this.enabled = !isPaused;
        }
    }

}