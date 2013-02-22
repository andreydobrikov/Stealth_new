using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class ScriptApachePath : MonoBehaviour 

    {

    public Transform rotor;
    private GameObject tempObject; // Just a temporary handle for gameobjects
    private ScriptAInputManager inp; // Handle for accessing the 'Controls input Script attached to the GameWrapper Object.
    private FXManager sfx;

    //public List<Transform> bulletsList = new List<Transform>(); // List of all newly created bullets
    void Awake()
    {
       // transform.eulerAngles = new Vector3(0, 180f, 0);
    }

    // Use this for initialization
    void Start()
    {
      

        // FIND ROTOR - child of this mesh
        rotor = transform.Find("BladesFrontSpinning");



        //Find and get FX Manager - For sounds and any other custom visual FX
        tempObject = GameObject.Find("PlayerCamera");
        sfx = tempObject.GetComponent<FXManager>(); //


        // ITWEEN - quickly gets the gunner view moving along a pre-defined spline
        iTween.MoveTo(gameObject, iTween.Hash("Path", iTweenPath.GetPath("PlayerPath"), "Time", 200, "EaseType", iTween.EaseType.linear, "orienttopath", true, "looktime", 0.2f, "lookahead", 0.01f, "movetopath", false));


        // TEMPORARY SFX STARTED HERE
        sfx.Play_Looping_Sound(0); // XCheck in Editor for ID numbers - for now Zero is the Rotors sound



    }


    // Update is called once per frame
    void Update()
    {
        // Simple turn of Helicopter Rotor
        rotor.Rotate(0, Time.deltaTime * 600f, 0);

    }
}




   





    






