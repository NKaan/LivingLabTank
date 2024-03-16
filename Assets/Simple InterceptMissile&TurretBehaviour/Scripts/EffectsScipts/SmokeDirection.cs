using System.Collections;
using UnityEngine;

public class SmokeDirection : MonoBehaviour {

	
	[Range(0.0f, 90.0f)]
	public float SmokeJitteringSpeed = 90f; // Speed at which the jitter's angle moves towards a new angle
	[Range(0.0f, 90.0f)]
	public float JitterMaximumAngle = 5.0f;  // Smoke Jittering Maximum angle
	[Range(0.0f, 30.0f)]
	public float RefreshRate = 15f; // Often rate to create jittering smoke
	private Vector3 startingEulers; // Get the initial value of local smoke rotation
	private Vector3 newSmokeRotation; // Store new smoke rotation
	private Vector3 smokeRotation = Vector3.zero;
	private void Start()
	{
		startingEulers = transform.localEulerAngles;
		StartCoroutine(NewRotationTarget());
	}

	private void Update()
	{	
		// Set the smoke rotation to new rotation value
		smokeRotation = Vector3.MoveTowards(smokeRotation, newSmokeRotation, SmokeJitteringSpeed * Time.deltaTime);
		transform.eulerAngles = startingEulers + smokeRotation;
	}

	private IEnumerator NewRotationTarget()
	{	
		// Get new smoke rottation value per RefreshRate;
		while(true)
		{
			newSmokeRotation.x = Random.Range(-JitterMaximumAngle, JitterMaximumAngle);
			newSmokeRotation.y = Random.Range(-JitterMaximumAngle, JitterMaximumAngle);
			newSmokeRotation.z = Random.Range(-JitterMaximumAngle, JitterMaximumAngle);

			if(SmokeJitteringSpeed <= 0)
				SmokeJitteringSpeed = 1f;

			yield return new WaitForSeconds(1.0f / RefreshRate);
		}
		
	}
}
