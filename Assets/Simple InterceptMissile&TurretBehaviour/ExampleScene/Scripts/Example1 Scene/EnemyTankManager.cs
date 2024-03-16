using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTankManager : MonoBehaviour {

	public Vector3 AreaSize;
	public Tank TankPrefabs;
	public float RespawnCount;
	public float LaunchRate;

	private float nextlaunch;
	private bool Fire;
	
	
	// Update is called once per frame
	void Update () {

		Fire =  Input.GetKey(KeyCode.LeftShift);

		// Only fire the missile when user give some input and time more than next launch time;
		if(Fire && Time.time >= nextlaunch)
		{	
			nextlaunch = Time.time + LaunchRate;
			LaunchTank();
		}
	}

	void LaunchTank()
	{	
		for (int i=0; i<RespawnCount; i++)
		{
			Vector3 pos = transform.localPosition + new Vector3(Random.Range(-AreaSize.x/2, AreaSize.x/2),Random.Range(-AreaSize.y/2, AreaSize.y/2),Random.Range(-AreaSize.z/2, AreaSize.z/2));
			Instantiate(TankPrefabs, pos, transform.rotation);
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawCube(transform.localPosition, AreaSize);
	}
}
