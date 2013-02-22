using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// The ObjectPool is the storage class for pooled objects of the same kind (e.g. "Pistol Bullet", or "Enemy A")
// This is used by the ObjectPoolManager and is not meant to be used separately
public class ObjectPool : MonoBehaviour
{

	private Transform nullPoolParent; // SHOULD BE CALLED FROM INPUTS TO DEACTIVATE OBJECTS ON 'PAUSE'
 

	void Awake()
	{

        GameObject tempObject = GameObject.Find("A_Null_Active_Pool");
		nullPoolParent = tempObject.transform;// SHOULD BE CALLED FROM INPUTS TO DEACTIVATE OBJECTS ON 'PAUSE' or clear for new level

		pool = new Queue<GameObject> ();
	}



	// The type of object this pool is handling
	GameObject prefab;
	private int ArrayID;
	public GameObject Prefab
	{
		get { return prefab; }
		set { prefab = value; }
	}

	// This stores the cached objects waiting to be reactivated
	Queue<GameObject> pool;	

	// How many objects are currently sitting in the cache
	public int Count
	{
		get { return pool.Count; }
	}

	


	public GameObject Instanciate(Vector3 position, Quaternion rotation, int tempID)
	{
		GameObject obj;
		ArrayID = tempID;
		// if we don't have any object already in the cache, create a new one
		if( pool.Count < 1 )
		{
			obj = Object.Instantiate( prefab, position, rotation ) as GameObject;

			//Debug.Log("MAKING AN OBJECT: " + prefab.name);

            obj.SetActive(true);
            obj.transform.parent = null;
            obj.transform.position = position;
            obj.transform.rotation = rotation;

		}
		else // else pull one from the cache
		{
            //Debug.Log("USING FREE POOL OBJECT: " + prefab.name + " POOL COUNT: " + pool.Count);

			obj = pool.Dequeue();

            obj.SetActive(true);

			// reactivate the object
			obj.transform.parent = null;
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			//if(obj.renderer) { obj.renderer.enabled = true; }
			
			

			// Call Start again
			obj.SendMessage( "Start", SendMessageOptions.DontRequireReceiver );

			

		}

		obj.transform.parent = nullPoolParent;// WILL BE CALLED FROM INPUTS TO DEACTIVATE OBJECTS ON 'PAUSE'

		return obj;
	}

	// put the object in the cache and deactivate it
	public void Recycle( GameObject obj )
	{
		// deactivate the object
		//obj.active = false;
		obj.SetActive (false);// ADDED children code - nick



		// put the recycled object in this ObjectPool's bucket
		obj.transform.parent = this.gameObject.transform;

		// put object back in cache for reuse later
		pool.Enqueue( obj );
	}
}
