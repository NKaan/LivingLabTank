using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MachineGunController : AntiMissileGunController {

	[Header("Child class")]
	[Tooltip("parameters of child class")]
	public Animator _animator;
	private int _animatorFireHash = Animator.StringToHash("Fire");

	protected override void Start()
	{	
		base.Start();
	}

	protected override void Fire()
	{	
		_animator.SetBool(_animatorFireHash, false);
		
		// Only fire when there is a target, turret is aiming and within fire rate
		if(target != null && ProjectileCount > 0 && Time.time > nextFireAllowed && IsAiming)
		{	
			for(int i = 0; i < Barrel.Length; i++)
			{	
				if(UsePooling)
					PoolManager.instance.ReuseObject(ProjectilePrefab, Barrel[i].position, Barrel[i].rotation, predictedTargetPosition, ProjectileSpeed);
				else
				{
					AntiMissileProjectile newProjectile = Instantiate(ProjectilePrefab, Barrel[i].position, Barrel[i].rotation).GetComponent<AntiMissileProjectile>();							
					newProjectile.transform.LookAt(predictedTargetPosition);
					newProjectile.Speed = this.ProjectileSpeed;
				}
				
				ProjectileCount --;
			}
			nextFireAllowed = Time.time + FireRate;
			
			// Play Effects
			if(BulletShellFX != null)
			{
				bulletShellFX_PS.Play();
				Invoke("StopBulletShellEffect", 1.2f);
			}
			
			_animator.SetBool(_animatorFireHash, true);

			if(ShootFX == null) return;
			shootFX_PS.Play();
			
		}
	}

}
