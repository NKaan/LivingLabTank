using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour {

	Dictionary<int,Queue<ObjectInstance>> poolDictionary = new Dictionary<int, Queue<ObjectInstance>> (); // Dictionary to store queue of objects

	private static PoolManager _instance;

	public static PoolManager instance {
		get {
			if (_instance == null) {
				_instance = FindObjectOfType<PoolManager>();
			}
			return _instance;
		}
	}

	public void CreatePool(GameObject prefab, int poolSize)
	{
		int poolKey = prefab.GetInstanceID(); // use instance id for poolKey in dictionary

		if(!poolDictionary.ContainsKey(poolKey))
		{
			poolDictionary.Add(poolKey, new Queue<ObjectInstance>()); // create queue in dictionary
			GameObject poolHolder = new GameObject(prefab.name + "pool"); // create new game object as parent for "pooling gameobject" 
			poolHolder.transform.parent = transform;

			for(int i = 0; i < poolSize; i++)
			{
				ObjectInstance newObject = new ObjectInstance(Instantiate(prefab) as GameObject); // instantiate "pooling gameobject" 
				poolDictionary[poolKey].Enqueue(newObject); // Add "pooling gameobject"  in queue 
				newObject.SetParent(poolHolder.transform);	// put "pooling gameobject" in poolHolder
			}
		}
	}

	// Reuse game "pooling gameobject" 

	public void ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation)
	{
		int poolKey = prefab.GetInstanceID();

		if(poolDictionary.ContainsKey(poolKey))
		{
			ObjectInstance objecToReuse = poolDictionary[poolKey].Dequeue(); // get "pooling gameobject" 
			poolDictionary[poolKey].Enqueue(objecToReuse); // Add "pooling gameobject" in queue to restore it back 

			objecToReuse.Reuse(position, rotation);
		}
	}

	// This function is for "AntiMissileProjectile"
	public void ReuseObject(GameObject prefab, Vector3 position, Quaternion rotation, Vector3 target, float speed)
	{
		int poolKey = prefab.GetInstanceID();

		if(poolDictionary.ContainsKey(poolKey))
		{
			ObjectInstance objecToReuse = poolDictionary[poolKey].Dequeue();
			poolDictionary[poolKey].Enqueue(objecToReuse);

			objecToReuse.Reuse(position, rotation, target, speed);
		}
	}

	public class ObjectInstance 
	{
		GameObject gameobject;
		Transform transform;

		bool hasPoolObjectComponent;
		PoolObject poolObjectScript;

		public ObjectInstance(GameObject objectInstance)
		{
			gameobject = objectInstance;
			transform = gameobject.transform;
			gameobject.SetActive(false); // set "pooling gameobject" to not active

			if(gameobject.GetComponent<PoolObject>()) // check if "pooling gameobject" has PoolObject component
			{
				hasPoolObjectComponent = true;
				poolObjectScript = gameobject.GetComponent<PoolObject>(); // get PoolObject component
				poolObjectScript.IsPooling = true; 
			}
		}

		public void Reuse(Vector3 position, Quaternion rotation)
		{
			gameobject.SetActive(true); // set "pooling gameobject" to active
			transform.position = position;
			transform.rotation = rotation;

			if(hasPoolObjectComponent)
			{
				poolObjectScript.OnobjectReuse(); // set specific  settings for "pooling gameobject"
			}
		}

		// This function is for "AntiMissileProjectile"
		public void Reuse(Vector3 position, Quaternion rotation, Vector3 target, float speed)
		{
			gameobject.SetActive(true);
			transform.position = position;
			transform.rotation = rotation;

			if(hasPoolObjectComponent)
			{
				poolObjectScript.OnobjectReuse(target, speed);
			}
		}

		public void SetParent(Transform parent)
		{
			transform.parent = parent;
		}


	}
}
