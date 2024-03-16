using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntiMissileGunController : MonoBehaviour {

	[Header("Turret Settings")]
	[Tooltip("Pivot for horizontal rotation")]
	public Transform HorizontalPivot;

	[Tooltip("Pivot for vertical rotation")]
	public Transform VerticalPivot;

	[Header("Horizontal Rotation Settings")]
	[Tooltip("If you want to limit horizontal turret rotation")]
	public bool HorizontalRotationLimit;

	[Tooltip("Right rotation limit")]
	[Range(0,180)]
	public float RightRotationLimit; 

	[Tooltip("Left rotation limit")]
	[Range(0,180)]
	public float LeftRotationLimit; 

	[Header("Vertical Rotation Settings")]
	[Tooltip("If you want to limit vertical turret rotation")]
	public bool VerticalRotationLimit;

	[Tooltip("Upwards rotation limit")]
	[Range(0,70)]
	public float UpwardsRotationLimit;

	[Tooltip("Downwards rotation limit")]
	[Range(0,70)]
	public float DownwardsRotationLimit;
	
	[Tooltip("Turning speed")]
	[Range(0,300)]
	public float TurnSpeed;

	[Header("Gun Settings")]

	[Tooltip("Click if you want to use pooling")]
	public bool UsePooling;

	[Tooltip("Gun firing rate")]
	public float FireRate;

	[Tooltip("Projectile traveling speed")]
	public float ProjectileSpeed;

	[Tooltip("How many projectile in this turret")]
	public float ProjectileCount;

	[Tooltip("Projectile prefabs")]
	public GameObject ProjectilePrefab;

	[Tooltip("Adjust the efficiency of this turret")]
	[Range(3f,4f)]
	public float Efficiency;

	[Tooltip("Barrel for instantiating projectile")]
	public Transform[] Barrel;

	[HideInInspector]
	public Transform target; // Target position

	[HideInInspector]
	public Vector3 predictedTargetPosition; // lead target position;
	[Header("Effects (Optional)")]
	[Tooltip("Shoot effect when firing the gun (optional)")]
	public GameObject ShootFX;
	public GameObject BulletShellFX;

	private Vector3 targetlastPosition; // Target last  position in last frame; 
	protected ParticleSystem bulletShellFX_PS;
	protected ParticleSystem shootFX_PS;	
	protected float nextFireAllowed;
	protected bool IsAiming = false; 

	protected virtual void Start()
	{	
		// Make sure the two pivots and barrel are not null
		target = null;
		if(HorizontalPivot == null || VerticalPivot == null)
		{
			Debug.Log("There is no pivot found, Please drag your pivots into this script");
			return;
		}
			
		if(Barrel.Length == 0)
		{
			Debug.Log("There is no Barrel found, Please drag your pivots into this script");
			return;
		}

		if(ProjectilePrefab == null)
		{
			Debug.Log("There is no projectile prefab found, Please drag your projectile prefab into this script");
			return;
		}

		if(UsePooling)
		{	
			if(PoolManager.instance == null)
			{
			  Debug.Log("PoolManager is missing, Please create a GameObject and add PoolManager.cs");
			  return;
			}
			else
				PoolManager.instance.CreatePool(ProjectilePrefab, 100);	
		}
			
		if(BulletShellFX != null)
		{
			BulletShellFX.SetActive(true);
			bulletShellFX_PS = BulletShellFX.GetComponent<ParticleSystem>();
			bulletShellFX_PS.Stop();
		} 
			

		if(ShootFX != null)
		{
			ShootFX.SetActive(true);
			shootFX_PS = ShootFX.GetComponent<ParticleSystem>();
			shootFX_PS.Stop();
		}
			
	}
	
	private void FixedUpdate()
	{	
		

		LeadTarget();
		HorizontalRotation();
		VerticalRotation();
		Fire();
	}

	private void LeadTarget()
	{	
		if(target == null) return;
		// Get target position in one second ahead 
		Vector3 targetSpeed = (target.position - targetlastPosition);
		targetSpeed /= Time.deltaTime; // Target distance in one second. Since "Time.deltaTime" = 1/FPS

		// ---------------------------------------------------------------------------------------------
		// Calculate the the lead target position based on target speed and projectileTravelTime to reach the target

		float distance = Vector3.Distance(transform.position, target.position);
		float projectileTravelTime = distance / Mathf.Max(ProjectileSpeed,2f);
		Vector3 aimPoint = target.position + targetSpeed * Efficiency/4 * projectileTravelTime;

		float distance2 = Vector3.Distance(transform.position, aimPoint);
		float projectileTravelTime2 = distance2 / Mathf.Max(ProjectileSpeed,2f);
		predictedTargetPosition = target.position + targetSpeed * Efficiency/4 * projectileTravelTime2;

		Debug.DrawLine(transform.position, predictedTargetPosition, Color.blue);

		targetlastPosition = target.position;
	}

	private void HorizontalRotation()
	{	
			if(HorizontalPivot == null && VerticalPivot == null || target == null) return;

			// Get target position from world space to local space from our  parent position (this.transfrom)
			Vector3 targetPositionInLocalSpace = this.transform.InverseTransformPoint(predictedTargetPosition);
			// Set "TargetPositionInLocalSpace" Y axis to zero, since this is horizontal rotation
			targetPositionInLocalSpace.y = 0f;
			
			// Store clamp value of the rotation
			Vector3 clamp = targetPositionInLocalSpace;
			// Clamp turret horizontal rotation according to its limit
			if(HorizontalRotationLimit)
			{
				if(targetPositionInLocalSpace.x >= 0f)
					clamp = Vector3.RotateTowards(Vector3.forward, targetPositionInLocalSpace, Mathf.Deg2Rad * RightRotationLimit, 0f);
				else
					clamp = Vector3.RotateTowards(Vector3.forward, targetPositionInLocalSpace, Mathf.Deg2Rad * LeftRotationLimit, 0f);	
			}
			else
			{
				RightRotationLimit = 0f;
				LeftRotationLimit = 0f;
			}

			// Rotate turret
			Quaternion whereToRotate = Quaternion.LookRotation(clamp);	
			HorizontalPivot.localRotation = Quaternion.RotateTowards(HorizontalPivot.localRotation, whereToRotate, TurnSpeed * Time.deltaTime);

			//Debug.DrawLine(HorizontalPivot.position, HorizontalPivot.position + HorizontalPivot.forward * 2000f, Color.yellow);

	}


	private void VerticalRotation()
	{	
		if(HorizontalPivot == null && VerticalPivot == null || target == null) return;

			// Get target position from world space to local space from horizontal pivot position
			Vector3 targetPositionInLocalSpace = HorizontalPivot.transform.InverseTransformPoint(predictedTargetPosition);

			// Set "TargetPositionInLocalSpace" X axis to zero, since this is vertical rotation
			targetPositionInLocalSpace.x = 0f;

			// Store clamp value of the rotation
			Vector3 clamp = targetPositionInLocalSpace;
			// Clamp turret vertical rotation according to its limit
			if(VerticalRotationLimit)
			{
				if(targetPositionInLocalSpace.y >= 0f)
					clamp = Vector3.RotateTowards(Vector3.forward, targetPositionInLocalSpace, Mathf.Deg2Rad * UpwardsRotationLimit, 0f);
				else
					clamp = Vector3.RotateTowards(Vector3.forward, targetPositionInLocalSpace, Mathf.Deg2Rad * DownwardsRotationLimit, 0f);
			}
			else
			{
				UpwardsRotationLimit = 0f;
				DownwardsRotationLimit = 0f;
			}
			

			// Rotate 
			Quaternion whereToRotate = Quaternion.LookRotation(clamp);			
			VerticalPivot.localRotation = Quaternion.RotateTowards(VerticalPivot.localRotation, whereToRotate, 2 * TurnSpeed * Time.deltaTime);

			//Debug.DrawLine(VerticalPivot.position, VerticalPivot.position + VerticalPivot.forward * 2000f, Color.black);

			// Check if target is out of turret rotation limit
			Vector3 dirTotarget = (predictedTargetPosition - VerticalPivot.position).normalized;
			float angle = Mathf.Abs(Vector3.Angle(VerticalPivot.forward, dirTotarget));
			if(angle < 5)
				IsAiming = true;
			else
				IsAiming = false;
			
	}

	// Set Target, this called from AntiMissileGunScanner
	public void SetTargetGun(Transform targetPosition)
	{
		this.target = targetPosition;
	}

	protected virtual void Fire()
	{			
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
			

			if(ShootFX == null) return;
			shootFX_PS.Play();
			
		}
	}

	void StopBulletShellEffect()
	{
		bulletShellFX_PS.Stop();
	}

	
}
