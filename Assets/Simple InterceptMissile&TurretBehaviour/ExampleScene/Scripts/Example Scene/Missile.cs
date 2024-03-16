using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {
	[Header("General Parameter")]

	[Tooltip(" Missile traveling speed")]
	public float MissileSpeed = 0f;

	[Tooltip("Missile acceleration during missile motor is active")]
	public float Acceleration = 20f;

	[Tooltip("Time for missile automatically explode")]
	public float MissileLifeTime = 20f;

	[Tooltip("Time delay before activate the missile")]
	public float LaunchDelayTime = 3f;

	[Tooltip("Time delay before start to guide(tracking target) missile")]
	public float TrackingDelay = 5f;

	[Tooltip("Initial force before activate the missile")]
	public float InitialLaunchForce = 15f;

	[Tooltip("Motor life time before it stops accelerating")]
	public float MotorLifeTime = 15f;

	[Tooltip("Missile turn rate towards target")]
	public float TurnRate = 90;

	[Tooltip("Missile Explsotion GameObject")]
	public GameObject MissileExplosion;

	[Tooltip("Missile Flame trail")]
	public GameObject MissileFlameTrail;

	[Tooltip("Missile launch Sound effect")]
	public AudioSource LaunchSFX;

	
	[HideInInspector]
	public Transform Target; // Missile's target transform;
	

	private  bool targetTracking = false; // Bool to check whether the missile can track the target;
	private bool missileActive = false; // Bool  to check if missile is active or not;
	private bool motorActive = false; // Bool  to check if motor is active or not;
	private bool explosionActive = false; // Bool to activate the explosive;
	private float MissileLaunchTime; // Get missile launch time;
	private float MotorActiveTime; // Get missile Motor active time;
	private Quaternion guideRotation; // Store rotation to guide the missile;
	private Rigidbody rb;
	
	private void Start() 
	{
	  rb = GetComponent<Rigidbody>();
	  MissileLaunchTime = Time.time;
	  StartCoroutine(LaunchDelay(LaunchDelayTime));
	}

	private void ActivateMissile()
	{	
		missileActive = true;

		motorActive = true;
		MotorActiveTime = Time.time;

		MissileFlameTrail.SetActive(true);
		LaunchSFX.Play();
	}

	private void FixedUpdate()
	{	
		// Running missile
		Run();
		// Guide missile
		GuideMissile();
	}

	private void OnCollisionEnter(Collision col)
	{
		if(!explosionActive) return;
		MissileFlameTrail.transform.parent = null;
		Destroy(MissileFlameTrail, 5f);
		DestroyMissile();
	}

	private void Run()
	{	
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

	// Guide missile towards target
	private void GuideMissile()
	{	
		if(Target == null) return;
		
		Vector3 relativePosition = Target.position - transform.position; // Get current relaPosition towards target;
		

		if(targetTracking)
		{
			relativePosition = Target.position - transform.position; 
			guideRotation = Quaternion.LookRotation(relativePosition, transform.up); 
		}
	}

	IEnumerator LaunchDelay(float time)
	{	
		// Put initial speed to missile before activate 
		rb.velocity = transform.forward * InitialLaunchForce;
		yield return new WaitForSeconds(time); // Time delay before activate 
		ActivateMissile();
		yield return new WaitForSeconds(Random.Range(TrackingDelay, TrackingDelay + 3f));
		targetTracking = true; // Set targetTracking true to start guide the missile;
		explosionActive = true; // Set explostion to active
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
		Instantiate(MissileExplosion, transform.position, transform.rotation);
	}
	
}
