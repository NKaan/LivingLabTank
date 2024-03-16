using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Track_Scroll_CS))]
    public class Track_Scroll_CSEditor : Editor
    {

        SerializedProperty isLeftProp;
        SerializedProperty referenceWheelProp;
        SerializedProperty referenceNameProp;
        SerializedProperty referenceParentNameProp;
        SerializedProperty scrollRateProp;
        SerializedProperty scrollYAxisProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;


        void OnEnable()
        {
            isLeftProp = serializedObject.FindProperty("isLeft");
            referenceWheelProp = serializedObject.FindProperty("referenceWheel");
            referenceNameProp = serializedObject.FindProperty("referenceName");
            referenceParentNameProp = serializedObject.FindProperty("referenceParentName");
            scrollRateProp = serializedObject.FindProperty("scrollRate");
            scrollYAxisProp = serializedObject.FindProperty("scrollYAxis");

            hasChangedProp = serializedObject.FindProperty("hasChanged");

            if (Selection.activeGameObject)
            {
                thisTransform = Selection.activeGameObject.transform;
            }
        }


        public override void OnInspectorGUI()
        {
            bool isPrepared;
            if (Application.isPlaying || thisTransform.parent == null || thisTransform.parent.gameObject.GetComponent<Rigidbody>() == null)
            {
                isPrepared = false;
            }
            else
            {
                isPrepared = true;
            }

            if (isPrepared)
            {
                // Set Inspector window.
                Set_Inspector();
            }
            else
            {
                if (Application.isPlaying)
                {
                    Inspector_In_Runtime();
                }
            }
        }


        void Set_Inspector()
        {
            serializedObject.Update();
            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("Track Scroll settings", MessageType.None, true);

            if (GUILayout.Button("Find RoadWheel [ Left ]", GUILayout.Width(200)))
            {
                isLeftProp.boolValue = true;
                Find_RoadWheel(isLeftProp.boolValue);
            }
            if (GUILayout.Button("Find RoadWheel [ Right ]", GUILayout.Width(200)))
            {
                isLeftProp.boolValue = false;
                Find_RoadWheel(isLeftProp.boolValue);
            }
            EditorGUILayout.Space();

            isLeftProp.boolValue = EditorGUILayout.Toggle("Left", isLeftProp.boolValue);
            referenceWheelProp.objectReferenceValue = EditorGUILayout.ObjectField("Reference Wheel", referenceWheelProp.objectReferenceValue, typeof(Transform), true);
            if (referenceWheelProp.objectReferenceValue != null)
            {
                Transform tempTransform = referenceWheelProp.objectReferenceValue as Transform;
                referenceNameProp.stringValue = tempTransform.name;
                if (tempTransform.parent)
                {
                    referenceParentNameProp.stringValue = tempTransform.parent.name;
                }
            }
            else
            {
                // Find Reference Wheel with reference to the name.
                string tempName = referenceNameProp.stringValue;
                string tempParentName = referenceParentNameProp.stringValue;
                if (string.IsNullOrEmpty(tempName) == false && string.IsNullOrEmpty(tempParentName) == false)
                {
                    referenceWheelProp.objectReferenceValue = thisTransform.parent.Find(tempParentName + "/" + tempName);
                }
            }

            EditorGUILayout.Slider(scrollRateProp, -0.01f, 0.01f, "Scroll Rate");
            scrollYAxisProp.boolValue = EditorGUILayout.Toggle("Scroll Y Axis", scrollYAxisProp.boolValue);

            // Update Value
            if (GUI.changed || Event.current.commandName == "UndoRedoPerformed")
            {
                hasChangedProp.boolValue = !hasChangedProp.boolValue;
                Find_RoadWheel(isLeftProp.boolValue);
            }

            EditorGUILayout.Space();

            serializedObject.ApplyModifiedProperties();
        }


        void Inspector_In_Runtime()
        {
            serializedObject.Update();
            EditorGUILayout.Space();
            GUI.backgroundColor = new Color(0.7f, 0.7f, 1.0f, 1.0f);
            EditorGUILayout.Slider(scrollRateProp, -0.01f, 0.01f, "Scroll Rate");
            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }


        void Find_RoadWheel(bool isLeft)
        {
            Transform bodyTransform = Selection.activeGameObject.transform.parent;
            Wheel_Rotate_CS[] wheelScripts = bodyTransform.GetComponentsInChildren<Wheel_Rotate_CS>();
            float minDist = Mathf.Infinity;
            Transform closestWheel = null;
            foreach (Wheel_Rotate_CS wheelScript in wheelScripts)
            {
                Transform connectedTransform = wheelScript.GetComponent<HingeJoint>().connectedBody.transform;
                MeshFilter meshFilter = wheelScript.GetComponent<MeshFilter>();
                if (connectedTransform != bodyTransform && meshFilter && meshFilter.sharedMesh)
                { // connected to Suspension && not invisible.
                    float tempDist = Vector3.Distance(bodyTransform.position, wheelScript.transform.position); // Distance to the MainBody.
                    if (isLeft)
                    { // Left.
                        if (wheelScript.transform.localEulerAngles.z == 0.0f)
                        { // Left.
                            if (tempDist < minDist)
                            {
                                closestWheel = wheelScript.transform;
                                minDist = tempDist;
                            }
                        }
                    }
                    else
                    { // Right.
                        if (wheelScript.transform.localEulerAngles.z != 0.0f)
                        { // Right.
                            if (tempDist < minDist)
                            {
                                closestWheel = wheelScript.transform;
                                minDist = tempDist;
                            }
                        }
                    }
                }
            }
            referenceWheelProp.objectReferenceValue = closestWheel;
        }
    }

}
