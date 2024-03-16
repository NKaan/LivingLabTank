using UnityEngine;

public class Manager : MonoBehaviour {
	private static Manager instance;
	public Transform[] Waypoints1;
	
	public static Manager GetInstance()
	{
		return instance;
	}
	void Awake()
	{
		instance = this;
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
