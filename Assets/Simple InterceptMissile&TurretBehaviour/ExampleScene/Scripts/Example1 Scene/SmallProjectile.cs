using UnityEngine;

public class SmallProjectile : AntiMissileProjectile {
	[Header("Child class")]
	[Tooltip("parameters of child class")]
	
	public GameObject Blood_FX;
	public GameObject Dirt_FX;

	protected override void OnCollisionEnter(Collision col)
	{
		if(IsPooling)
			Destroy(gameObject); // disable this projectile
		else
			Destroy(gameObject, 0.1f); // if not pooling destroy it immediately

		var layer = LayerMask.LayerToName(col.gameObject.layer);

		if(layer == "EnemyPlane" || layer == "EnemyTank")
		{
			if(Dirt_FX != null)
				Instantiate(Dirt_FX, transform.position, transform.rotation);
		}
		else if(layer == "EnemySoldier")
		{
			if(Blood_FX != null)
				Instantiate(Blood_FX, transform.position, transform.rotation);
		}
		else
			if(Dirt_FX != null)
				Instantiate(Dirt_FX, transform.position, transform.rotation);	
	}
}
