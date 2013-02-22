using UnityEngine;
using System.Collections;

public class scriptSkybox : MonoBehaviour {



    private Vector3 pos;

    // SkyBox original size is 1700 meters
    // So we need to scale this value according to
    // the clipping value set in the Camera Code so it 
    // doesn't get clipped

	// Use this for initialization
	void Start () {
	
	}
	
	
    // Update is called once per frame
    void LateUpdate()
    {


        transform.rotation = Quaternion.identity;
        pos = transform.position;

        pos.y = 50f;
        transform.position = pos;

    }
}
