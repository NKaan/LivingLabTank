using UnityEngine;
using UnityEngine.AI;
public class Tank : MonoBehaviour {
	public float Speed;
	public float Health;
	public float TankLifeTime;
	public float[] RandomTurnRate;
	public float LerpSpeed;
	public GameObject DamageFX;
	public GameObject ExplosionFX;

	private Rigidbody _rb;
	private bool _isDead;
	private NavMeshAgent _agent;
	private Transform[] _waypoints;
	private Vector3 _targetPoint;

	// Use this for initialization
	void Start () {

		_rb = GetComponent<Rigidbody>();

		transform.Rotate(0f,Random.Range(RandomTurnRate[0],RandomTurnRate[1]),0f);

		DamageFX.SetActive(false);
		
		ExplosionFX.SetActive(false);
		
		_agent = GetComponent<NavMeshAgent>();
		
		_waypoints = Manager.GetInstance().Waypoints1;

		RandomPoint(_waypoints[Random.Range(0, _waypoints.Length)].position, 2f, out _targetPoint);

		NavAgentControl(true, false);
		
		Invoke("Destroy", TankLifeTime);

	}
	
	// Update is called once per frame
	void Update () {

		if(_isDead){
			Speed = Mathf.Lerp(Speed, 0, LerpSpeed * Time.fixedDeltaTime);
		}	

		// Generate a new Quaternion representing the rotation we should have
		Quaternion newRot = Quaternion.LookRotation (_agent.desiredVelocity);
		// Smoothly rotate to that new rotation over time
		transform.rotation = Quaternion.Slerp(transform.rotation, newRot, Time.deltaTime * 5.0f);

		if(Vector3.Distance(transform.position, _targetPoint) < _agent.stoppingDistance)
		{
			RandomPoint(_waypoints[Random.Range(0, _waypoints.Length)].position, 2f, out _targetPoint);
		}

		if (_agent.isPathStale || 
			!_agent.hasPath   ||
			_agent.pathStatus!=NavMeshPathStatus.PathComplete) 
		{
			RandomPoint(_waypoints[Random.Range(0, _waypoints.Length)].position, 2f, out _targetPoint);
		}

	}

	void OnCollisionEnter(Collision collision)
	{	
		if(LayerMask.LayerToName(collision.gameObject.layer) != "Projectile") return;
	
		if(_isDead) return;
		
		Health --;
		if(Health <= 0)
		{	
			_isDead = true;
			DamageFX.SetActive(true);
			_rb.useGravity = true;
			Invoke("Destroy", 10.0f);		
		}
	}

	public void NavAgentControl( bool positionUpdate, bool rotationUpdate )
	{
		if (_agent)
		{
			_agent.updatePosition = positionUpdate;
			_agent.updateRotation = rotationUpdate;
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
		_agent.SetDestination(result);
	}

	void Destroy()
	{			
		if(!DamageFX.activeInHierarchy)
			DamageFX.SetActive(true);
		DamageFX.transform.SetParent(null);
		ExplosionFX.SetActive(true);
		ExplosionFX.transform.SetParent(null);
		Destroy(gameObject);
	}
}
