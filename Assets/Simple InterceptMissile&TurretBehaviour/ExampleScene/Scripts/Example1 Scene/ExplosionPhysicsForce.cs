using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionPhysicsForce : MonoBehaviour
{
	public float ExplosionForce = 4;
	public float Radius = 10;

	private IEnumerator Start()
	{
		// wait one frame because some explosions instantiate debris which should then
		// be pushed by physics force
		yield return null;

		float r = 10;
		var cols = Physics.OverlapSphere(transform.position, r);
		var rigidbodies = new List<Rigidbody>();
		foreach (var col in cols)
		{
			if (col.attachedRigidbody != null && !rigidbodies.Contains(col.attachedRigidbody))
			{
				rigidbodies.Add(col.attachedRigidbody);
			}
		}
		foreach (var rb in rigidbodies)
		{
			rb.AddExplosionForce(ExplosionForce, transform.position, r, 10, ForceMode.Impulse);
		}
	}
}
