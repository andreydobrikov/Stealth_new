using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptPrefabList : MonoBehaviour
{

    public List<GameObject> prefabList = new List<GameObject>(); //LIBRARY of all Pool Prefabs - dragged to in Editor



    // Use this for initialization
    void Awake()
    {

       // foreach (GameObject p in prefabList)
       // {
           // if (p) { p.SetActive(false); }
       // }
    }



    // Use this for initialization
    void Start()
    {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
