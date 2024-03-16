using Complete;
using Mirror;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InterceptMissile : NetworkBehaviour 
{

	[Header("General Paramaters")]
	[Tooltip(" Missile traveling speed")]
	public float MissileSpeed = 0f;

	[Tooltip("Initial force before activate the missile")]
	public float InitialLaunchForce;

	[Tooltip("Missile acceleration during missile motor is active")]
	public float Acceleration = 20f;

	[Tooltip("Motor life time before it stops accelerating")]
	public float MotorLifeTime;

	[Tooltip("Time for missile automatically explode")]
	public float MissileLifeTime;

	[Tooltip("Missile turn rate towards target")]
	public float TurnRate = 90;

	[Tooltip("Missile range for guidance towards target")]
	public float MissileViewRange;

	[Tooltip("Missile view angle in degree for guidance towards target")]
	[Range(0,360)]
	public float MissileViewAngle;

	[Tooltip(" Set explosion active delay")]
	public bool isExplosionActiveDelay;

	[Tooltip("Set tracking delay")]
	public bool isTrackingDelay;

	[Tooltip("Missile Flame trail")]
	public GameObject MissileFlameTrail;

	[Tooltip("Missile Explsotion GameObject")]
	public GameObject MissileExplosion;

	[Tooltip("Missile launch Sound effect")]
	public AudioSource LaunchSFX;

    private bool targetTracking = false; // Bool to check whether the missile can track the target;
	private bool missileActive = false; // Bool  to check if missile is active or not;
	private float MissileLaunchTime; // Get missile launch time;
	private bool motorActive = false; // Bool  to check if motor is active or not;
	private float MotorActiveTime; // Get missile Motor active time;
	private Quaternion guideRotation; // Store rotation to guide the missile;
	private Rigidbody rb;
	private bool isLaunch;
	private Transform target;
	private Vector3 targetlastPosition; // Target last  position in last frame;
	private bool explosionActive = false; // Bool to activate the explosive; 
	private CapsuleCollider myCollider;

	[SyncVar]
	public uint killerPlayerID;

    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    private void Awake()
	{
		rb = GetComponent<Rigidbody>();
		myCollider = GetComponent<CapsuleCollider>();
		myCollider.enabled = false;

    }

    private void Start()
	{
        if (!isLaunch)
			rb.isKinematic = true;
	}

	private void FixedUpdate()
	{
		if (!isServer)
			return;

		Run();

		if(target == null) return;

		GuideMissile();
	}

	private void OnCollisionEnter(Collision col)
	{	
		if(!explosionActive) return;
		// Detach rocket flame 
		//MissileFlameTrail.transform.parent = null;
		//Destroy(MissileFlameTrail, 5f);
		// Destroy this missile

		if(col.gameObject.tag == "Player")
		{
            TankHealth targetHealth = col.gameObject.GetComponent<TankHealth>();
			targetHealth.TakeDamage(50, killerPlayerID);
        }

		DestroyMissile();
	}

	// This Launch called from "InterceptMissileControlelr"
	public void Launch(Transform target)
	{	
		this.target = target;
		RpcRun();
        transform.parent = null;
        isLaunch = true;
		rb.isKinematic = false;
		MissileLaunchTime = Time.time;

		if(isExplosionActiveDelay)
			StartCoroutine(ExplosionDelay());
		else
			explosionActive = true;
		
		if(isTrackingDelay)
			StartCoroutine(TrackingDelay());
		else
			targetTracking = true;
		// missile activation delay
		StartCoroutine(ActiveDelay(1));
	}

	[ClientRpc]
	public void RpcRun()
	{
        transform.parent = null;
    }

	private void Run()
	{
		if(!isLaunch) return;

		if(!missileActive) return;

		// Check if missile motor is still active ?
		if(Since(MotorActiveTime) > MotorLifeTime)
			motorActive = false; // if motor exceed the "MotorActiveTime" duration : motor will be stopped
		else 
			motorActive = true;  // if not : motor continuing running

		// if missile active move it
		if(!missileActive)  return;

			// Keep missile accelerating when motor is still active
			if(motorActive)
			MissileSpeed += Acceleration * Time.deltaTime;
			
			rb.velocity = transform.forward * MissileSpeed;

			// Rotate missile towards target according to "guideRotation" value
			if(targetTracking)
				transform.rotation = Quaternion.RotateTowards(transform.rotation, guideRotation, TurnRate * Time.deltaTime);
			
			if(Since(MissileLaunchTime) > MissileLifeTime) // Destroy Missile if it more than live time
				DestroyMissile();

	}

	private void GuideMissile()
	{
		
		Vector3 relativePosition = target.position - transform.position; // Get current relaPosition towards target;
		float angleToTarget = Mathf.Abs(Vector3.Angle(transform.position.normalized, relativePosition.normalized));
		float distance = Vector3.Distance(target.position, transform.position);
		
		// target is out of missile's view angle or target distance out of missile's view range
		if(angleToTarget > MissileViewAngle || distance > MissileViewRange)
			targetTracking = false;
		
		if(!targetTracking) return;

			// Get target position in one second ahead 
			Vector3 targetSpeed = (target.position - targetlastPosition);
			targetSpeed /= Time.deltaTime; // Target distance in one second. Since "Time.deltaTime" = 1/FPS

			// ---------------------------------------------------------------------------------------------
			// Calculate the the lead target position based on target speed and projectileTravelTime to reach the target

			// Get time to hit based on distance
			float MissilespeedPrediction = MissileSpeed + Acceleration * Since(MissileLaunchTime);
			float MissileTravelTime = distance / MissilespeedPrediction;

			// Lead Position based on target position prediction within impact time
			Vector3 targetFuturePosition = target.position + targetSpeed * MissileTravelTime;
			Vector3 aimPosition = targetFuturePosition - transform.position;

			// During Rotation get target 90% in "MissileViewAngle" sinse positionToGo will likely out of "MissileViewAngle"
			relativePosition = Vector3.RotateTowards(relativePosition.normalized, aimPosition.normalized, MissileViewAngle * Mathf.Deg2Rad * 0.9f, 0f);
			guideRotation = Quaternion.LookRotation(relativePosition, transform.up);

			targetlastPosition = target.position;
		
	}

	IEnumerator ExplosionDelay()
	{
		yield return new WaitForSeconds(2);
		explosionActive = true;
		myCollider.enabled = true;

    }

	IEnumerator TrackingDelay()
	{
		yield return new WaitForSeconds(2);
		targetTracking = true;
	}

	IEnumerator ActiveDelay(float time)
	{	
		// Put initial speed to missile before activate 
		rb.velocity = transform.forward * InitialLaunchForce;
		yield return new WaitForSeconds(time);
		ActivateMissile();
	}

	// Activate missile
	private void ActivateMissile()
	{
		missileActive = true;
		motorActive = true;
		MotorActiveTime = Time.time;
		MissileFlameTrail.SetActive(true);
		LaunchSFX.Play();
	}

	// Get the "Since" time from the input/parameter value
	private float Since(float Since)
	{
		return Time.time - Since;
	}

	// Destroy Missile
	private void DestroyMissile()
	{
		Destroy(gameObject);
		RpcDestroyMissile();

    }

	[ClientRpc]
    private void RpcDestroyMissile()
    {
        Instantiate(MissileExplosion, transform.position, transform.rotation);
    }




}
