using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadarRotation : MonoBehaviour {
	public float Speed = 100f;

	void Update () {
		transform.Rotate(Vector3.up, Speed * Time.deltaTime);
	}
}
