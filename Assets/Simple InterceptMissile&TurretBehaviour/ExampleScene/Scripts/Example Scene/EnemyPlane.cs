using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPlane : MonoBehaviour {

	public float Speed;
	public float TurnRate;

	
	void FixedUpdate () {

		transform.Rotate(0f, TurnRate * Time.deltaTime, 0f);
		transform.Translate(0f, 0f, Speed * Time.deltaTime);
	}
}
