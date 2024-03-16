using UnityEngine;
using UnityEngine.AI;
public class Plane2 : MonoBehaviour {
	public float Speed;
	public float Health;
	public float PlaneLifeTime;
	public float[] RandomTurnRate;
	public float LerpSpeed;
	public GameObject DamageFX;
	public GameObject ExplosionFX;

	private Rigidbody _rb;
	private bool _isDead;
	
	
	// Use this for initialization
	void Start () {
		_rb = GetComponent<Rigidbody>();
		DamageFX.SetActive(false);
		ExplosionFX.SetActive(false);
		Invoke("Destroy", PlaneLifeTime);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if(_isDead){
			Speed = Mathf.Lerp(Speed, 0, LerpSpeed * Time.fixedDeltaTime);
		}

		transform.Translate(new Vector3(0,0,1) * Speed * Time.fixedDeltaTime);
	}

	void OnCollisionEnter(Collision collision)
	{	
		//Debug.Log(LayerMask.LayerToName(collision.gameObject.layer));
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

	void Destroy()
	{			
		DamageFX.transform.SetParent(null);
		ExplosionFX.SetActive(true);
		ExplosionFX.transform.SetParent(null);
		Destroy(gameObject);
		Destroy(DamageFX);
	}
}
