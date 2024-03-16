using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerRotator : MonoBehaviour {

	[SerializeField] private float _speed = 100.0f;
	// Update is called once per frame
	void Update () {
		transform.Rotate(Vector3.forward * _speed * Time.deltaTime);
	}
}
