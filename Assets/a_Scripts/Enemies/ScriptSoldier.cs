using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class ScriptSoldier : MonoBehaviour

{
    // ANIM FRAMES & BEHAVIOUR MODES
    private enum mode { passive, attack, running, retaliate, dying, destroyed };
    private mode state;
    private Transform[] FRAME_ARRAY; // Array List  for 'Frames'..actually separate meshes here

    private bool dataSynch; // Switch which allows a pause for the health & any other data to be retrived from the LevelObjectArray[]

    private int health;

    private int armour; // Armour subtractor rating enables different endurance to heavier bullets

    private int damage;

    private float hitValue; // Value of damage this enemy can do



    // GENERAL ANIMATION
    private float turnRate;
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float animTime;
    private float animSpeed;
    private float startTime;
    private float endTime;
    private int startFrame;
    private int endFrame;
    private int currentFrame;
    private float muzzleTime;
    private float attackRadius;
    private float reloadTimer;
    private float targetDistance;
    private float runSpeed;
    private int temp;

    private Transform target;
    private Transform gunView;
    // RAY TESTS- for movement
    private int targetAngle;
    private int runTargetAngle;
    private Vector3 rayDirection;
    private Vector3 rayStartPosition;
    private int rayLength ;
    private int downStartDistance;
    private float downRayLength;
    private float rayTime;
    private bool leftOrRight;
    private bool newDirection;
    private bool hit;
    private  RaycastHit hitobject;
    private float gravityRayLength;
    private float gravity;

    //TRACK A TARGET - in attack mode
    private float ly;
    private float lz;
    private Vector3 targetVector;
    private Vector3 targetDirection;
    private float directionTime;
    private float distanceTime; //timer for target distance checks
    private float distanceStep; // time gap

    // LOCKED ON TARGET DIRECTION
    private bool locked;
    private bool reload;


    // STUFF
    public int i;
    public int fightOrFlight; // simple value for a weighted random decision
    private float fightTime;
    private float flightTime;

   
    //Scripts
    private FXManager sfx;
    private bool sfxShoot = false;
    private int sfxID;
    public int sfxArrayIndex; // Will be accessed from SceneManager when culled to stop any lingering looping sounds - SET to -1 when reset in this script

    public int pointsValue;
    private ScriptPointsManager pointsScript;
    private ScriptASceneManager sceneManagerScript; 
    private ScriptAInputManager inp;

    private List<float> damageDelayList = new List<float>(); // Delay before bullet arrives from Raycast
    private List<int> damageList = new List<int>(); // Damage assigned from the Input script
    private List<Vector3> hitPositionList = new List<Vector3>(); // Vector assigned from the Input script

    private Vector3 sparkPosition;
    private  List<Transform> sparkList = new List<Transform>();
       
   
    //-------------------
    // AWAKE
    //-------------------



    void Awake()
    {

                
        Get_Prefab_Pointers();

        pointsValue = 125;

        armour = -40; // Very weak enemy :)

        hitValue = 0.5f; // Mere Bullets :)

        distanceStep = 0.5f;

        gravityRayLength = 0.5f;
        gravity = 10.0f;

        rayLength = 20;
        downStartDistance = 16;
        downRayLength = 2.5f;

        attackRadius = 150f;

        target = GameObject.Find("PlayerCamera").transform;

        FRAME_ARRAY = new Transform[transform.GetChildCount()];



        // Find relevant anim frames

        foreach (Transform child in transform)
        {

            if (child.renderer) { child.renderer.enabled = false; }

            switch (child.name)
            {

                case "muzzleFlash": FRAME_ARRAY[0] = (child);  break;
                case "muzzleFlashKneel": FRAME_ARRAY[1] = (child);  break;

                case "stand01": FRAME_ARRAY[2] = (child); break;
                case "stand02": FRAME_ARRAY[3] = (child); break;
                case "stand03": FRAME_ARRAY[4] = (child); break;
                case "stand04": FRAME_ARRAY[5] = (child); break;

                case "shoot_stand": FRAME_ARRAY[6] = (child); break;
                case "shoot_kneel": FRAME_ARRAY[7] = (child); break;

                case "run01": FRAME_ARRAY[8] = (child); break;
                case "run02": FRAME_ARRAY[9] = (child); break;
                case "run03": FRAME_ARRAY[10] = (child); break;
                case "run04": FRAME_ARRAY[11] = (child); break;
                case "run05": FRAME_ARRAY[12] = (child); break;
                case "run06": FRAME_ARRAY[13] = (child); break;
                case "run07": FRAME_ARRAY[14] = (child); break;
                case "run08": FRAME_ARRAY[15] = (child); break;
                case "run09": FRAME_ARRAY[16] = (child); break;
                case "run10": FRAME_ARRAY[17] = (child); break;
                case "run11": FRAME_ARRAY[18] = (child); break;

                case "die01": FRAME_ARRAY[19] = (child); break;
                case "die02": FRAME_ARRAY[20] = (child); break;
                case "die03": FRAME_ARRAY[21] = (child); break;
            }
        }
        //FRAME_ARRAY[18].renderer.enabled = true; // Quick test
    }




    //-------------------
    // PREFAB FINDER
    //-------------------
  

    void Get_Prefab_Pointers()
    {

        GameObject tempObject = GameObject.Find("A_Scene_Manager");
        sceneManagerScript = tempObject.GetComponent<ScriptASceneManager>(); // Access to 'LEVEL DATA[]'  file necessary


        //Find and get INPUT MANAGER SCRIPT - Totally necessary :)
        tempObject = GameObject.Find("A_GameWrapper");
        inp = tempObject.GetComponent<ScriptAInputManager>(); //

        tempObject = GameObject.Find("PlayerCamera");
        Camera playerCamera = tempObject.GetComponent<Camera>(); //
        sfx = tempObject.GetComponent<FXManager>(); // Sound and particles

        // NGUI LABELS POOL
        tempObject = GameObject.Find("A_Null_Labels");
        pointsScript = tempObject.GetComponent<ScriptPointsManager>(); 

        //pre = tempObject.GetComponent<ScriptASceneManager>();

        //for (int i = 0; i < pre.objectPoolList.Length; i++)
        //{
            // Assign Default projectile for this enemy to Shoot
            //if (pre.objectPoolList[i].name == "bulletTracer") { projectile = pre.objectPoolList[i]; }
            // Basic Thrust Particle Effect to be added to the missile so find a reference in the object pool and assign it
            //if (pre.objectPoolList[i].name == "P_fx_thrust01") { fx = pre.objectPoolList[i]; }
        //}


        //-------------------
        // Find Spawn positions for bullet sparks on Player's helicopter
        gunView = GameObject.Find("innerA").transform; // for 'Quickly' lining up bullets with end of barrel - Use as Muzzle flash too

        i = 0;
        for (i = 0; i < gunView.transform.childCount; i++)
        {
            Transform c = gunView.transform.GetChild(i);
            string st = c.name;
            //if (st.Contains("spp")) { Destroy(c.renderer); Mesh mesh = c.GetComponent<MeshFilter>().mesh; DestroyImmediate(mesh); Debug.Log("found!"); }
            if (st.Contains("spp")) { sparkList.Add(c); }
        }

    }







    //-------------------
    // START
    //-------------------
	void Start () 
    {

      
        dataSynch = false;


        endTime = 0;
        startPosition = transform.position;
        turnRate = Random.Range(360f, 480f);
        state = mode.passive;
        //state = mode.running;

        // Set Run up;
        animSpeed = Random.Range(0.04f, 0.08f);
        startFrame = 8;
        endFrame = 18;
        currentFrame =  currentFrame = Random.Range(2, 5); // Choose new random pose
        runSpeed = 0.4f/animSpeed;

        // Direction Switch Timer for Running mode
        directionTime = Time.time + Random.Range(5f, 15f);

        // Direction & Ray tests
        hit = false;
        targetAngle = 0;
        newDirection = false;

        // Bravery
        fightOrFlight = Random.Range(80, 90);

        // For shooting pauses
        reload = false;


        for (i = 0; i < FRAME_ARRAY.Length; i++) 
        {
           FRAME_ARRAY[i].renderer.enabled = false; // All left over Frames to be safe

        }

        // Set sound of Weapon
        i = Random.Range(1, 101);

        if (i < 50)
        {
            sfxID = 11; // Looping MACHINE GUN BURST
        }
        else
        {
            sfxID =12; // Looping MACHINE GUN BURST 2
        }


        i = 0;

       // sfx variables need reseting
        sfxShoot = false;
        sfxArrayIndex = -1;




	}


  



    //-------------------
    // UPDATE
    //-------------------


	void Update () 
    {



        // GET HEALTH OR ANY STATUS FROM SCENE MANAGER DATA ARRAY
      
        if (!dataSynch) { health = sceneManagerScript.Update_GameObject_Health_OnShow(this.gameObject); dataSynch = true; }



        if (inp.tapObject == this.transform)
        {

            // Retrieve information from Raycast Function in inputs
            damageDelayList.Add(inp.tapObjectDamageDelay);
            damageList.Add(inp.hitDamage);
            hitPositionList.Add(inp.tapObjectPosition);

            inp.tapObject = null;
        }




       




        if (state != mode.dying && state != mode.destroyed)
        {



            // DO WE HAVE DAMAGE WAITING TO ARRIVE?

            if (health < 0)
            {

                if (health < 0)
                {

                    state = mode.dying;
                    temp = 0; // use this for the dying Case Select
                    turnRate = 0;
                    endTime = 0.1f; // This is for the flicker upon death - before deletion
                    startTime = 0.2f;//
                    animTime = Time.time + 0.15f; // Display first die frame for this time period
                    if (sfxShoot) { Stop_Looping_SFX(); sfxShoot = false; } // Reset sound switch}
                    //bigExplosionPosition = hitPositionList[i];
                    //bigExplosionPosition.y = bigExplosionPosition.y + 2.0f; // Y Offset to reduce clipping effect on Floor
                }


            }

            else
            {

                for (i = damageDelayList.Count - 1; i >= 0; i--) // Recurse backwards in this case
                {
                    if (Time.time > damageDelayList[i])
                    {
                        Calculate_Damage(damageList[i]);

                        if (health < 0)
                        {

                            state = mode.dying;
                            temp = 0; // use this for the dying Case Select
                            turnRate = 0;
                            endTime = 0.1f; // This is for the flicker upon death - before deletion
                            startTime = 0.2f;//
                            animTime = Time.time + 0.15f; // Display first die frame for this time period
                            if (sfxShoot) { Stop_Looping_SFX(); sfxShoot = false; } // Reset sound switch}
                            //bigExplosionPosition = hitPositionList[i];
                            //bigExplosionPosition.y = bigExplosionPosition.y + 2.0f; // Y Offset to reduce clipping effect on Floor
                        }

                        damageDelayList.Remove(damageDelayList[i]);
                        damageList.Remove(damageList[i]);
                        hitPositionList.Remove(hitPositionList[i]);

                    }
                }
            }

            if (state != mode.dying)
            {

                //--------------
                if (Time.time > distanceTime)
                {
                    targetDistance = Vector3.Distance(transform.position, target.position);

                    distanceTime = Time.time + distanceStep;
                }
                //--------------
                if (targetDistance < attackRadius)
                {
                    if (state != mode.attack)
                    {


                        if (sfxShoot) { Stop_Looping_SFX(); sfxShoot = false; } // Reset sound switch }



                        if (Time.time > flightTime)
                        {

                            i = Random.Range(0, 101);

                            fightTime = Time.time + Random.Range(2f, 7f);

                            if (i < fightOrFlight)
                            {

                                state = mode.attack;

                            }
                            else
                            {
                                flightTime = Time.time + Random.Range(2f, 5f);

                                state = mode.running;



                            }
                        }
                    }

                }
                else
                {


                    if (sfxShoot) { Stop_Looping_SFX(); sfxShoot = false; } // Reset sound switch}
                }
            }

        }

        //--------------
        switch (state)
        {
            case mode.passive: DoStanding(); break;
            case mode.attack: Do_Attack_Mode(); break;
            case mode.running: DoRun(); break;
            case mode.dying: DoDying(); break;
            case mode.destroyed: Do_Destroy_Mode(); break;
        }

      
	}



    //-------------------
    // DAMAGE UPDATER
    //-------------------


    public void Calculate_Damage(int damage)
    {

        //Damage Saved from inputs script..gets set when raycast hits something
        health = (health - (damage - armour));

        // UPDATE SCENE MANAGER ARRAY DATA
        sceneManagerScript.Update_GameObject_Health_OnHit(this.gameObject, health);

       // if (health < 0)
      //  {

         //state = mode.dying;
        //Debug.Log("DAMAGEDONE!!" + health);

       // }

    }













    //-------------------
    // STANDING ANIMATION = just 4 poses for now
    //-------------------
    void DoStanding()
    {
        //--------------
        if  (Time.time > endTime)
        {
            FRAME_ARRAY[0].renderer.enabled = false; // Hide Muzzle flashes to be safe
            FRAME_ARRAY[1].renderer.enabled = false;

            endTime = Time.time + Random.Range(1f, 2f);

            if (currentFrame == 6 || currentFrame == 7) // If a shooting stance
            {
                FRAME_ARRAY[0].renderer.enabled = false; //Hide Gun Flares  - they may be visible
            }

            FRAME_ARRAY[currentFrame].renderer.enabled = false; // Hide current frame;

            currentFrame = Random.Range(2, 5); // Choose new random pose


            FRAME_ARRAY[currentFrame].renderer.enabled = true; // Show the mesh frame
    
        }


    }

   


    //-------------------
    //ATTACK BEHAVIOUR & ANIMATION
    //-------------------

    void Do_Attack_Mode()
    {


        //--------------
        if (Time.time > animTime) // This swaps between a 'Kneel Shoot' and a 'Stand Shoot'

        {
            animTime = Time.time + Random.Range(2f, 5f);

            FRAME_ARRAY[currentFrame].renderer.enabled = false; // Hide current frame;

            currentFrame = Random.Range(6, 8); // Choose new random pose

            FRAME_ARRAY[currentFrame].renderer.enabled = true; // Show the mesh frame

            FRAME_ARRAY[0].renderer.enabled = false; // Hide Muzzle flashes to be safe
            FRAME_ARRAY[1].renderer.enabled = false;

        }


        //--------------
        locked = Track_Target();

        //--------------
        if (targetDistance < attackRadius)
        {

            if (locked)
            {
                if (Time.time > reloadTimer)
                {

                    reloadTimer = Time.time + Random.Range(0.3f, 1.2f);
                  
                    reload = (!reload);
                }

                DoShooting();
               
            }

        }

        else
        {

            state = mode.passive;

        }

    }



    //-------------------
    //TRACK A TARGET
    //-------------------
  
    public bool Track_Target()
    {
        locked = false;

        //------------
        // LookAt  - rotate on Y axis

        targetVector = target.transform.position;
        targetVector.y = transform.position.y;

        targetPosition = targetVector - transform.position;
        targetDirection = Vector3.RotateTowards(transform.forward, targetPosition, Mathf.Deg2Rad * turnRate * Time.deltaTime, 1);
        transform.rotation = Quaternion.LookRotation(targetDirection);

        //------------
        // Check if 'locked on'

        ly = Vector3.Angle(targetPosition, transform.forward);
        
        if (ly < 10.0F) { locked = true; }


        return locked;


    }


    //-------------------
    //PERFORM 'KEEL OVER'
    //-------------------
    void DoDying()
    {

        if (turnRate > -90f)
        {
            turnRate = turnRate  + (-270f * Time.deltaTime);
          
            transform.eulerAngles = new Vector3(turnRate, transform.eulerAngles.y, transform.eulerAngles.z);
        }
        else
        {
            turnRate = -90;
            //transform.rotation = (Vector3.right * turnRate);
            transform.eulerAngles = new Vector3(turnRate, transform.eulerAngles.y, transform.eulerAngles.z);
        }

        //if ((FRAME_ARRAY[currentFrame].transform.eulerAngles.x) > -90f)
        //{
            
        //}

        switch (temp)
        {
            case 0:

                transform.rigidbody.isKinematic = true;

                foreach (Transform child in transform)  { if (child.renderer) { child.renderer.enabled = false; } } // Hide all frames here to be safe

                 FRAME_ARRAY[19].renderer.enabled = true; //
                 currentFrame = 19;

                pointsScript.Activate_Points_Label(transform.position, pointsValue);// SHOW POINTS


                 temp = Random.Range(1, 4);

                switch (temp) // Play Scream sfx
                {
                    case 1: 
                        sfx.PlaySound(24, transform.position); break;
                    case 2: 
                        sfx.PlaySound(25, transform.position); break;
                    case 3: 
                        sfx.PlaySound(26,transform.position); break;
                }




                 temp = 1;
        
                break;

            case 1:

                if (Time.time > animTime)
                {
                    FRAME_ARRAY[currentFrame].renderer.enabled = false; //Hide Current Frame
                    FRAME_ARRAY[20].renderer.enabled = true; //
                    currentFrame = 20;
                    animTime = Time.time + 0.15f; // Display first die frame for this time period
                    temp = 2;
                }


                break;

            case 2:

                if (Time.time > animTime)
                {
                    FRAME_ARRAY[currentFrame].renderer.enabled = false; //Hide Gun Flares  - they may be visible
                    FRAME_ARRAY[21].renderer.enabled = true; //
                    currentFrame = 21;
                    animTime = Time.time + 0.15f; // Display first die frame for this time period
                    temp = 3;
                }

                break;

            case 3:


                if (Time.time > animTime)
                {
           
                    if (FRAME_ARRAY[currentFrame].renderer.enabled == true)
                    {
                        animTime = Time.time + endTime; // 0.1f; // Display first die frame for this time period
                        FRAME_ARRAY[currentFrame].renderer.enabled = false; //
                        endTime = endTime - 0.007f;
                    }
                    else
                    {
                        startTime = startTime - 0.02f;
                        if (startTime < 0) { startTime = 0.007f; }
                        animTime = Time.time + startTime; // // Display first die frame for this time period
                        FRAME_ARRAY[currentFrame].renderer.enabled = true; //

                    }
                }

                if (endTime < 0)
                {
                    FRAME_ARRAY[currentFrame].renderer.enabled = false;
                    state = mode.destroyed;

                }


                break;









        }

    }






    //-------------------
    //SHOOT AT TARGET
    //-------------------
    
    void DoShooting()
    {



        if ( !reload)
        {
            if (!sfxShoot)
            {
            
             sfxArrayIndex = sfx.PlayLoopingSoundWithDistance(sfxID, transform.position, 0.75f); // Looping MACHINE GUN BURST

             if (sfxArrayIndex > 0) { sfxShoot = true; } // Only if return value is within Audio Source Array limits

            } 
       

            //--------------
            if (currentFrame == 6) // STAND SHOOTING - show only stand shooting MuzzleFlash
            {
                if (Time.time > muzzleTime)
                {
                    if (FRAME_ARRAY[0].renderer.enabled)
                    {
                        FRAME_ARRAY[0].renderer.enabled = false; //Hide Gun Flares  - they may be visible
                        muzzleTime = Time.time + Random.Range(0.05f, 0.1f);
                    }
                    else
                    {
                        FRAME_ARRAY[0].renderer.enabled = true; //Hide Gun Flares  - they may be visible
                        muzzleTime = Time.time + Random.Range(0.05f, 0.1f);

                        FRAME_ARRAY[0].transform.LookAt(target);
                    }

                }

                else
                {
                    if (FRAME_ARRAY[0].renderer.enabled == true)
                    {
                        Score_A_Hit();
                    }
                }

            }

            else
            {
                if (Time.time > muzzleTime) // KNEEL SHOOTING- show only Kneel shooting MuzzleFlash
                {
                    if (FRAME_ARRAY[1].renderer.enabled)
                    {
                        FRAME_ARRAY[1].renderer.enabled = false; //Hide Gun Flares  - they may be visible
                        muzzleTime = Time.time + Random.Range(0.05f, 0.1f);
                    }
                    else
                    {
                        FRAME_ARRAY[1].renderer.enabled = true; //Hide Gun Flares  - they may be visible
                        muzzleTime = Time.time + Random.Range(0.05f, 0.1f);

                        FRAME_ARRAY[1].transform.LookAt(target);
                    }

                }
                else
                {
                    if (FRAME_ARRAY[0].renderer.enabled == true)
                    {
                        Score_A_Hit();
                    }
                }


            }

        }
        else
        {
            if (sfxShoot) { Stop_Looping_SFX();  sfxShoot = false; }// Reset sound switch}

            FRAME_ARRAY[0].renderer.enabled = false; // Hide Muzzle flashes to be safe
            FRAME_ARRAY[1].renderer.enabled = false;

           
        }



        //-----------------
        // Change position and run a little

        if (Time.time > fightTime) 
        { 
            state = mode.running; 
            flightTime = Time.time + Random.Range(2f, 5f);
            FRAME_ARRAY[currentFrame].renderer.enabled = false; //Hide Old Frame

            FRAME_ARRAY[0].renderer.enabled = false; // Hide Muzzle flashes to be safe
            FRAME_ARRAY[1].renderer.enabled = false;

            currentFrame = startFrame;
            FRAME_ARRAY[currentFrame].renderer.enabled = true; // Show New Frame

            animTime = 0;
        }

    }





    //-------------------
    // SCORE A HIT ON THE PLAYER COPTER
    //-------------------
    private float scoreHitTime;

    void Score_A_Hit()
    {
        if (Time.time > scoreHitTime)
        {
            scoreHitTime = Time.time+0.2f;

            temp = Random.Range(1, 101);

            if (temp < 50) // was 5
            {

                temp = Random.Range(0, sparkList.Count); // Spark list is a list of invisible child transforms on helicopter where spark may be seen & spawned

                // 7 is a SPARK FX
                sfx.Play_One_Shot_Parented_Particle(7, gunView, sparkList[temp].transform.localPosition);

                temp = Random.Range(1, 4);

                switch (temp) // Play Whizz Bang hits sfx
                {
                    case 1: // Left Vertical hit
                        sfx.PlaySound(14, sparkList[temp].transform.position); break;
                    case 2: // Right Vertical hit
                        sfx.PlaySound(15, sparkList[temp].transform.position); break;
                    case 3: // Top Horizontal hit
                        sfx.PlaySound(16, sparkList[temp].transform.position); break;
                }

                inp.Update_Player_Health(hitValue);


            }

        }

    }








    //-------------------
    // RUN AROUND & TEST FOR OBSTRUCTIONS
    //-------------------
    void DoRun()
    {

        if (Time.time <= flightTime)
        {

            //-----------------------
            // Animation Frames - hides/Shows separate meshes

            if (Time.time > animTime)
            {

                animTime = Time.time + animSpeed;

                FRAME_ARRAY[currentFrame].renderer.enabled = false; //Hide Old Frame

                currentFrame++;

                if (currentFrame > endFrame) { currentFrame = startFrame; }

                FRAME_ARRAY[currentFrame].renderer.enabled = true; // Show New Frame
            }







            //-----------------------
            // Tests for Obstructions
            if (newDirection)
            {
                float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, runTargetAngle, turnRate * Time.deltaTime); transform.eulerAngles = new Vector3(0, angle, 0);

                float a = Mathf.Abs((transform.eulerAngles.y - runTargetAngle));

               // Debug.Log(" Finding new angle  " + a);

                if (a < 5f) { newDirection = false; }
            }
            else
            {

                // Randomish direction changer

                if (Time.time > directionTime) { newDirection = true; directionTime = Time.time + Random.Range(5f, 15f);}


                // Regular Obstruction Ray test

                if (!hit)
                {
                    if (Time.time > rayTime)
                    {

                        rayTime = Time.time + 0.2f;
                        Do_RayPick();
                    }
                }

                else
                {
                    Do_NewDirectionTest(leftOrRight);
                }
            }



            //----------------------- 
            // Running Translation
            transform.Translate(Vector3.forward * (runSpeed * Time.deltaTime)); // Perform Gravity effect
            //---------------
            // Gravity
            Do_Gravity();

            Vector3 tempVector = transform.eulerAngles;
            tempVector.x = 0;
            tempVector.z = 0;
            transform.rotation = Quaternion.Euler(tempVector);

           
          

            //----------------------- 
            // Quick Debug - make a dummy object to show where Ray ends & points
            /*
             if (cube) 
             {
                 cube.transform.rotation = transform.rotation;
                 cube.transform.position = FRAME_ARRAY[currentFrame].transform.position;
                 cube.transform.position = cube.transform.position + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * transform.forward) * rayLength;
             }
             */
        }

        else
        {
            state = mode.passive;

        }
    }


    void Do_Gravity()
    {

         //----------------------- 
        // Don't want exact Transform as the Y is too low - forgot to centre Model in Y plane in 3Dsmax :(
        rayStartPosition = transform.position;
        rayStartPosition.y = rayStartPosition.y + 0.25f;


        if (!Physics.Raycast(rayStartPosition, -transform.up, gravityRayLength))
        {
            transform.Translate(Vector3.down * (gravity * Time.deltaTime)); // Perform Gravity effect
         
        }
       
        
    }


    
    
    //-------------------
    // RAY PICK TESTS
    //-------------------
    //private GameObject cube; 

    void Do_RayPick()
    {
        
        hit = false;
        targetAngle = 0;

         // Quick Debug - make a dummy object to show where Ray ends & points
       
        //if (cube == null){ cube = GameObject.CreatePrimitive(PrimitiveType.Cube); cube.transform.localScale = new Vector3(0.2f, 0.2f, 1f); }             
        //Vector3 rayDirection = transform.TransformDirection(Vector3.forward);
        //cube.transform.rotation = transform.rotation;
        //cube.transform.position = FRAME_ARRAY[currentFrame].transform.position;
        //cube.transform.Translate(transform.forward *29f, Space.World);
        //cube.transform.position = cube.transform.position + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * transform.forward) * rayLength;
        



        //----------------------- 
        // Don't want exact Transform as the Y is too low - forgot to centre Model in Y plane in 3Dsmax :(
        rayStartPosition = transform.position;
        rayStartPosition.y = rayStartPosition.y + 1.5f;


        // Forward check for obstacles
        //----------------------- 
        rayDirection = (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * transform.forward);

       


        if (Physics.Raycast(rayStartPosition, rayDirection, rayLength))
        {

          
            hit = true;
            int temp = Random.Range(0, 101);

            if (temp < 51)
            {
                leftOrRight = true; // Go left
            }
            else
            {
                leftOrRight = false; // Go right
            }

            newDirection = false;

            Do_NewDirectionTest(leftOrRight);




        }
        else
        {


            // Downward check for drop offs
            //----------------------- 
            Vector3 StartPosition = rayStartPosition + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * (transform.forward * downStartDistance ));
            rayDirection =  -transform.up;

            if (!Physics.Raycast(StartPosition, rayDirection, out hitobject, downRayLength))
            {

              
                //Debug.Log(" Nothing here!!");
                //cube.transform.position = rayStartPosition + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * (transform.forward * (rayLength / 2f)));
                hit = true;
                int temp = Random.Range(0, 101);

                if (temp < 51)
                {
                    leftOrRight = true; // Go left
                }
                else
                {
                    leftOrRight = false; // Go right
                }

                newDirection = false;

                Do_NewDirectionTest(leftOrRight);
              
            }








        }


       



    }







    //-------------------
    // FREE DIRECTION TESTS
    //-------------------



    void Do_NewDirectionTest(bool leftOrRight)
    {

        i = 0;

        while ( i < 7)
        {
            i++;

            if (leftOrRight) // Check LEFT First
            {

                switch (targetAngle)
                {
                    case 0: targetAngle = 45; break;

                    case 45: targetAngle = 90; break;

                    case 90: targetAngle = 135; break;

                    case 135: targetAngle = -45; break;

                    case -45: targetAngle = -90; break;

                    case -90: targetAngle = -135; break;

                    case -135:

                        //If already exhausted direction tests - simply about turn and go back

                        runTargetAngle = (int)(transform.localEulerAngles.y + 180);
                        runTargetAngle = ClampAngle(runTargetAngle);
                        targetAngle = 0;
                        newDirection = true;
                        hit = false;
                      
                        return;
                }



            }


            else // Check RIGHT
            {
                switch (targetAngle)
                {

                    case 0: targetAngle = -45; break;

                    case -45: targetAngle = -90; break;

                    case -90: targetAngle = -135; break;

                    case -135: targetAngle = 45; break;

                    case 45: targetAngle = 90; break;

                    case 90: targetAngle = 135; break;

                    case 135:

                        //If already exhausted direction tests - simply about turn and go back

                        runTargetAngle = (int)(transform.localEulerAngles.y + 180);
                        runTargetAngle = ClampAngle(runTargetAngle);
                        targetAngle = 0;
                        newDirection = true;
                        hit = false;
                       
                        return;

                }

            }


            // If we've not exhausted all reasonable directions yet

            if (targetAngle != 0)
            {
                rayDirection = (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * transform.forward);


                // If hit an empty space then set new direction for there
                if (!Physics.Raycast(rayStartPosition, rayDirection, rayLength))
                {


                    // Downward check for drop offs
                    //----------------------- 
                    Vector3 StartPosition = rayStartPosition + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * (transform.forward * downStartDistance));
                    rayDirection = -transform.up;

                    if (Physics.Raycast(StartPosition, rayDirection, downRayLength)) //out hitobject,
                    {



                        //cube.transform.position = rayStartPosition + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * (transform.forward * (rayLength / 2f)));

                        runTargetAngle = (int)(transform.localEulerAngles.y + targetAngle);
                        runTargetAngle = ClampAngle(runTargetAngle);
                        targetAngle = 0;
                        newDirection = true;
                        hit = false;
                    }



                    // Quick Debug
                    /*
                    GameObject qube;
                    qube = GameObject.CreatePrimitive(PrimitiveType.Cube); qube.transform.localScale = new Vector3(0.2f, 0.2f, 1f);
                    qube.transform.rotation = transform.rotation;
                    qube.transform.position = rayStartPosition;
                    qube.transform.position = qube.transform.position + (Quaternion.Euler(transform.eulerAngles.x, targetAngle, transform.eulerAngles.z) * transform.forward) * rayLength;
                    qube = null;
                    */

                }


            }
        }
       
       
        
    }



    //-------------------
    // CLAMP ANGLES
    //-------------------
    private int ClampAngle(int angle)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;
        return Mathf.Clamp(angle, 0, 355);
    }







    




    //-------------------
    // Stop Looping Sound
    //-------------------
    public void Stop_Looping_SFX()
    {

        sfx.Stop_Looping_Sound(sfxArrayIndex);

        sfxArrayIndex = -1;

    }

    //-------------------
    // PERFORM DEATH
    //-------------------
    void Do_Destroy_Mode()
    {

        //sfx.Play_One_Shot_Particle(2, bigExplosionPosition); // Play a bigger Explosion with Blast wave 
        //sfx.Make_Metal_Debris(transform.position);
       // pointsScript.Activate_Points_Label(transform.position, pointsValue);// SHOW POINTS
        sceneManagerScript.Permanently_Destroy_GameObject(this.gameObject);

        //bigExplosionPosition = Vector3.zero;
        state = mode.passive;
        dataSynch = false;
        damageDelayList = null;
        hitPositionList = null;
        damageList = null;
        damageDelayList = new List<float>(); // Delay before bullet arrives from Raycast
        damageList = new List<int>(); // Damage assigned from the Input script
        hitPositionList = new List<Vector3>(); // Vector assigned from the Input script

        // RESET rigidBody
        transform.rigidbody.isKinematic = false;
        transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);
    }
}
