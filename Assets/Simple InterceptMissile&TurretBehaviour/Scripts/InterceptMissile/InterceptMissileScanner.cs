using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterceptMissileScanner : MonoBehaviour {
	
	[Header("Settings")]
	[Tooltip("How often to scan in second")]
	public float ScanSpeed;

	[Tooltip("Scanner view angle")]
	[Range(0,360)]
	public float ViewAngle;

	[Tooltip(" Layers the scanner will detect")]
	public LayerMask Mask;

	[Tooltip("Get scanner range / or radius")]
	public float ScanRadius;

	[Tooltip("On or Off gizmos")]
	public bool ShowGizmos;

	[Tooltip("Turret Controller, Note: you can put Scanner in 'TurretController' or seperate it")]
	public InterceptMissileController InterceptMissileController;

	private List<Transform> targetList = new List<Transform>(); // List of targets position
	private List<Transform> curretTargets = new List<Transform>(); // Register current target;

	private void Start()
	{
		if (!InterceptMissileController.isServer)
			return;

		StartCoroutine(ScanIteration()); // Start scan for target
		if(InterceptMissileController == null)
		{
			Debug.Log("No controller found, Please drag it into this script");
		}
	}


	IEnumerator ScanIteration() // repeat scanning
	{
		while(true)
		{	
			yield return new WaitForSeconds(ScanSpeed);
			ScanForTarget();
		}
	}


	public Vector3 GetViewAngle(float angle)
	{	
		// Calculate the Vector3 of the given angle for visualisation
		float radiant = (angle + transform.eulerAngles.y) * Mathf.Deg2Rad;
		return new Vector3(Mathf.Sin(radiant), 0, Mathf.Cos(radiant));
	}

	
	private void OnDrawGizmos()
	{	
		if(!ShowGizmos) return; // Show the visualisation if "ShowGizmos" is true

		Gizmos.color = Color.black;
		Gizmos.DrawWireSphere(transform.position, ScanRadius);
		Gizmos.DrawLine(transform.position, transform.position + GetViewAngle(ViewAngle /2) * ScanRadius);
		Gizmos.DrawLine(transform.position, transform.position + GetViewAngle(-ViewAngle / 2) * ScanRadius);

		Gizmos.color = Color.red;
		if(targetList.Count == 0) return;
		foreach(Transform target in targetList)
		{	
			if(target == null) continue;
			Gizmos.DrawLine(transform.position, target.position);
		}
	}

	public void ScanForTarget()
	{	
		targetList.Clear();
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ScanRadius, Mask); // Set up an OverlapSphere within certain radius and mask 

		for(int i = 0; i < targetsInViewRadius.Length; i ++) // Loop every result that collided
		{
			Transform targetPosition = targetsInViewRadius[i].transform;

			if (targetPosition == transform)
				continue;

			Vector3 dirTotarget = (targetPosition.position - transform.position).normalized;
			if(Vector3.Angle(transform.forward, dirTotarget) < ViewAngle / 2) // If target is in our viewAngle area add it into "targetList"
			{
				targetList.Add(targetPosition);		
				SetTargetMissile(targetPosition);			
			}
		}
	}

	IEnumerator ClearCurrentTarget(Transform current)
	{	
		yield return new WaitForSeconds(10);
		curretTargets.Remove(current);
	}

	private void SetTargetMissile(Transform targetPosition)
	{	
		// Make sure one target one missile
		foreach(Transform target in curretTargets)
		{	
			if(target == targetPosition) return;
		}
		if(InterceptMissileController.loadedMissileCount <= 0) return; // if loaded missile on launcher is null return

		InterceptMissileController.SetTargetMissile(targetPosition);
		curretTargets.Add(targetPosition);
		StartCoroutine(ClearCurrentTarget(targetPosition)); // Clear current target in 10 seconds
	}
}
