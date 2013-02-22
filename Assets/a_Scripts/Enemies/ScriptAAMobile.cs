using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptAAMobile : MonoBehaviour {

    public Transform target;

    private bool dataSynch; // Switch which allows a pause for the health & any other data to be retrived from the LevelObjectArray[]

    private int health;

    private int armour; // Armour subtractor rating enables different endurance to heavier bullets

    private int damage;

    private int attackRadius;

    private int blindRadius; //When player too close

    private Vector3 targetPosition;

    private float xAngle;

    private float yAngle;

    private float turnRate;

    private float reactionTimer;

    private float reloadTimer;

    private float bulletDelay; // Time spacing between bullets in a burst

    public Transform turret;

    public Transform hinge;

    private GameObject bullet;

    private int burst; // Maximum Burst of bullets

    private int burstCount; // Count for bullets fired
    
    private enum mode { passive, alert, attack, destroyed };

    private mode state;

    private ScriptBulletTracer bulletScript;

    private GameObject fx;

    private FXManager sfx;

    private Camera playerCamera;

    private Vector3[] bulletXY;  // Spawning offsets but also stores projectile's life in the Z

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
        pointsValue = 750;

        armour = 5; // subtracted from any hit damage values this enemy may receive


        burst = 10; // Maximum bullet salvo
        bulletXY = new Vector3[burst]; //for 4 BARRELS:  offsets on the end of the turret Gun: 2x2 grid
        //bulletXY[0] = new Vector3(0, 0, 0);
        //bulletXY[1] = new Vector3(0, 0, 0);
        //bulletXY[2] = new Vector3(0, 0, 0);
        //bulletXY[3] = new Vector3(0, 0, 0);
      

        turretZOffset = 7.5f; // Makes Missile appear on end of 'barrel' Z and not from its pivot point



        state = mode.passive;

        attackRadius = 350;
        blindRadius = 80; // Switches off the targeting if too close

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
        for (int i = 0; i < transform.childCount; i++) { Transform c = transform.GetChild(i); if (c.name == "turret") { hinge = c; } }
        // Get turret
        for (int i = 0; i < hinge.transform.childCount; i++) { Transform c = hinge.transform.GetChild(i); if (c.name == "gun") { turret = c; } }

       
    }



    //-------------------
    // START... REMEMBER - A RE-USED POOL OBJECT CALLS START() EACH TIME - so any relevant individual values need setting here & not in Awake()
    //-------------------
   
    void Start()
    {
        burst = 0; // Reset so sound plays immediately when shooting for the first time

        dataSynch = false;

        bigExplosionPosition = Vector3.zero; //Final Death Big explosion trigger

        state = mode.passive;

        if (projectile == null)
        {
            Get_Prefab_Pointers();
        }

        turnRate = Random.Range(40f, 60f);

        //hinge.Rotate(Vector3.up * (int)Random.Range(0f, 355f));

        turret.localEulerAngles = new Vector3((int)Random.Range(-5f, -25f), turret.localEulerAngles.y, turret.localEulerAngles.z);


          

    }



    //-------------------
    // PREFAB FINDER
    //-------------------
    private GameObject projectile;

    void Get_Prefab_Pointers()
    {
       
        GameObject tempObject = GameObject.Find("A_Scene_Manager");
        ScriptASceneManager pre;

        pre = tempObject.GetComponent<ScriptASceneManager>();

        for (int i = 0; i < pre.objectPoolList.Length; i++)
        {
            // Assign Default projectile for this enemy to Shoot
            if (pre.objectPoolList[i].name == "bulletTracer") { projectile = pre.objectPoolList[i]; }
            // Basic Thrust Particle Effect to be added to the missile so find a reference in the object pool and assign it
            //if (pre.objectPoolList[i].name == "P_fx_thrust01") { fx = pre.objectPoolList[i]; }
        }

    }




    //-------------------
    // UPDATE
    //-------------------
    private int i;

    void Update()
    {

        // GET HEALTH OR ANY STATUS FROM SCENE MANAGER DATA ARRAY
        if (!dataSynch) { health = sceneManagerScript.Update_GameObject_Health_OnShow(this.gameObject); dataSynch = true;  }



        if (inp.tapObject == this.transform)
        {
           
            // Retrieve information from Raycast Function in inputs
            damageDelayList.Add(inp.tapObjectDamageDelay);
            damageList.Add(inp.hitDamage);
            hitPositionList.Add(inp.tapObjectPosition);

            inp.tapObject = null;
        }




        // DO WE HAVE DAMAGE WAITING TO ARRIVE?



        for (i = damageDelayList.Count - 1; i >= 0; i--) // Recurse backwards in this case
        {
            if (Time.time > damageDelayList[i])
            {
                Calculate_Damage(damageList[i]);

                if (health < 0)
                {
                    bigExplosionPosition = hitPositionList[i];
                    bigExplosionPosition.y = bigExplosionPosition.y + 2.0f; // Y Offset to reduce clipping effect on Floor
                }

                damageDelayList.Remove(damageDelayList[i]);
                damageList.Remove(damageList[i]);
                hitPositionList.Remove(hitPositionList[i]);

            }
        }

            

        




       

        if (health > 0)
        {

            // DISTANCE TO PLAYER

            dist = Vector3.Distance(transform.position, target.position);

        }



        // BEHAVIOUR STATES;

        switch (state)
        {
            case mode.passive:

                Do_Passive_Mode();

                break;

            case mode.alert:

                if (Time.time > reactionTimer)
                {

                    state = mode.attack;

                }
                else
                {
                    Do_Alert_Mode();
                }

                break;

            case mode.attack:

                Do_Attack_Mode();

                break;

            case mode.destroyed:

                //Debug.Log("Health On Destroy: " + health);

                Do_Destroy_Mode();

                break;

        }


    }







    //-------------------
    // DAMAGE UPDATER
    //-------------------


    private int damageTest;
    public void Calculate_Damage(int damage)
    {

        //Damage Saved from inputs script..gets set when raycast hits something

        damageTest = (damage - armour);

        if (damageTest < 1) { damage = 5; }
       

        //Damage Saved from inputs script..gets set when raycast hits something
        health = (health - (damage - armour));

       
        // UPDATE SCENE MANAGER ARRAY DATA
        sceneManagerScript.Update_GameObject_Health_OnHit(this.gameObject, health);



        if (health < 0)
        {

            state = mode.destroyed;
            //Debug.Log("Destroyed");

        }




    }












    //-------------------
    // OLD DAMAGE UPDATER
    //-------------------
    void Calculate_DamageOLD_NO_LONGER_USED()
    {

        if (inp.tapObject == this.transform)
        {

             //get from inputs script..gets set when raycast hits something
            health = (health - (inp.hitDamage - armour ));
            
        }

      

        // UPDATE SCENE MANAGER ARRAY DATA
        sceneManagerScript.Update_GameObject_Health_OnHit(this.gameObject, health);



        if (health < 0)
        {

            state = mode.destroyed;
        
        }
       



    }




    //-------------------
    // PASSIVE MODE
    //-------------------
    void Do_Passive_Mode()
    {



        if (dist < attackRadius && dist > blindRadius) // Switches off the targeting if too close)
        {

            state = mode.alert;

            reactionTimer = Time.time + Random.Range(0.5f, 5f); // delay before opening fire

        }
        else
        {

            hinge.Rotate(Vector3.up * 30f * Time.deltaTime);

        }

    }



    //-------------------
    // ALERT
    //-------------------
    void Do_Alert_Mode()
    {


        float dist = Vector3.Distance(transform.position, target.position);

        if (dist < attackRadius && dist > blindRadius) // Switches off the targeting if too close)
        {
            Track_Target();
        }
        else
        {
            state = mode.passive;
        }

    }

    //-------------------
    // ATTACK
    //-------------------
   
    void Do_Attack_Mode()
    {

        bool locked = Track_Target();


        float dist = Vector3.Distance(transform.position, target.position);

        if (dist < attackRadius && dist > blindRadius) // Switches off the targeting if too close)
        {

            if (locked)
            {
                

                if (Time.time > reloadTimer)
                {
                    // How many bullets in a burst - reset after bullets fired
                    if (burst == 0) { burst = (int)Random.Range(4f, 10f); sfx.PlaySound(6, Vector3.zero); } // MACHINE GUN BURST

                    Shoot_Bullet();

                    

                  
                }
            }

        }

        else
        {

            state = mode.passive;

        }

    }


    // Remember All active objects get rooted to the nullActivePool object for easy deactivation
    // with one call for menu or pause - here we attach it to a new parent to align it with the barrel 
    // or turret of the gameobject that is firing it - then we must remember to reparent it back to its 
    // Null Root Parent or else it will not pause with one call eg: GameObject.SetActive(A_Null_Active_Pool)

    Transform nullParent;

    void Shoot_Bullet()
    {



        //for (int i = 0; i < burst; i++)

        if (Time.time > bulletDelay)
        {
            if (burstCount < (burst-1))
            {

                bulletDelay = Time.time + Random.Range(0.1f, 0.15f);

                burstCount++;
                //if (bulletXY[i].z > 0)
                //{
                //if (Time.time > bulletXY[i].z) { bulletXY[i].z = 0f; }

                //}
                //else
                //{
                bulletXY[burstCount].z = Time.time + 3.5f; // LIFE OF BULLET
                bullet = ObjectPoolManager.CreatePooled(projectile, turret.transform.position, Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                bullet.SetActive(true);
                bulletScript = bullet.GetComponent<ScriptBulletTracer>() as ScriptBulletTracer;
                bulletScript.enabled = true;
                //bullet.collider.enabled = true;
                //Physics.IgnoreCollision(bullet.collider, collider); // STOP Bullet colliding with this object - IMPORTANT!!!
                bulletScript.bulletSpeed = 130f;
                bulletScript.bulletLife = bulletXY[burstCount].z; // Stored life duration timer in the Vector3 Z as it was free
                bulletScript.bulletHitValue = 20;

                bullet.renderer.enabled = true;

                //Vector3(bulletXY[i].x, bulletXY[i].y, turretZOffset)
                bullet.transform.rotation = turret.transform.rotation;
                bullet.transform.eulerAngles = new Vector3(turret.eulerAngles.x + (int)Random.Range(-5f, 5f), turret.eulerAngles.y + (int)Random.Range(-2f, 2f), (int)turret.eulerAngles.z);

                // SAVE ORIGINAL NULL POOL PARENT
                nullParent = bullet.transform.parent;

                bullet.transform.parent = turret.transform;


                bullet.transform.localPosition = new Vector3(bulletXY[burstCount].x, bulletXY[burstCount].y, turretZOffset);



                bullet.transform.parent = null;

                //--------------
                //FX: Thrust particle
                //--------------
                //fx = sfx.Play_Particle(1, bullet.transform.position); // 1 is standard Billboard explosion but with a frozen animation frame
                //fx.transform.parent = bullet.transform;
                //fx.transform.localPosition = new Vector3(0, 0, -3.5f);

                //bulletScript.sfx = sfx; // Pass handle to FX manager
                //bulletScript.fx = fx; // so Bullet can call Pool destroy on this particle
                bulletScript.target = target;

                // RESET GLOBAL PARENT AGAIN
                bullet.transform.parent = nullParent;

                //break;
                //}


            }

            else
            {
                burst = 0; // Reset ready for another salvo
                burstCount = 0;
                reloadTimer = Time.time + Random.Range(0.3f, 2f);

            }

        }
       


    }







    //-------------------
    // DESTROYED - DEATH
    //-------------------
    void Do_Destroy_Mode()
    {
        
        sfx.Play_One_Shot_Particle(2, bigExplosionPosition); // Play a bigger Explosion with Blast wave 
        sfx.Make_Metal_Debris(transform.position);
        pointsScript.Activate_Points_Label(transform.position, pointsValue);// SHOW POINTS
        sceneManagerScript.Permanently_Destroy_GameObject(this.gameObject);

        bigExplosionPosition = Vector3.zero;
        state = mode.passive;
        dataSynch = false;
        damageDelayList = null;
        hitPositionList = null;
        damageList = null;
        damageDelayList = new List<float>(); // Delay before bullet arrives from Raycast
        damageList = new List<int>(); // Damage assigned from the Input script
        hitPositionList = new List<Vector3>(); // Vector assigned from the Input script
    }











    //-------------------
    //TRACK A TARGET
    //-------------------
    private float ly;
    private float lz;
    private Vector3 targetVector;
    private Vector3 targetDirection;


    public bool Track_Target()
    {
        bool locked = false;

        //------------
        // Hinge LookAt  - rotate on Y axis

        targetVector = target.transform.position;
        targetVector.y = hinge.position.y;

        targetPosition = targetVector - hinge.position;
        targetDirection = Vector3.RotateTowards(hinge.transform.forward, targetPosition, Mathf.Deg2Rad * turnRate * Time.deltaTime, 1);
        hinge.transform.rotation = Quaternion.LookRotation(targetDirection);

        //------------
        // Turret LookAt - rotate on X axis

       

            ly = turret.eulerAngles.y; // save current PARENT ('named:hinge') angles - only want the X for up and down rotation on turret
            lz = turret.eulerAngles.z;
            targetVector = target.transform.position;

            targetPosition = targetVector - hinge.position;
            targetDirection = Vector3.RotateTowards(turret.transform.forward, targetPosition, Mathf.Deg2Rad * turnRate * Time.deltaTime, 1);
           

            turret.transform.rotation = Quaternion.LookRotation(targetDirection);


            //if (turret.eulerAngles.x < 310f)
           // {

                //turret.eulerAngles = new Vector3(310f, ly, lz);
            //}

            //else
            //{

                turret.eulerAngles = new Vector3(turret.eulerAngles.x, ly, lz);
            //}


            //------------
            // Check if turret is 'locked on'

            targetPosition = targetVector - turret.transform.position;

            ly = Vector3.Angle(targetPosition, turret.transform.forward);

            if (ly < 10.0F) { locked = true; }




       

        return locked;


    }




}

