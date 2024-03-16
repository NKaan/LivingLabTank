using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiMissileGunScanner : MonoBehaviour {
	public enum Mode 
	{
		NEAREST,
		FURTHEST
	}

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

	[Tooltip("Turret Controller")]
	public AntiMissileGunController AntiMissileGunController;

	[Tooltip("Turret modes NOTE: Only working for anti missile gun controller")]
	public Mode TurretModes = Mode.NEAREST;

	private List<Transform> targetList = new List<Transform>(); // List of targets position

	private void Start()
	{	
		if(AntiMissileGunController == null)
			Debug.Log("No controller found, Please drag it into this script");
		StartCoroutine(ScanIteration()); // Start scan for target
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

		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, ScanRadius);
		Gizmos.DrawLine(transform.position, transform.position + GetViewAngle(ViewAngle / 2) * ScanRadius);
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
		//Debug.Log("Scanning");
		targetList.Clear();
		Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, ScanRadius, Mask); // Set up an OverlapSphere within certain radius and mask 

		for(int i = 0; i < targetsInViewRadius.Length; i ++) // Loop every result that collided
		{
			Transform targetPosition = targetsInViewRadius[i].transform;
			Vector3 dirTotarget = (targetPosition.position - transform.position).normalized;
			if(Vector3.Angle(transform.forward, dirTotarget) < ViewAngle / 2) // If target is in our viewAngle area add it into "targetList"
			{
				targetList.Add(targetPosition);			
			}
		}
		
		// Select target 
		switch(TurretModes)
		{
			case Mode.NEAREST:
			SelectClosestTarget();
			break;

			case Mode.FURTHEST:
			SelectFurthersTarget();
			break;
		}
	}

	
	private void SelectClosestTarget()
	{	
		if(targetList.Count == 0) return;

		if(targetList.Count == 1)
		{
			SetTargetGun(targetList[0]);
			return;
		}

		Transform curretTargets = null;
		float closestDistance = 0f;
		for(int i = 0; i < targetList.Count; i++)
		{
			float distance = Vector3.Distance(transform.position, targetList[i].position);

			if(curretTargets == null || distance < closestDistance)
			{
				curretTargets = targetList[i];
				closestDistance = distance;
				SetTargetGun(curretTargets);
			}
		}
	}

	
	private void SelectFurthersTarget()
	{	
		if(targetList.Count == 0) return;

		if(targetList.Count == 1)
		{
			SetTargetGun(targetList[0]);
			return;
		}

		Transform curretTargets = null;
		float furthestDistance = 0f;
		for(int i = 0; i < targetList.Count; i++)
		{
			float distance = Vector3.Distance(transform.position, targetList[i].position);

			if(curretTargets == null || distance > furthestDistance)
			{
				curretTargets = targetList[i];
				furthestDistance = distance;
				SetTargetGun(curretTargets);
			}
		}
	}


	
	private void SetTargetGun(Transform targetPosition)
	{
		AntiMissileGunController.SetTargetGun(targetPosition);
	}
}
