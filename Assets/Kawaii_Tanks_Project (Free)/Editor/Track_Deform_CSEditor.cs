using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;

namespace ChobiAssets.KTP
{

    [CustomEditor(typeof(Track_Deform_CS))]
    public class Track_Deform_CSEditor : Editor
    {

        SerializedProperty anchorNumProp;
        SerializedProperty anchorArrayProp;
        SerializedProperty anchorNamesProp;
        SerializedProperty anchorParentNamesProp;
        SerializedProperty widthArrayProp;
        SerializedProperty heightArrayProp;
        SerializedProperty offsetArrayProp;

        SerializedProperty hasChangedProp;

        Transform thisTransform;


        void OnEnable()
        {
            anchorNumProp = serializedObject.FindProperty("anchorNum");
            anchorArrayProp = serializedObject.FindProperty("anchorArray");
            anchorNamesProp = serializedObject.FindProperty("anchorNames");
            anchorParentNamesProp = serializedObject.FindProperty("anchorParentNames");
            widthArrayProp = serializedObject.FindProperty("widthArray");
            heightArrayProp = serializedObject.FindProperty("heightArray");
            offsetArrayProp = serializedObject.FindProperty("offsetArray");

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
        }


        void Set_Inspector()
        {
            if (EditorApplication.isPlaying == false)
            {
                serializedObject.Update();

                GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f, 1.0f);

                EditorGUILayout.Space();

                EditorGUILayout.HelpBox("Track Deform settings", MessageType.None, true);

                if (GUILayout.Button("Find RoadWheels [ Left ]", GUILayout.Width(200)))
                {
                    Find_RoadWheels(true);
                }
                if (GUILayout.Button("Find RoadWheels [ Right ]", GUILayout.Width(200)))
                {
                    Find_RoadWheels(false);
                }
                EditorGUILayout.Space();

                EditorGUILayout.IntSlider(anchorNumProp, 1, 64, "Number of Anchor Wheels");
                EditorGUILayout.Space();

                anchorArrayProp.arraySize = anchorNumProp.intValue;
                widthArrayProp.arraySize = anchorNumProp.intValue;
                anchorNamesProp.arraySize = anchorNumProp.intValue;
                anchorParentNamesProp.arraySize = anchorNumProp.intValue;
                heightArrayProp.arraySize = anchorNumProp.intValue;
                offsetArrayProp.arraySize = anchorNumProp.intValue;
                for (int i = 0; i < anchorArrayProp.arraySize; i++)
                {
                    anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = EditorGUILayout.ObjectField("Anchor Wheel", anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue, typeof(Transform), true);
                    if (anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue != null)
                    {
                        Transform tempTransform = anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                        anchorNamesProp.GetArrayElementAtIndex(i).stringValue = tempTransform.name;
                        if (tempTransform.parent)
                        {
                            anchorParentNamesProp.GetArrayElementAtIndex(i).stringValue = tempTransform.parent.name;
                        }
                    }
                    else
                    {
                        // Find Anchor with reference to the name.
                        string temp_AnchorName = anchorNamesProp.GetArrayElementAtIndex(i).stringValue;
                        string temp_AnchorParentName = anchorParentNamesProp.GetArrayElementAtIndex(i).stringValue;
                        if (string.IsNullOrEmpty(temp_AnchorName) == false && string.IsNullOrEmpty(temp_AnchorParentName) == false)
                        {
                            anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = thisTransform.transform.parent.Find(temp_AnchorParentName + "/" + temp_AnchorName);
                        }
                    }

                    EditorGUILayout.Slider(widthArrayProp.GetArrayElementAtIndex(i), 0.0f, 10.0f, "Width");
                    if (widthArrayProp.GetArrayElementAtIndex(i).floatValue == 0.0f)
                    {
                        widthArrayProp.GetArrayElementAtIndex(i).floatValue = 0.5f;
                    }
                    EditorGUILayout.Slider(heightArrayProp.GetArrayElementAtIndex(i), 0.0f, 10.0f, "Height");
                    if (heightArrayProp.GetArrayElementAtIndex(i).floatValue == 0.0f)
                    {
                        heightArrayProp.GetArrayElementAtIndex(i).floatValue = 1.0f;
                    }
                    EditorGUILayout.Slider(offsetArrayProp.GetArrayElementAtIndex(i), -10.0f, 10.0f, "Offset");
                    EditorGUILayout.Space();
                }

                // Update Value
                if (GUI.changed || GUILayout.Button("Update Values") || Event.current.commandName == "UndoRedoPerformed")
                {
                    hasChangedProp.boolValue = !hasChangedProp.boolValue;
                    Set_Vertices();
                }

                EditorGUILayout.Space();
                EditorGUILayout.Space();
                
                serializedObject.ApplyModifiedProperties();
            }
        }


        void Set_Vertices()
        {
            GameObject thisGameObject = Selection.activeGameObject;
            if (thisGameObject.GetComponent<MeshFilter>().sharedMesh == null)
            {
                Debug.LogError("Mesh is not assigned in the Mesh Filter.");
                return;
            }
            Mesh thisMesh = thisGameObject.GetComponent<MeshFilter>().sharedMesh;
            float[] initialPosArray = new float[anchorArrayProp.arraySize];
            IntArray[] movableVerticesList = new IntArray[anchorArrayProp.arraySize];
            // Get vertices in the range.
            for (int i = 0; i < anchorArrayProp.arraySize; i++)
            {
                if (anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue != null)
                {
                    Transform anchorTransform = anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                    initialPosArray[i] = anchorTransform.localPosition.x;
                    Vector3 anchorPos = thisGameObject.transform.InverseTransformPoint(anchorTransform.position);
                    List<int> withinVerticesList = new List<int>();
                    for (int j = 0; j < thisMesh.vertices.Length; j++)
                    {
                        float distZ = Mathf.Abs(anchorPos.z - thisMesh.vertices[j].z);
                        float distY = Mathf.Abs((anchorPos.y + offsetArrayProp.GetArrayElementAtIndex(i).floatValue) - thisMesh.vertices[j].y);
                        if (distZ <= widthArrayProp.GetArrayElementAtIndex(i).floatValue * 0.5f && distY <= heightArrayProp.GetArrayElementAtIndex(i).floatValue * 0.5f)
                        {
                            withinVerticesList.Add(j);
                        }
                    }
                    IntArray withinVerticesArray = new IntArray(withinVerticesList.ToArray());
                    movableVerticesList[i] = withinVerticesArray;
                }
            }
            // Set values.
            EditorApplication.delayCall += () =>
            {
                Track_Deform_CS deformScript = thisGameObject.GetComponent<Track_Deform_CS>();
                deformScript.initialPosArray = initialPosArray;
                deformScript.initialVertices = thisMesh.vertices;
                deformScript.movableVerticesList = movableVerticesList;
            };
        }

        void Find_RoadWheels(bool isLeft)
        {
            // Find RoadWheels.
            List<Transform> roadWheelsList = new List<Transform>();
            List<Transform> tempList = new List<Transform>();
            Transform bodyTransform = Selection.activeGameObject.transform.parent;
            Wheel_Rotate_CS[] wheelScripts = bodyTransform.GetComponentsInChildren<Wheel_Rotate_CS>();
            foreach (Wheel_Rotate_CS wheelScript in wheelScripts)
            {
                Transform connectedTransform = wheelScript.GetComponent<HingeJoint>().connectedBody.transform;
                MeshFilter meshFilter = wheelScript.GetComponent<MeshFilter>();
                if (connectedTransform != bodyTransform && meshFilter && meshFilter.sharedMesh)
                { // connected to Suspension && not invisible.
                    if (isLeft)
                    { // Left.
                        if (wheelScript.transform.localEulerAngles.z == 0.0f)
                        { // Left.
                            tempList.Add(wheelScript.transform);
                        }
                    }
                    else
                    { // Right.
                        if (wheelScript.transform.localEulerAngles.z != 0.0f)
                        { // Right.
                            tempList.Add(wheelScript.transform);
                        }
                    }
                }
            }
            // Sort (rear >> front)
            int tempCount = tempList.Count;
            for (int i = 0; i < tempCount; i++)
            {
                float maxPosZ = Mathf.Infinity;
                int targetIndex = 0;
                for (int j = 0; j < tempList.Count; j++)
                {
                    if (tempList[j].position.z < maxPosZ)
                    {
                        maxPosZ = tempList[j].position.z;
                        targetIndex = j;
                    }
                }
                roadWheelsList.Add(tempList[targetIndex]);
                tempList.RemoveAt(targetIndex);
            }
            // Set
            anchorNumProp.intValue = roadWheelsList.Count;
            anchorArrayProp.arraySize = anchorNumProp.intValue;
            widthArrayProp.arraySize = anchorNumProp.intValue;
            heightArrayProp.arraySize = anchorNumProp.intValue;
            offsetArrayProp.arraySize = anchorNumProp.intValue;
            for (int i = 0; i < anchorArrayProp.arraySize; i++)
            {
                anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue = roadWheelsList[i];
            }
        }


        void OnSceneGUI()
        {
            // Draw the square.
            if (anchorArrayProp != null && anchorArrayProp.arraySize != 0 && offsetArrayProp != null && offsetArrayProp.arraySize != 0)
            {
                Handles.color = Color.green;
                for (int i = 0; i < anchorArrayProp.arraySize; i++)
                {
                    if (anchorArrayProp.GetArrayElementAtIndex(i) != null)
                    {
                        var tempSize = new Vector3(0.0f, heightArrayProp.GetArrayElementAtIndex(i).floatValue, widthArrayProp.GetArrayElementAtIndex(i).floatValue);
                        var anchorTransform = anchorArrayProp.GetArrayElementAtIndex(i).objectReferenceValue as Transform;
                        if (anchorTransform == null)
                        {
                            continue;
                        }
                        var tempCenter = anchorTransform.position;
                        tempCenter.y += offsetArrayProp.GetArrayElementAtIndex(i).floatValue;
                        Handles.DrawWireCube(tempCenter, tempSize);
                    }
                }
            }

            // Draw vertices.
            MeshFilter thisFilter = thisTransform.GetComponent<MeshFilter>();
            if (thisFilter && thisFilter.sharedMesh)
            {
                Handles.matrix = Matrix4x4.TRS(Vector3.zero, thisTransform.rotation, thisTransform.localScale);
                Vector3[] vertices = thisFilter.sharedMesh.vertices;
                for (int i = 0; i < vertices.Length; i++)
                {
                    Handles.color = Color.red;
                    Handles.DrawWireCube(vertices[i] + thisTransform.position, Vector3.one * 0.05f);
                }
            }
                
        }

    }

}