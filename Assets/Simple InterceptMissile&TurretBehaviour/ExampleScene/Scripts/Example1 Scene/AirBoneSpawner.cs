using UnityEngine;

public class AirBoneSpawner : MonoBehaviour {

	public Vector3 AreaSize;
	public GameObject AirbonesPrefabs;
	public float RespawnCount;

	void OnTriggerEnter(Collider collision)
	{	
		
		if(LayerMask.LayerToName(collision.gameObject.layer) == "AirboneTriggerArea")
		{
			for(int i = 0; i < RespawnCount; i++)
			{	
				Vector3 pos = transform.position + new Vector3(Random.Range(-AreaSize.x/2, AreaSize.x/2),Random.Range(-AreaSize.y/2, AreaSize.y/2),Random.Range(-AreaSize.z/2, AreaSize.z/2));
				Instantiate(AirbonesPrefabs, pos, transform.rotation);
			}
		}
	}

	void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(transform.localPosition, AreaSize);
	}
}
