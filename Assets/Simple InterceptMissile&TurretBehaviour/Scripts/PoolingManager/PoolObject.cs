using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolObject : MonoBehaviour {
	[HideInInspector]
	public bool IsPooling; 
	public virtual void OnobjectReuse(){}

	public virtual void OnobjectReuse(Vector3 target, float speed){}

	protected void Destroy(GameObject gameObject)
	{
		gameObject.SetActive(false);
	}
}
