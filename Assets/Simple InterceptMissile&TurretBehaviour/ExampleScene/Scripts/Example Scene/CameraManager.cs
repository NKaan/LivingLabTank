using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	[Tooltip("")]
	public Camera Camera;

	[Tooltip("")]
	public Vector3 offset = new Vector3(-4,4,2);

	[Tooltip("")]
	[HideInInspector]
	public static List<Transform> CameraTargets = new List<Transform>();

	private int currentTarget;

	private void Start()
	{
		currentTarget = 0;
	}

	private void Update()
	{	
		if(CameraTargets.Count == 0) return; 

		 if (Input.GetKeyDown("x"))
		 {	
			 currentTarget ++;
			 if(currentTarget > CameraTargets.Count - 1)
			 	currentTarget = 0;
		 }
            
        
        if (Input.GetKeyDown("z"))
		{
			currentTarget --;
			if(currentTarget < 0)
			 	currentTarget = CameraTargets.Count - 1;
		}

		if(CameraTargets[currentTarget] == null)
		{
			if(CameraTargets.Count == 0) return; 

			CameraTargets.Remove(CameraTargets[currentTarget]);
			currentTarget ++;
			if(currentTarget > CameraTargets.Count - 1)
				currentTarget = 0;
		}
	}

	private void LateUpdate()
	{	
		if(CameraTargets.Count == 0) return; 

		Transform target = CameraTargets[currentTarget];

		if(CameraTargets[currentTarget] == null) return;

		Camera.transform.position = target.position;
		Camera.transform.rotation = target.rotation;
		Camera.transform.position += offset; 
	}
}
