using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptDebris : MonoBehaviour {

	// Use this for initialization

    private List <float> turnRateList = new List<float>();
     
    private float startYPosition;
    private int childCount;
    private float life;
    
    //private List<GameObject> fxGlowList = new List<GameObject>();
    //private FXManager sfx;
    //private GameObject fx;


    void Awake()
    {


        //GameObject tempObject = GameObject.Find("PlayerCamera");
        //Camera playerCamera = tempObject.GetComponent<Camera>(); //
        //sfx = tempObject.GetComponent<FXManager>(); //

        //foreach (Transform child in transform)
       // {
        //}
    }


	
    
    void Start () 
    {

        childCount = 0;
        startYPosition = 0;
        life = Time.time + 5.0f;

        foreach (Transform child in transform)
        {
            child.renderer.enabled = true;
            child.eulerAngles = new Vector3((int)Random.Range(15f, 90f), (int)Random.Range(0f, 355), 0f);
            child.rigidbody.AddRelativeForce(Vector3.forward * Random.Range(800f, 950f));
            child.rigidbody.AddForce(Vector3.up * Random.Range(950f, 1200f));
            turnRateList.Add(Random.Range(150f, 600f));
            float sc = Random.Range(0.75f, 2.0f);
            child.transform.localScale = new Vector3(sc, sc, sc);
            
            childCount++;

        }

	}
	
	// Update is called once per frame
    int i;
    Vector3 scale;
    void Update()
    {

        if (startYPosition == 0)
        {
            startYPosition = (transform.position.y - 10f);
        }




        if (Time.time < life)
        {

            if (childCount > 0)
            {

                i = 0;
                foreach (Transform child in transform)
                {
                    child.transform.Rotate(Vector3.up, turnRateList[i] * Time.deltaTime);
                    child.transform.Rotate(Vector3.forward, turnRateList[i] * Time.deltaTime);

                    if (child.transform.position.y < startYPosition)
                    {
                        if (turnRateList[i] > 0f)
                        {
                            Destroy_Debris(child);
                            turnRateList[i] = 0f;
                        }
                    }

                    i++;
                }
            }
            else
            {
                ResetDebris();
            }
        }
        else
        {
            ResetDebris();
        }

    }



     void Destroy_Debris(Transform t)
     {

         t.renderer.enabled = false;
         t.rigidbody.velocity = Vector3.zero;
         t.transform.rotation = Quaternion.identity;

         childCount--;

     }




     void ResetDebris()
     {

         foreach (Transform t in transform)
         {
             t.transform.localPosition = Vector3.zero;
             t.rigidbody.velocity = Vector3.zero;
             t.transform.rotation = Quaternion.identity;
         }

         ObjectPoolManager.DestroyPooled(this.gameObject); // this ONLY de-activates the object and places back in the POOL LIBRARY

         turnRateList = null;
         turnRateList = new List<float>();
     }




   
  





}
