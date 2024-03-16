using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour {

	public float TimeToDestroy;
	void Start () 
	{
		Destroy(gameObject, TimeToDestroy);
	}
	
	
}
