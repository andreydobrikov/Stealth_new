using UnityEngine;
using System.Collections;

public class ScriptGunControls : MonoBehaviour
{
  
    //--------
    // TEMP MOUSELOOK
    //----------

    //enum Axes { MouseXandY, MouseX, MouseY }
   // Axes Axis = Axes.MouseXandY;

    public enum RotationAxes { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
    public RotationAxes axes = RotationAxes.MouseXAndY;

  
    float sensitivityX = 1.75f;
    float sensitivityY = 1.75f;
    float minimumX = -42.0f;
    float maximumX = 42.0f;
    float minimumY = -60.0f;
    float maximumY = 60.0f;
    float rotationX = 0.0f;
    float rotationY = 0.0f;
    float lookSpeed = 2.0f;
    float axisX;
    float axisY;


    //--------
    //GUN & BULLET STUFF
    //----------

    private Transform gunFlare;
    private Transform gunFlareLeft;
    private Transform gunFlareRight;
    private bool gunFlareToggle; // Toggles left and right Muzzle flashes

    public GameObject gun;
    private GameObject gunView; // This is the parent object of the Gun
    private GameObject bullet;
    private Transform tempBullet; // Just a placeholder Bullet mesh for now
    private GameObject bulletDefault;
    private GameObject bulletMachineGun;
    private GameObject bulletSpray;
    private GameObject bulletTracer;
    private GameObject bulletLoEx;
    private GameObject bulletHiEx;

    private int machineGunBulletCount;

    private GameObject tempObject; // Just a temporary handle for gameobjects
   
    private float muzzleFlashTime;
    private float shellFlashTime;
    public int bulletID; // Tells us which projectile we are firing
    public int activeBulletCount; //How many bullets are flying around still - decrease as bullets are destroyed on impact
    private float bulletLife; // Sets the extermination time of bullets in case they don't hit anything - gets sent to bullet script on spawn
    private float bulletSpeed; // - gets sent to bullet script on spawn
    private int bulletHitValue; // - gets sent to bullet script on spawn
    private Transform bulletHitTarget; //  obtained from inputs script raycast
    private string bulletHitTargetTag; //  obtained from inputs script raycast
    private ScriptBullet bulletScript;
    private ScriptAInputManager inp; // Handle for accessing the 'Controls input Script attached to the GameWrapper Object.

    private FXManager sfx;  // For particles or sound  - has basic functions to control them

    private bool sfxShoot = false;
    private int sfxID;
    private int sfxArrayIndex; // Returned value when sound is played  - indicates the position in the Audio sources array so can be tracked & switched off if looping
    private bool sfxShootLoop;


    //--------
    //RECOIL STUFF
    //----------

    public bool recoilTrigger;
    private int recoilStage; //Counter for processing recoil stages/animations etc..
    private float recoilTime;
    private float recoilValue; //Amount of recoil
   

    private Vector3 recoilStartPosition;
    private Vector3 recoilTargetPosition;
    
    //public Transform cameraReference;
    private Camera playerCamera;

    
    public ParticleSystem p_exp01;


    private float[,] SHELL_DATA;
    private Transform[] SHELL_ARRAY;
    private Transform[] TSHELL_ARRAY;
    private int shellCount;
    private int i;
    private Vector3 shellStartPosition;
    private Transform parentTransform;
    Quaternion originalRotation; // For dirty mouse control - take out..

    
    //************************
    // PREFAB POINTERS


    void Awake()
    {
        SHELL_ARRAY = new Transform[5]; // For machine gun bullets
        TSHELL_ARRAY = new Transform[5]; // For machine gun bullets
        SHELL_DATA = new float[5, 8]; // Bool, Time, Time step, X,Y,Z scales, X, Y, Z positions
               
        
        // MACHINE GUN BULLET SHELLS
         for (int i = 0; i < transform.childCount; i++) 
         {
             Transform c = transform.GetChild(i);
             if (c.name == "empty_shell") { SHELL_ARRAY[0] = c; c.renderer.enabled = false; }
             if (c.name == "empty_shell01") { SHELL_ARRAY[1] = c; c.renderer.enabled = false; }
             if (c.name == "empty_shell02") { SHELL_ARRAY[2] = c; c.renderer.enabled = false; }
             if (c.name == "empty_shell03") { SHELL_ARRAY[3] = c; c.renderer.enabled = false; }
             if (c.name == "empty_shell04") { SHELL_ARRAY[4] = c; c.renderer.enabled = false; } 

         }

        // TRACER BULLET SHELLS

         for (int i = 0; i < transform.childCount; i++)
         {
             Transform c = transform.GetChild(i);
             if (c.name == "empty_Tshell") { TSHELL_ARRAY[0] = c; c.renderer.enabled = false; }
             if (c.name == "empty_Tshell01") { TSHELL_ARRAY[1] = c; c.renderer.enabled = false; }
             if (c.name == "empty_Tshell02") { TSHELL_ARRAY[2] = c; c.renderer.enabled = false; }
             if (c.name == "empty_Tshell03") { TSHELL_ARRAY[3] = c; c.renderer.enabled = false; }
             if (c.name == "empty_Tshell04") { TSHELL_ARRAY[4] = c; c.renderer.enabled = false; }
         }



         shellStartPosition = SHELL_ARRAY[0].transform.localPosition; //new Vector3(0.08805615,0.1419492,0.4300919) // Original position








         GameObject tempObject = GameObject.Find("GunView");
         parentTransform = tempObject.transform;
    }






   
    void Get_Prefab_Pointers()
    {
        //GameObject tempObject;
        tempObject = GameObject.Find("A_Scene_Manager");
        ScriptASceneManager pre;

        pre = tempObject.GetComponent<ScriptASceneManager>();

        for ( i = 0; i <pre.objectPoolList.Length; i++)
        {
            //Default Bullet
            if (pre.objectPoolList[i].name == "bulletDefault") { bulletDefault = pre.objectPoolList[i];  }
            //Tracer Bullet
            if (pre.objectPoolList[i].name == "bulletTracerPlayer") { bulletTracer = pre.objectPoolList[i]; }
            //Hi Ex Bullet
            if (pre.objectPoolList[i].name == "bullet_hi_ex") { bulletHiEx = pre.objectPoolList[i]; }
            //Hi Ex Bullet
            if (pre.objectPoolList[i].name == "bullet_lo_ex") { bulletLoEx = pre.objectPoolList[i]; }

            if (pre.objectPoolList[i].name == "bulletMachineGun(Clone)") { bulletMachineGun = pre.objectPoolList[i]; } //  Debug.Log("bulletMachineGun Found!!");
        }



        tempObject = GameObject.Find("A_GameWrapper");
        inp = tempObject.GetComponent<ScriptAInputManager>();

        gunFlare = transform.Find("gunDummy"); // for 'Quickly' lining up bullets with end of barrel - Use as Muzzle flash too
        gunFlare.renderer.enabled = false;


        gunFlareLeft = transform.Find("gunFlareLeft"); // for 'Quickly' lining up bullets with end of barrel - Use as Muzzle flash too
        gunFlareLeft.renderer.enabled = false;


        gunFlareRight = transform.Find("gunFlareRight"); // for 'Quickly' lining up bullets with end of barrel - Use as Muzzle flash too
        gunFlareRight.renderer.enabled = false;

        recoilStartPosition = transform.localPosition;


        gunView = GameObject.Find("GunView"); // for 'Quickly' lining up bullets with end of barrel - Use as Muzzle flash too

        //-------------------
        // Find Spark FX permitted spawn dummy objects (linked children) and delete 3DSMAX meshes - only want their transform positions
        //-------------------
        tempObject = GameObject.Find("innerA"); // for 'Quickly' lining up bullets with end of barrel - Use as Muzzle flash too

        i = 0;
        for (i = 0; i < tempObject.transform.childCount; i++)
        {
            Transform c = tempObject.transform.GetChild(i);
            string st = c.name;
            //if (st.Contains("spp")) { Destroy(c.renderer); Mesh mesh = c.GetComponent<MeshFilter>().mesh; DestroyImmediate(mesh); Debug.Log("found!"); }
            if (st.Contains("spp")) { Destroy(c.renderer); Destroy(c.GetComponent("MeshFilter")); }
        }
        //-------------------
        //-------------------

        //Find and get FX Manager - For sounds and any other custom visual FX

        tempObject = GameObject.Find("PlayerCamera");
        playerCamera = tempObject.GetComponent<Camera>(); //
        sfx = tempObject.GetComponent<FXManager>(); //


        // Make the rigid body not change rotation
        if (rigidbody)
            rigidbody.freezeRotation = true;
        originalRotation = playerCamera.transform.localRotation;



    }
    


    void Start()
    {

        shellCount = 0;

        machineGunBulletCount = 0;

        Get_Prefab_Pointers();

      

    }





    void Update()
    {

        // ------------------
        // Temporary MouseLook Controls
        // ------------------
         Do_MouseLook();




        //-----------------------
        // GUN TRIGGER
        //-----------------------

        if (inp.iFire)
        {
            if (Time.time > muzzleFlashTime)
            {
                Fire_Projectiles();

                switch (bulletID)
                {
                    case 0: // Standard

                        inp.iFire = false;

                        break;

                    case 1: // Machine gun - uses continuous press and requires different timer to reset bullet/input code code
                        if (Time.time > machineGunTime)
                        {
                            inp.iFire = false;
                        }
                    break;

                    case 2: // Machine gun - uses continuous press and requires different timer to reset bullet/input code code
                    if (Time.time > machineGunTime)
                    {
                        inp.iFire = false;
                    }
                    break;

                    default:

                    inp.iFire = false;

                    break;

                }
                
            }

            if (Time.time > shellFlashTime)
            {
                gunFlareLeft.renderer.enabled = false;
                gunFlareRight.renderer.enabled = false;
            }
        }
        else
        {
            if (Time.time > muzzleFlashTime)
            {
                gunFlare.renderer.enabled = false;
            }

            if (Time.time > shellFlashTime)
            {
                gunFlareLeft.renderer.enabled = false;
                gunFlareRight.renderer.enabled = false;
            }

            // LOOPING SFX needs switching off
            if (sfxShootLoop)
            {
                //Debug.Log("Off!");
                Stop_Looping_SFX();
            }
        }



        //-----------------------
        // GUN RECOIL
        //-----------------------

        if (recoilTrigger) // Set on new bullet creation
        {
            Do_Gun_Recoil();
        }

        if (shellCount > 0)
        {
            Do_Shells();
        }

             
    }




    //-------------------
    // Stop Looping Sound
    //-------------------
    void Stop_Looping_SFX()
    {

        sfx.Stop_Looping_Sound(sfxArrayIndex);

        sfxArrayIndex = -1;

        sfxShootLoop = false;
    }






    //-------------------
    // CASE SELECT FOR ALL BULLETS BEHAVIOURS 
    //-------------------

    private float tempScale;
    float machineGunTime;
    void Fire_Projectiles()
    {

        bulletID = inp.bulletID; // Bullet ID's are set in inputs script upon weapon swap



        switch (bulletID)
        {



            case 0:  // STANDARD BULLET GUN

            
                if (!recoilTrigger)
                {
                    recoilTrigger = true; // Get Recoil going
                }

                // MUZZLE FLASH
                gunFlare.renderer.enabled = true; // Show Standard  Muzzle Flash
                muzzleFlashTime = Time.time + 0.05f;

                gunFlare.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                tempScale = Random.Range(0.8F, 1.2F);
                gunFlare.transform.localScale = new Vector3(tempScale, tempScale, tempScale);


                // MAKE BULLET
                 bullet = ObjectPoolManager.CreatePooled(bulletMachineGun, new Vector3(0, 0, 0), Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                 bullet.SetActive(true);
                    //bullet.renderer.enabled = true;

                    // GET SCRIPT TO SET IMPORTANT VARIABLES
                    bulletScript = bullet.GetComponent<ScriptBullet>();// as ScriptBullet;
                    bulletScript.enabled = true;
                    bulletScript.bulletSpeed = 1000f;

                    if (inp.tapObject)
                    {
                        
                        bulletScript.bulletID = bulletID;
                        bulletScript.bulletHitTarget = inp.tapObject;
                        bulletLife = (inp.tapObjectDistance / bulletScript.bulletSpeed) / 100f;
                        bulletScript.bulletLife = Time.time + bulletLife * 95f;// This makes sure FX are place before Target impact area
                        inp.tapObjectDamageDelay = bulletScript.bulletLife; // Return this value so that the 'target object' knows when to apply damage to itself
                        bulletScript.bulletHitValue = inp.Set_Bullet_Damage(); // Inputs script has enum list for all bullet values
                        bulletScript.bulletHitTargetTag = inp.tapObjectTag; // Inputs script gets tag of RayCast hit
                        //Debug.Log(" bulletScript.bulletLife" + (bulletScript.bulletLife- Time.time));
                        //inp.tapObject = null; // Reset this value in this case as the key will be pressed & held down so won't reset itself in the input script function


                    }
                    else
                    {
                        bulletScript.bulletLife = Time.time + 2f; // Flying into the clouds 
                    }

                //POSITION ON THE END OF THE GUN BARREL
                bullet.transform.rotation = gunFlare.transform.rotation;
                bullet.transform.position = gunFlare.transform.position;

                inp.Update_Current_Ammo(1); // Subtract from current Ammo Supply

                //  FINALLY - PLAY SFX;
                sfx.PlaySound(21, Vector3.zero); // Check in Editor for ID numbers - for now 1 is BulletDefault Sound



                break;



            case 1: // MACHINE GUN


                if (!recoilTrigger)
                {

                    recoilTrigger = true; // Get Recoil going

                    // SFX machine gun loop

                    if (!sfxShootLoop)
                    {
                        sfxID = 13;

                        sfxArrayIndex = sfx.PlayLoopingSoundWithDistance(sfxID, transform.position, 1f); // Looping MACHINE GUN BURST

                        if (sfxArrayIndex > 0) { sfxShootLoop = true; } // Only if return value is within Audio Source Array limits


                    }

                    else
                    {

                        if (sfxID != 13)
                        {
                            Stop_Looping_SFX();

                            sfxID = 13;

                            sfxArrayIndex = sfx.PlayLoopingSoundWithDistance(sfxID, transform.position, 1f); // Looping MACHINE GUN BURST

                            if (sfxArrayIndex > 0) { sfxShootLoop = true; } // Only if return value is within Audio Source Array limits

                        }


                    }





                    // MUZZLE FLASH
                    gunFlare.renderer.enabled = true; // Show Standard  Muzzle Flash

                    muzzleFlashTime = Time.time + 0.05f; // Time to show Muzzle flash frame

                    machineGunTime = Time.time + 0.1f; // Delay for each new bullet - muzzleflash is too fast

                    gunFlare.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                    tempScale = Random.Range(0.8F, 1.2F);
                    gunFlare.transform.localScale = new Vector3(tempScale, tempScale, tempScale);




                    machineGunBulletCount++;

                    // MAKE BULLET
                    bullet = ObjectPoolManager.CreatePooled(bulletMachineGun, new Vector3(0, 0, 0), Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                    bullet.SetActive(true);
                    //bullet.renderer.enabled = true;

                    // GET SCRIPT TO SET IMPORTANT VARIABLES
                    bulletScript = bullet.GetComponent<ScriptBullet>();// as ScriptBullet;
                    bulletScript.enabled = true;
                    bulletScript.bulletSpeed = 1000f;

                    if (inp.tapObject)
                    {
                        bulletScript.bulletID = bulletID;
                        bulletScript.bulletHitTarget = inp.tapObject;
                        bulletLife = (inp.tapObjectDistance / bulletScript.bulletSpeed) / 100f;
                        bulletScript.bulletLife = Time.time + bulletLife * 95f;// This makes sure FX are place before Target impact area
                        inp.tapObjectDamageDelay = bulletScript.bulletLife; // Return this value so that the 'target object' knows when to apply damage to itself
                        bulletScript.bulletHitValue = inp.Set_Bullet_Damage(); // Inputs script has enum list for all bullet values
                        bulletScript.bulletHitTargetTag = inp.tapObjectTag; // Inputs script gets tag of RayCast hit
                         
                        //inp.tapObject = null; // Reset this value in this case as the key will be pressed & held down so won't reset itself in the input script function

                       
                    }
                    else
                    {
                        bulletScript.bulletLife = Time.time + .8f; // Flying into the clouds 
                    }

                    //POSITION ON THE END OF THE GUN BARREL
                    bullet.transform.rotation = gunFlare.transform.rotation;
                    bullet.transform.position = gunFlare.transform.position;


                    inp.Update_Current_Ammo(1); // Subtract from current Ammo Supply
                }

               
                 

                //  FINALLY - PLAY SFX;
                //sfx.PlaySound(1,Vector3.zero); // Check in Editor for ID numbers - for now 1 is BulletDefault Sound


                break;

          
            case 2: // HEAVY MACHINE GUN - with tracer bullets


                if (!recoilTrigger)
                {

                    recoilTrigger = true; // Get Recoil going

                    // SFX machine gun loop

                    if (!sfxShootLoop)
                    {
                        sfxID = 11;

                        sfxArrayIndex = sfx.PlayLoopingSoundWithDistance(sfxID, transform.position, 1f); // Looping MACHINE GUN BURST

                        if (sfxArrayIndex > 0) { sfxShootLoop = true; } // Only if return value is within Audio Source Array limits


                    }

                    else
                    {

                        if (sfxID != 11)
                        {
                            Stop_Looping_SFX();

                            sfxID = 11;

                            sfxArrayIndex = sfx.PlayLoopingSoundWithDistance(sfxID, transform.position, 1f); // Looping MACHINE GUN BURST

                            if (sfxArrayIndex > 0) { sfxShootLoop = true; } // Only if return value is within Audio Source Array limits

                        }


                    }





                    // MUZZLE FLASH
                    gunFlare.renderer.enabled = true; // Show Standard  Muzzle Flash

                    muzzleFlashTime = Time.time + 0.05f; // Time to show Muzzle flash frame

                    machineGunTime = Time.time + 0.1f; // Delay for each new bullet - muzzleflash is too fast

                    gunFlare.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                    tempScale = Random.Range(0.8F, 1.2F);
                    gunFlare.transform.localScale = new Vector3(tempScale, tempScale, tempScale);




                    machineGunBulletCount++;

                    // MAKE BULLET
                    bullet = ObjectPoolManager.CreatePooled( bulletTracer, new Vector3(0, 0, 0), Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                    bullet.SetActive(true);
                    //bullet.renderer.enabled = true;

                    // GET SCRIPT TO SET IMPORTANT VARIABLES
                    bulletScript = bullet.GetComponent<ScriptBullet>();// as ScriptBullet;
                    bulletScript.enabled = true;
                    bulletScript.bulletSpeed = 700f;

                    if (inp.tapObject)
                    {
                        bulletScript.bulletID = bulletID;
                        bulletScript.bulletHitTarget = inp.tapObject;
                        bulletLife = (inp.tapObjectDistance / bulletScript.bulletSpeed) / 100f;
                        bulletScript.bulletLife = Time.time + bulletLife * 95f;// This makes sure FX are place before Target impact area
                        inp.tapObjectDamageDelay = bulletScript.bulletLife; // Return this value so that the 'target object' knows when to apply damage to itself
                        bulletScript.bulletHitValue = inp.Set_Bullet_Damage(); // Inputs script has enum list for all bullet values
                        bulletScript.bulletHitTargetTag = inp.tapObjectTag; // Inputs script gets tag of RayCast hit

                        //inp.tapObject = null; // Reset this value in this case as the key will be pressed & held down so won't reset itself in the input script function


                    }
                    else
                    {
                        bulletScript.bulletLife = Time.time + .8f; // Flying into the clouds 
                    }

                    //POSITION ON THE END OF THE GUN BARREL
                    bullet.transform.rotation = gunFlare.transform.rotation;
                    bullet.transform.position = gunFlare.transform.position;


                    inp.Update_Current_Ammo(1); // Subtract from current Ammo Supply
                }




                //  FINALLY - PLAY SFX;
                //sfx.PlaySound(1,Vector3.zero); // Check in Editor for ID numbers - for now 1 is BulletDefault Sound


                break;

            case 3: // LOW EXPLOSIVE SHELLS


                 if (!recoilTrigger)
                {
                    recoilTrigger = true; // Get Recoil going
                }

                // MUZZLE FLASH
                 if (!gunFlareToggle) // LEFT
                 {
                     gunFlareLeft.renderer.enabled = true; // Show Standard  Muzzle Flash
                     gunFlareLeft.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                    shellFlashTime = Time.time + 0.05f;
                 }
                 else                // RIGHT
                 {
                     gunFlareRight.renderer.enabled = true; // Show Standard  Muzzle Flash
                     gunFlareRight.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                     shellFlashTime = Time.time + 0.05f;

                 }

               

                // MAKE BULLET
                bullet = ObjectPoolManager.CreatePooled(bulletDefault, new Vector3(0, 0, 0), Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                bullet.SetActive(true);
                bullet.renderer.enabled = true;

                // GET SCRIPT TO SET IMPORTANT VARIABLES
                bulletScript = bullet.GetComponent<ScriptBullet>();// as ScriptBullet;
                bulletScript.enabled = true;
                bulletScript.bulletSpeed = 300f;

                if (inp.tapObject)
                {
                    bulletScript.bulletID = bulletID;
                    bulletScript.bulletHitTarget = inp.tapObject;
                    bulletLife = (inp.tapObjectDistance / bulletScript.bulletSpeed) / 100f;
                    bulletScript.bulletLife = Time.time + bulletLife * 95f; // This makes sure FX are place before Target impact area
                    inp.tapObjectDamageDelay = bulletScript.bulletLife; // Return this value so that the 'target object' knows when to apply damage to itself
                    bulletScript.bulletHitValue = inp.Set_Bullet_Damage(); // Inputs script has enum list for all bullet values
                    bulletScript.bulletHitTargetTag = inp.tapObjectTag; // Inputs script gets tag of RayCast hit
                }
                else
                {
                    bulletScript.bulletLife = Time.time + 1.5f; // Flying into the clouds 
                }


                // MUZZLE FLASH
                if (!gunFlareToggle) // LEFT
                {
                    //POSITION ON THE END OF THE GUN BARREL
                    bullet.transform.rotation = gunFlareLeft.transform.rotation;
                    bullet.transform.position = gunFlareLeft.transform.position;
                    gunFlareToggle = true;
                }
                else                // RIGHT
                {
                    //POSITION ON THE END OF THE GUN BARREL
                    bullet.transform.rotation = gunFlareRight.transform.rotation;
                    bullet.transform.position = gunFlareRight.transform.position;
                    gunFlareToggle = false;

                }

                

                inp.Update_Current_Ammo(1); // Subtract from current Ammo Supply

                //  FINALLY - PLAY SFX;
                sfx.PlaySound(22, Vector3.zero); // Check in Editor for ID numbers - for now 1 is BulletDefault Sound


                break;



            case 4: // HIGH EXPLOSIVE SHELLS

                if (!recoilTrigger)
                {
                    recoilTrigger = true; // Get Recoil going
                }

               
                
                    gunFlareLeft.renderer.enabled = true; // Show Standard  Muzzle Flash
                    gunFlareLeft.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                    gunFlareRight.renderer.enabled = true; // Show Standard  Muzzle Flash
                    gunFlareRight.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                   
                    shellFlashTime = Time.time + 0.05f;

                   // 2 EXPLOSIVE BULLETS!!

                    for (i = 0; i < 2; i++)
                    {

                        // MAKE BULLET
                        bullet = ObjectPoolManager.CreatePooled(bulletDefault, new Vector3(0, 0, 0), Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                        bullet.SetActive(true);
                        bullet.renderer.enabled = true;

                        // GET SCRIPT TO SET IMPORTANT VARIABLES
                        bulletScript = bullet.GetComponent<ScriptBullet>();// as ScriptBullet;
                        bulletScript.enabled = true;
                        bulletScript.bulletSpeed = 300f;

                        if (inp.tapObject)
                        {
                            bulletScript.bulletID = bulletID;
                            bulletScript.bulletHitTarget = inp.tapObject;
                            bulletLife = (inp.tapObjectDistance / bulletScript.bulletSpeed) / 100f;
                            bulletScript.bulletLife = Time.time + bulletLife * 95f; // This makes sure FX are place before Target impact area
                            inp.tapObjectDamageDelay = bulletScript.bulletLife; // Return this value so that the 'target object' knows when to apply damage to itself
                            bulletScript.bulletHitValue = inp.Set_Bullet_Damage(); // Inputs script has enum list for all bullet values
                            bulletScript.bulletHitTargetTag = inp.tapObjectTag; // Inputs script gets tag of RayCast hit
                        }
                        else
                        {
                            bulletScript.bulletLife = Time.time + 1.5f; // Flying into the clouds 
                        }


                        // MUZZLE FLASH
                        if (i == 0) // LEFT
                        {
                            //POSITION ON THE END OF THE GUN BARREL
                            bullet.transform.rotation = gunFlareLeft.transform.rotation;
                            bullet.transform.position = gunFlareLeft.transform.position;
                            gunFlareToggle = true;
                        }
                        else                // RIGHT
                        {
                            //POSITION ON THE END OF THE GUN BARREL
                            bullet.transform.rotation = gunFlareRight.transform.rotation;
                            bullet.transform.position = gunFlareRight.transform.position;
                            gunFlareToggle = false;

                        }

                       

                    }

                inp.Update_Current_Ammo(1); // Subtract from current Ammo Supply

                //  FINALLY - PLAY SFX;
                sfx.PlaySound(23, Vector3.zero); // Check in Editor for ID numbers - for now 1 is BulletDefault Sound


                break;

            default: // Standard bullets

                if (!recoilTrigger)
                {
                    recoilTrigger = true; // Get Recoil going
                }

                // MUZZLE FLASH
                gunFlare.renderer.enabled = true; // Show Standard  Muzzle Flash
                muzzleFlashTime = Time.time + 0.05f;

                gunFlare.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0F, 359.0F));
                tempScale = Random.Range(0.8F, 1.2F);
                gunFlare.transform.localScale = new Vector3(tempScale, tempScale, tempScale);


                // MAKE BULLET
                bullet = ObjectPoolManager.CreatePooled(bulletDefault, new Vector3(0, 0, 0), Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
                bullet.SetActive(true);
                bullet.renderer.enabled = true;

                // GET SCRIPT TO SET IMPORTANT VARIABLES
                bulletScript = bullet.GetComponent<ScriptBullet>();// as ScriptBullet;
                bulletScript.enabled = true;
                bulletScript.bulletSpeed = 300f;

                if (inp.tapObject)
                {
                    bulletScript.bulletHitTarget = inp.tapObject;
                    bulletScript.bulletLife = Time.time + (inp.tapObjectDistance / bulletScript.bulletSpeed);
                    inp.tapObjectDamageDelay = bulletScript.bulletLife; // Return this value so that the 'target object' knows when to apply damage to itself
                    bulletScript.bulletHitValue = inp.Set_Bullet_Damage(); // Inputs script has enum list for all bullet values
                    bulletScript.bulletHitTargetTag = inp.tapObjectTag; // Inputs script gets tag of RayCast hit
                }
                else
                {
                    bulletScript.bulletLife = Time.time + 2f; // Flying into the clouds 
                }

                //POSITION ON THE END OF THE GUN BARREL
                bullet.transform.rotation = gunFlare.transform.rotation;
                bullet.transform.position = gunFlare.transform.position;

                inp.Update_Current_Ammo(1); // Subtract from current Ammo Supply

                //  FINALLY - PLAY SFX;
                sfx.PlaySound(1, Vector3.zero); // Check in Editor for ID numbers - for now 1 is BulletDefault Sound



                break;

        }
    }



   



    //-------------------
    //PROCESS GUN RECOIL
    //-------------------

    void Do_Gun_Recoil()
    {

        switch (bulletID)
        {

            case 1: // MACHINE GUN

                // Simple Recoil - split into 3 stages.. Initialize,  'Back' and then 'Forward' recoil

                switch (recoilStage)
                {
                    case 0: // START - Recoil quickly back

                        recoilValue = 0.08f;
                        recoilTime = 0f;
                        recoilTargetPosition = recoilStartPosition;
                        recoilStage = 1;
                        break;

                    case 1:

                        if (recoilTime < 90f)
                        {
                            recoilTime = recoilTime + 2440f * Time.deltaTime; ;

                            if (recoilTime >= 90f)
                            {
                                recoilTime = 90f;

                            }

                            float tempTime = Mathf.Sin(recoilTime * Mathf.Deg2Rad) * recoilValue;

                            recoilTargetPosition.z = recoilStartPosition.z - tempTime;

                            if (recoilTargetPosition.z < (recoilStartPosition.z - recoilValue))
                            {
                                recoilTargetPosition.z = recoilStartPosition.z - recoilValue;
                            }

                            transform.localPosition = recoilTargetPosition;

                            if (recoilTime == 90f)
                            {
                                //recoilTime = 0f;
                                recoilStage = 2;

                                Make_Empty_Shell();
                            }


                            if (Time.time > muzzleFlashTime)
                            {
                                gunFlare.renderer.enabled = false;
                            }

                        }

                        break;


                    case 2: // END - Reset slighty slower than recoil back


                        if (recoilTime > 0f)
                        {

                            recoilTime = recoilTime - 1440f * Time.deltaTime; ;

                            if (recoilTime <= 0f)
                            {
                                recoilTime = 0f;
                            }

                            float tempTime = Mathf.Sin(recoilTime * Mathf.Deg2Rad) * recoilValue;

                            recoilTargetPosition.z = recoilStartPosition.z - tempTime;

                            if (recoilTargetPosition.z > recoilStartPosition.z)
                            {
                                recoilTargetPosition.z = recoilStartPosition.z;
                            }

                            transform.localPosition = recoilTargetPosition;

                            if (recoilTime == 0f)
                            {
                                recoilTrigger = false; // Reset Recoil
                                recoilStage = 0;
                            }

                            if (Time.time > muzzleFlashTime)
                            {
                                gunFlare.renderer.enabled = false;
                            }

                        }


                        break;
                }



                break;

            case 2: // HEAVY MACHINE GUN - tracer bullets



                switch (recoilStage)
                {
                    case 0: // START - Recoil quickly back

                        recoilValue = 0.08f;
                        recoilTime = 0f;
                        recoilTargetPosition = recoilStartPosition;
                        recoilStage = 1;
                        break;

                    case 1:

                        if (recoilTime < 90f)
                        {
                            recoilTime = recoilTime + 2440f * Time.deltaTime; ;

                            if (recoilTime >= 90f)
                            {
                                recoilTime = 90f;

                            }

                            float tempTime = Mathf.Sin(recoilTime * Mathf.Deg2Rad) * recoilValue;

                            recoilTargetPosition.z = recoilStartPosition.z - tempTime;

                            if (recoilTargetPosition.z < (recoilStartPosition.z - recoilValue))
                            {
                                recoilTargetPosition.z = recoilStartPosition.z - recoilValue;
                            }

                            transform.localPosition = recoilTargetPosition;

                            if (recoilTime == 90f)
                            {
                                //recoilTime = 0f;
                                recoilStage = 2;

                                Make_Empty_Shell();
                            }


                            if (Time.time > muzzleFlashTime)
                            {
                                gunFlare.renderer.enabled = false;
                            }

                        }

                        break;


                    case 2: // END - Reset slighty slower than recoil back




                        if (recoilTime > 0f)
                        {

                            recoilTime = recoilTime - 1440f * Time.deltaTime; ;

                            if (recoilTime <= 0f)
                            {
                                recoilTime = 0f;
                            }

                            float tempTime = Mathf.Sin(recoilTime * Mathf.Deg2Rad) * recoilValue;

                            recoilTargetPosition.z = recoilStartPosition.z - tempTime;

                            if (recoilTargetPosition.z > recoilStartPosition.z)
                            {
                                recoilTargetPosition.z = recoilStartPosition.z;
                            }

                            transform.localPosition = recoilTargetPosition;

                            if (recoilTime == 0f)
                            {
                                recoilTrigger = false; // Reset Recoil
                                recoilStage = 0;
                            }

                            if (Time.time > muzzleFlashTime)
                            {
                                gunFlare.renderer.enabled = false;
                            }

                        }


                        break;
                }


                


                break;

            //case 3: // etc...

               // break;

            default: // Standard bullets

                // Simple Recoil - split into 3 stages.. Initialize,  'Back' and then 'Forward' recoil

               

                switch (recoilStage)
                {
                    case 0: // START - Recoil quickly back

                        recoilValue = 0.08f;
                        recoilTime = 0f;
                        recoilTargetPosition = recoilStartPosition;
                        recoilStage = 1;
                        break;

                    case 1:

                        if (recoilTime < 90f)
                        {
                            recoilTime = recoilTime + 1440f * Time.deltaTime; ;

                            if (recoilTime >= 90f)
                            {
                                recoilTime = 90f;

                            }

                            float tempTime = Mathf.Sin(recoilTime * Mathf.Deg2Rad) * recoilValue;

                            recoilTargetPosition.z = recoilStartPosition.z - tempTime;

                            if (recoilTargetPosition.z < (recoilStartPosition.z - recoilValue))
                            {
                                recoilTargetPosition.z = recoilStartPosition.z - recoilValue;
                            }

                            transform.localPosition = recoilTargetPosition;

                            if (recoilTime == 90f)
                            {
                                //recoilTime = 0f;
                                recoilStage = 2;
                            }

                        }

                        break;


                    case 2: // END - Reset slighty slower than recoil back




                        if (recoilTime > 0f)
                        {

                            recoilTime = recoilTime - 720f * Time.deltaTime; ;

                            if (recoilTime <= 0f)
                            {
                                recoilTime = 0f;
                            }

                            float tempTime = Mathf.Sin(recoilTime * Mathf.Deg2Rad) * recoilValue;

                            recoilTargetPosition.z = recoilStartPosition.z - tempTime;

                            if (recoilTargetPosition.z > recoilStartPosition.z)
                            {
                                recoilTargetPosition.z = recoilStartPosition.z;
                            }

                            transform.localPosition = recoilTargetPosition;

                            if (recoilTime == 0f)
                            {
                                recoilTrigger = false; // Reset Recoil
                                recoilStage = 0;
                              
                            }

                        }


                        break;
                }


                break;


        }
    }




    //-------------
    // EMPTY SHELLS
    //--------------

    void Make_Empty_Shell()
    {


        for (i = 0; i < 5; i++)
        {

            if (SHELL_DATA[i, 0] == 0) // Set boolean(float) value to active
            {

               

                if (bulletID == 1) // STANDARD MACHINE GUN
                {


                    // Bool, Time, Time step, X,Y,Z scales, X, Y, Z positions

                    SHELL_DATA[i, 0] = 1.0f; // bool is on
                    SHELL_DATA[i, 1] = 0f; // reset Time 
                    SHELL_DATA[i, 2] = Random.Range(1f, 1.5f);// X scale
                    SHELL_DATA[i, 3] = Random.Range(0.85f, 1.3f);// Y scale
                    SHELL_DATA[i, 4] = Random.Range(-0.05f, 0.1f);// Z scale
                    SHELL_ARRAY[i].renderer.enabled = true;
                }
                else
                {
                    // Bool, Time, Time step, X,Y,Z scales, X, Y, Z positions

                    SHELL_DATA[i, 0] = 2.0f; // bool is on
                    SHELL_DATA[i, 1] = 0f; // reset Time 
                    SHELL_DATA[i, 2] = Random.Range(1f, 1.5f);// X scale
                    SHELL_DATA[i, 3] = Random.Range(0.85f, 1.3f);// Y scale
                    SHELL_DATA[i, 4] = Random.Range(-0.05f, 0.1f);// Z scale
                    TSHELL_ARRAY[i].renderer.enabled = true;
                }

                shellCount++;

                return;
            }

        }



    }


    //-------------
    // TRANSFORM THE EMPTY SHELLS
    //--------------

    void Do_Shells()
    {

        for (i = 0; i < 5; i++)
        {
            // Bool, Time, Time step, X,Y,Z scales, X, Y, Z positions
            //   0      1      2      3 4 5         6  7  8
            if (SHELL_DATA[i, 0] == 1) // MACHINE GUN value active
            {

                if (SHELL_DATA[i, 1] < 180f)
                {
                    SHELL_DATA[i, 1] = SHELL_DATA[i, 1] + 360f * Time.deltaTime;

                    SHELL_DATA[i, 5] = Mathf.Sin(SHELL_DATA[i, 1] * Mathf.Deg2Rad); // Perform Sine Calc only once

                    SHELL_DATA[i, 6] = SHELL_DATA[i, 5] * SHELL_DATA[i, 3]; // Y // Pass Sine value to other axis'
                    SHELL_DATA[i, 7] = SHELL_DATA[i, 5] * SHELL_DATA[i, 4]; // Z
                    SHELL_DATA[i, 5] = SHELL_DATA[i, 5] * SHELL_DATA[i, 2]; // X
                    
                    SHELL_ARRAY[i].transform.Translate(new Vector3(SHELL_DATA[i, 5], SHELL_DATA[i, 6], SHELL_DATA[i, 7]) * Time.deltaTime);
                    SHELL_ARRAY[i].Rotate(Vector3.right * -270f * Time.deltaTime);

                }

                else
                {
                    // Reset MACHINE GUN
                    SHELL_DATA[i, 0] = 0;// Reset Active trigger
                    SHELL_ARRAY[i].transform.localPosition = shellStartPosition;
                    SHELL_ARRAY[i].transform.localRotation = Quaternion.identity;
                    SHELL_ARRAY[i].renderer.enabled = false;

                    shellCount--;
                }


            }
            else
            {

                if (SHELL_DATA[i, 0] == 2) //  HEAVY MACHINE GUN value active
                {

                    if (SHELL_DATA[i, 1] < 180f)
                    {
                        SHELL_DATA[i, 1] = SHELL_DATA[i, 1] + 360f * Time.deltaTime;

                        SHELL_DATA[i, 5] = Mathf.Sin(SHELL_DATA[i, 1] * Mathf.Deg2Rad); // Perform Sine Calc only once

                        SHELL_DATA[i, 6] = SHELL_DATA[i, 5] * SHELL_DATA[i, 3]; // Y // Pass Sine value to other axis'
                        SHELL_DATA[i, 7] = SHELL_DATA[i, 5] * SHELL_DATA[i, 4]; // Z
                        SHELL_DATA[i, 5] = SHELL_DATA[i, 5] * SHELL_DATA[i, 2]; // X

                        TSHELL_ARRAY[i].transform.Translate(new Vector3(SHELL_DATA[i, 5], SHELL_DATA[i, 6], SHELL_DATA[i, 7]) * Time.deltaTime);
                        TSHELL_ARRAY[i].Rotate(Vector3.right * -270f * Time.deltaTime);

                    }

                    else
                    {
                        // Reset MACHINE GUN TRACERS
                        SHELL_DATA[i, 0] = 0;// Reset Active trigger
                        TSHELL_ARRAY[i].transform.localPosition = shellStartPosition;
                        TSHELL_ARRAY[i].transform.localRotation = Quaternion.identity;
                        TSHELL_ARRAY[i].renderer.enabled = false;

                        shellCount--;
                    }


                }


            }


        }


    }
       
          


       




    



























    //-------------------
    // MOUSE CONTROLS BELOW HERE
    //-------------------
    void Do_MouseLook()
    {

        //if (Input.GetMouseButton(0))
        //{
        if (axes == RotationAxes.MouseXAndY)
        {
            // Read the mouse input axis
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;

            rotationX = ClampAngle(rotationX, minimumX, maximumX);
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);

            playerCamera.transform.localRotation = originalRotation * xQuaternion * yQuaternion;
        }
        else if (axes == RotationAxes.MouseX)
        {
            rotationX += Input.GetAxis("Mouse X") * sensitivityX;
            rotationX = ClampAngle(rotationX, minimumX, maximumX);

            Quaternion xQuaternion = Quaternion.AngleAxis(rotationX, Vector3.up);
            playerCamera.transform.localRotation = originalRotation * xQuaternion;
        }
        else
        {
            rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
            rotationY = ClampAngle(rotationY, minimumY, maximumY);

            Quaternion yQuaternion = Quaternion.AngleAxis(rotationY, Vector3.left);
            playerCamera.transform.localRotation = originalRotation * yQuaternion;
        }
        //}
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


} // END BRACKET




















