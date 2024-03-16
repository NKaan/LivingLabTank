using UnityEngine;
using UnityEngine.AI;

public class EnemySoldier : MonoBehaviour {
	public GameObject Parachute;
	public NavMeshAgent Agent;
	public float SoldierLifeTime;
	public Transform Anchor;

	private Transform[] _waypoints;
	private Vector3 _targetPoint;
	private Rigidbody[] _bodyPartsRB;
	private Collider[] _bodyPartsCollder;
	private Animator _animator;
	private bool _isdead = false;
	private int _animatorRunHash = Animator.StringToHash("Run");

	void Awake()
	{	
		if(Anchor)
		{	
			_bodyPartsRB = Anchor.GetComponentsInChildren<Rigidbody>();
			_bodyPartsCollder = Anchor.GetComponentsInChildren<Collider>();
		}
		
		_animator = GetComponent<Animator>();
	}

	void Start()
	{	
		if(Anchor)
		{	
			for(int i = 0; i < _bodyPartsRB.Length; i ++)
			{
				_bodyPartsRB[i].isKinematic = true;
				_bodyPartsCollder[i].enabled = false;
			}		
		}

		_waypoints = Manager.GetInstance().Waypoints1;
		//Invoke("Dead", 1);
		Invoke("Destroy", SoldierLifeTime);
	}

	void Update()
	{	
		if(Agent.enabled == true && Agent.speed > 0.1)
			_animator.SetBool(_animatorRunHash, true);	
		else if(Agent.speed < 0.1)
			_animator.SetBool(_animatorRunHash, false);
	
		// Generate a new Quaternion representing the rotation we should have
		Quaternion newRot = Quaternion.LookRotation (Agent.desiredVelocity);
		// Smoothly rotate to that new rotation over time
		transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * 5.0f);
		
		if(Agent.enabled == true)
		{	
			
			if(Vector3.Distance(transform.position, _targetPoint) < Agent.stoppingDistance)
			{
				RandomPoint(_waypoints[Random.Range(0, _waypoints.Length)].position, 2f, out _targetPoint);
			}

			if (Agent.isPathStale || 
			!Agent.hasPath   ||
			Agent.pathStatus!=NavMeshPathStatus.PathComplete) 
			{
				RandomPoint(_waypoints[Random.Range(0, _waypoints.Length)].position, 2f, out _targetPoint);
			}
		}
	}

	
	void OnCollisionEnter(Collision collision)
	{	
		if(_isdead) return;

		if(LayerMask.LayerToName(collision.gameObject.layer) == "Projectile")
		{	
			Dead();
		}

		//Debug.Log(LayerMask.LayerToName(collision.gameObject.layer));
		if(LayerMask.LayerToName(collision.gameObject.layer) == "Ground")
		{
			Parachute.SetActive(false);
			Agent.enabled = true;
			_animator.applyRootMotion = true;
			NavAgentControl(true, false);
			RandomPoint(_waypoints[Random.Range(0, _waypoints.Length)].position, 2f, out _targetPoint);
		}
	}

	

	//calculate random point for movement on navigation mesh
    private void RandomPoint(Vector3 center, float range, out Vector3 result)
	{
		//clear previous target point
		result = Vector3.zero;
		
		//try to find a valid point on the navmesh with an upper limit (10 times)
		for (int i = 0; i < 10; i++)
		{
			//find a point in the movement radius
			Vector3 randomPoint = center + (Vector3)Random.insideUnitCircle * range;
			randomPoint.y = 0;
			NavMeshHit hit;

			//if the point found is a valid target point, set it and continue
			if (NavMesh.SamplePosition(randomPoint, out hit, 2f, NavMesh.AllAreas)) 
			{
				result = hit.position;
				break;
			}
		}
		
		//set the target point as the new destination
		Agent.SetDestination(result);
	}

	public void NavAgentControl( bool positionUpdate, bool rotationUpdate )
	{
		if (Agent)
		{
			Agent.updatePosition = positionUpdate;
			Agent.updateRotation = rotationUpdate;
		}
	}

	void Dead()
	{
		_isdead = true;
		if(Parachute.activeInHierarchy)
			Parachute.SetActive(false);
		if(Anchor)
			Anchor.transform.SetParent(null);
		if(Agent.enabled == true)
			Agent.isStopped = true;
		
		_animator.enabled = false;
		if(Anchor)
		{
			for(int i = 0; i < _bodyPartsRB.Length; i ++)
			{
				_bodyPartsRB[i].isKinematic = false;
				_bodyPartsCollder[i].enabled = true;
			}
		}

		Destroy();
			
			
	}

	void Destroy()
	{
		Destroy(gameObject);
	}

	

}
