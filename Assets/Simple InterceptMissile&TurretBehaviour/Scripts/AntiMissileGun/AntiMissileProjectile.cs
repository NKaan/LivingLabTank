using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiMissileProjectile : PoolObject {

	[Header("Projectile settings")]
	[Tooltip("Projectile traveling speed")]
	[HideInInspector]
	public float Speed; 

	[Tooltip("Projectile life time")]
	public float TimeTodestroy;

	[Tooltip("Projectile Explosion FX (Optional)")]
	public GameObject Explosion;
	
	private void Update()
	{
		// Move projectile
		transform.Translate(Vector3.forward * Speed * Time.deltaTime);		
	}

	private void OnEnable()
	{
		StartCoroutine(DestroyDelay()); // Destroy this projectile after TimeToDestroy time, every time this projectile is enable
	}

	// Destroy gameobject when collisin happen
	protected virtual void OnCollisionEnter(Collision col)
	{	
		if(IsPooling)
			Destroy(gameObject); // disable this projectile
		else
			Destroy(gameObject, 0.1f); // if not pooling destroy it immediately
		
		if(Explosion != null)
		Instantiate(Explosion, transform.position, transform.rotation);
	}

	IEnumerator DestroyDelay()
	{
		yield return new WaitForSeconds(TimeTodestroy);
		if(IsPooling)
			Destroy(gameObject); // disable this projectile
		else
			Destroy(gameObject,0.1f); // if not pooling destroy it immediately
		
		if(Explosion != null)
		Instantiate(Explosion, transform.position, transform.rotation);

	}

	public override void OnobjectReuse(Vector3 target, float speed)
	{
		transform.LookAt(target);
		Speed = speed;
	}



	
}
