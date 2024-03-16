using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InterceptMissileController : NetworkBehaviour {
	
	[Header("Turret Settings")]
	[Tooltip("Pivot for horizontal rotation")]
	public Transform HorizontalPivot;

	[Header("Rotation Settings")]
	[Tooltip("If you want to limit turret rotation")]
	public bool RotationLimit;

	[Tooltip("Right rotation limit")]
	[Range(0,180)]
	public float RightRotationLimit; 

	[Tooltip("Left rotation limit")]
	[Range(0,180)]
	public float LeftRotationLimit; 

	[Tooltip("Turning speed")]
	[Range(0,300)]
	public float TurnSpeed;

	[Header("Missile settings")]
	[Tooltip("How many missile in turret")]
	[SyncVar]
	public float MissileCount;

	[Tooltip("Launcher Spot")]
	public Transform[] LaunchSpot;

	[Tooltip("Missile Prefab")]
	public InterceptMissile missile;

	[HideInInspector]
	public Transform target; // Target position

	[HideInInspector]
	public float loadedMissileCount; // Count of loaded missile on launcher

	private List<InterceptMissile> loadedMissile = new List<InterceptMissile>(); // loaded missile list on launcher
	
	
	private void Start()
	{
		target = null;
		if(isServer)
			SpawnMissile(); // spawn missile
	}

	private void Update()
	{	
		//HorizontalRotation();
	}

	public void ResetSpawn()
	{
		StartCoroutine(RespawnMissile());

    }

	IEnumerator RespawnMissile()
	{	
		yield return new WaitForSeconds(2);
		if(MissileCount <= 0) yield return 0;
		SpawnMissile();
	}

	private void SpawnMissile()
	{	
		if(LaunchSpot.Length == 0) // check for missile launchSpot
		{
			Debug.Log("No LaunchSpot found, Please drag it into this script");
		}
		
		foreach(Transform spot in LaunchSpot)
		{	
			if(MissileCount <= 0) return;

			InterceptMissile newMissile = Instantiate(missile, spot.position, spot.rotation);
            newMissile.killerPlayerID = netId;
            NetworkServer.Spawn(newMissile.gameObject);
            newMissile.transform.parent = spot;
			
            SetMissileParent(newMissile.gameObject,netId, spot.gameObject.name);

            //Vector3 offset = new Vector3(0,8,-0.3f); 
            //newMissile.transform.localPosition = offset; // Note: optional position

            loadedMissile.Add(newMissile);
			//CameraManager.CameraTargets.Add(newMissile.transform); // just for missile camera
			loadedMissileCount ++;
			MissileCount --;
		}
	}

	[ClientRpc]
	public void SetMissileParent(GameObject newMissile, uint netID, string parentObjName)
	{
		Player player = NetworkClient.spawned[netID].GetComponent<Player>();
		Transform childObject = player.transform.FindChildObjectByName(parentObjName);
		newMissile.transform.parent = childObject;
    }

    

    private void HorizontalRotation()
	{
		if(HorizontalPivot == null || target == null) return;

			// Get target position from world space to local space from our  parent position (this.transfrom)
			Vector3 targetPositionInLocalSpace = this.transform.InverseTransformPoint(target.position);
			// Set "TargetPositionInLocalSpace" Y axis to zero, since this is horizontal rotation
			targetPositionInLocalSpace.y = 0f;
			
			// Store clamp value of the rotation
			Vector3 clamp = targetPositionInLocalSpace;
			// Clamp turret horizontal rotation according to its limit
			if(RotationLimit)
			{
				if(targetPositionInLocalSpace.x >= 0f)
					clamp = Vector3.RotateTowards(Vector3.forward, targetPositionInLocalSpace, Mathf.Deg2Rad * RightRotationLimit, 0f);
				else
					clamp = Vector3.RotateTowards(Vector3.forward, targetPositionInLocalSpace, Mathf.Deg2Rad * LeftRotationLimit, 0f);	
			}

			Debug.DrawLine(HorizontalPivot.position, target.position, Color.red);
			// Rotate turret
			Quaternion whereToRotate = Quaternion.LookRotation(clamp);	
			HorizontalPivot.localRotation = Quaternion.RotateTowards(HorizontalPivot.localRotation, whereToRotate, TurnSpeed * Time.deltaTime);
	}

	public void SetTargetMissile(Transform targetPosition)
	{	
		this.target = targetPosition;
		Launch(targetPosition);
	}

	private void Launch(Transform targetPosition)
	{			
		loadedMissile[(int)loadedMissileCount - 1].Launch(targetPosition); // Launch missile according to its sequence in list
		loadedMissile[((int)loadedMissileCount - 1)].transform.parent = null; 
		loadedMissile.Remove(loadedMissile[((int)loadedMissileCount - 1)]); // Remove missile from loaded missile list
		loadedMissileCount --;

		if(loadedMissileCount <= 0)   
			StartCoroutine(RespawnMissile()); //if loaded missile on launcher is null Respawn
	}

	
}
