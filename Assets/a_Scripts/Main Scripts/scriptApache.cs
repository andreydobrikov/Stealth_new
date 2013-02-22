

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class scriptApache : MonoBehaviour 





{

    public Transform target;

    private bool dataSynch; // Switch which allows a pause for the health & any other data to be retrived from the LevelObjectArray[]

    private int health;

    private int armour; // Armour subtractor rating enables different endurance to heavier bullets

    private int life; // health+Armour

    private int damage;

    private int attackRadius;

    private Vector3 targetPosition;

    private float xAngle;

    private float yAngle;

    private float turnRate;

    private float reactionTimer;

    private float reloadTimer;

    public Transform bRotor;

    public Transform fRotor;

    private GameObject bullet;

    private enum mode { passive, alert, attack, destroyed };

    private mode state;

    private ScriptMissileSAM bulletScript;

    private GameObject fx;

    private FXManager sfx;

    private Camera playerCamera;

    private Vector3[] bulletXY; // Spawning offsets but also stores projectile's life in the Z

    private float turretZOffset;

    private GameObject targetID; // Handle For NGUI targetID sprites in  A_Null_TargetID - gets handle to list of sprites

    private float dist;

    private ScriptAInputManager inp;

    private ScriptASceneManager sceneManagerScript;

    private List<float> damageDelayList = new List<float>(); // Delay before bullet arrives from Raycast
    private List<int> damageList = new List<int>(); // Damage assigned from the Input script
    private List<Vector3> hitPositionList = new List<Vector3>(); // Vector assigned from the Input script
    private Vector3 bigExplosionPosition; // Set to when health is 0 or less - then we make a big explosion with Debris instead of standard

    public int pointsValue;

    private ScriptPointsManager pointsScript;

    //-------------------
    // AWAKE
    //-------------------
    void Awake()
    {

        pointsValue = 850;

        armour = 15; // subtracted from any hit damage values this enemy may receive


        state = mode.passive;

        attackRadius = 260;

        GameObject tempObject = GameObject.Find("GunView");
        target = tempObject.transform;

        tempObject = GameObject.Find("PlayerCamera");
        playerCamera = tempObject.GetComponent<Camera>(); //
        sfx = tempObject.GetComponent<FXManager>(); //

        //Find and get INPUT MANAGER SCRIPT - Totally necessary :)
        tempObject = GameObject.Find("A_GameWrapper");
        inp = tempObject.GetComponent<ScriptAInputManager>(); //

        tempObject = GameObject.Find("A_Scene_Manager");
        sceneManagerScript = tempObject.GetComponent<ScriptASceneManager>(); // Access to 'LEVEL DATA[]'  file necessary

        // NGUI LABELS POOL
        tempObject = GameObject.Find("A_Null_Labels");
        pointsScript = tempObject.GetComponent<ScriptPointsManager>(); 


        // Get Turret hinge

        for (int i = 0; i < transform.childCount; i++) { Transform c = transform.GetChild(i); if (c.name == "BladesFrontSpinning") { c.renderer.enabled = false; } }
        for (int i = 0; i < transform.childCount; i++) { Transform c = transform.GetChild(i); if (c.name == "BladesBackSpinning") { c.renderer.enabled = false; } }

        for (int i = 0; i < transform.childCount; i++) { Transform c = transform.GetChild(i); if (c.name == "BladesFrontStill") { fRotor = c; } }
        for (int i = 0; i < transform.childCount; i++) { Transform c = transform.GetChild(i); if (c.name == "BladesBackStill") { bRotor = c; } }
      

    }



    //-------------------
    // START... REMEMBER - A RE-USED POOL OBJECT CALLS START() EACH TIME - so any relevant individual values need setting here & not in Awake()
    //-------------------
    void Start()
    {
        dataSynch = false;

        state = mode.passive;
               
        bigExplosionPosition = Vector3.zero; //Final Death Big explosion trigger

        Get_Prefab_Pointers();

        turnRate = Random.Range(40f, 60f);

        fRotor.Rotate(Vector3.up * (int)Random.Range(0f, 355f));
      
        //state = mode.attack;
       

    }
   


    //-------------------
    // PREFAB FINDER
    //-------------------
    private GameObject projectile;

    void Get_Prefab_Pointers()
    {
        //GameObject tempObject;
        GameObject tempObject = GameObject.Find("A_Scene_Manager");
        ScriptASceneManager pre;

        pre = tempObject.GetComponent<ScriptASceneManager>();

       



    }





    //-------------------
    // UPDATE
    //-------------------
    private int i;
    void Update()
    {

      

                Do_Passive_Mode();
              

        


    }



   


    //-------------------
    // PASSIVE
    //-------------------


    public int judderStage; // Called and set from inputs script when large amount of damage done

    public void Do_Helicopter_Judder()
    {

        switch (judderStage)
        {
            case 0:

                break;

            case 1:

                break;

            case 2:

                break;

            case 3:

                break;

            case 4:

                break;

        }



    }









    //-------------------
    // PASSIVE
    //-------------------
    void Do_Passive_Mode()
    {

       

        //if (dist < attackRadius)
       // {

           // state = mode.alert;

           // reactionTimer = Time.time + Random.Range(0.5f, 5f); // delay before opening fire

        //}
        //else
        //{

        fRotor.Rotate(Vector3.up * 180f * Time.deltaTime);
        bRotor.Rotate(Vector3.right * 180f * Time.deltaTime);
          

        //}

    }






    // Remember All active objects get rooted to the nullActivePool object for easy deactivation
    // with one call for menu or pause - here we attach it to a new parent to align it with the barrel 
    // or turret of the gameobject that is firing it - then we must remember to reparent it back to its 
    // Null Root Parent or else it will not pause with one call eg: GameObject.SetActive(A_Null_Active_Pool)

  



















}

