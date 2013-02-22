
using UnityEngine;
using System.Collections;

#if !UNITY_WEBPLAYER
using PlayerPrefs = PreviewLabs.PlayerPrefs;
#endif


//  THE INPUT MANAGER SCRIPT WILL HANDLE ALL THE GAME STATES
//  USING AN ENUMERATOR - THE REASON IS THIS SCRIPT IS EASIER TO
//  CALL, AS ALL OTHER SCRIPTS ARE ALREADY SET TO CALL THE INPUT MANAGER
//  USING THE 'inp.' PREFIX. THIS ALSO SAVES ON HAVING TO CALL YET ANOTHER
//  DIFFERENT SCRIPT IN ORDER TO ACCESS PUBLIC METHODS OR VARIABLES
//  WHICH IS GETTING MESSY ENOUGH ALREADY. Just as much hassle using Delegates & Listeners

// ALSO NOW ONLY USING PREVIEW LABS PLAYER PREFS SAVE NAMESPACE AS IT IS SUPERIOR TO
// THE STANDARD UNITY PLAYERPREFS CLASS.
// Use PlayerPrefs.Flush() to save all values. This can be done after writing a 
// bunch of data, or when the application is quit (in game):
// PlayerPrefs.Flush();




public class ScriptAInputManager : MonoBehaviour
{
    public bool debugOn = true;

    // PREFS & SAVE DATA
    public int levelNumber; // Gets read in and set at start of game or from Options Menu(Load Level Screen) & accessed from Scene Manager too
    private int maxLevelNumber;
    private string[] LEVELNAME;
    private int currentLevelNumber; // updates if a new level is loaded
    private int LevelNumber; // updates if a new level is loaded
    private int lastLevelNumber; // saves level on menu enter -in case 'Back To Game' Button is pressed it will prevent a new level loading
    private bool levelChanged; // Lets us know if level has been toggled off current level - allows the same level to be re-loaded
    private bool returnToGame; // Triggers the return to game process in the 'backFromMenu' switch() function
    public int score;
    public string playerName;

    private float playerHealth; // 
    private float playerLife; // = Health + Armour.. probably doesn't need to be public
    public float playerArmour;
   // private float playerDamage; // The damage value that was just inflicted

    private ScriptGunControls gunScript; // Access to ammo and current weapon
    private ScriptGameHUD HUD; // Hud script
    private FXManager sfx;  // For particles or sound  - has basic functions to control them

    public enum Ammo // PUBLIC Updated when weapon/Ammo changes by Gun Script
    {
        standard,
        machineGun,
        heavyMachineGun,
        expSmall,
        expLarge,
        hellfireMissile,
        incendaryMissile,
        bunkerMissile
    }

    public Ammo currentAmmo;
    private bool ammoZero; // Forces empty clip sound and no bullets

    private float currentAmmoAmount;
    private int currentAmmoMax;
    public int bulletID;
    public int hitDamage;

    private int standardAmmo;
    private int machineGunAmmo;
    private int heavyMachineGunAmmo;
    private int expSmallAmmo;
    private int expLargeAmmo;
    private int hellfireMissileAmmo;
    private int incendaryMissileAmmo;
    private int bunkerMissileAmmo;

    private int standardAmmoMax;
    private int machineGunAmmoMax;
    private int heavyMachineGunAmmoMax;
    private int expSmallAmmoMax;
    private int expLargeAmmoMax;
    private int hellfireMissileAmmoMax;
    private int incendaryMissileAmmoMax;
    private int bunkerMissileAmmoMax;
    

    public int stage; //This is a phase, or position counter for STATE/stage processes - better than Coroutines & more usable
    public float stageTime; //Timer for STATE/Stages

    //STATE
    public enum State // Overall states for the whole game
    {
        start,
        menu,
        game,
        gameOver,
        quit,
        end,
    }
    public State game;
    public float gameStateTime;



    // LANGUAGE
    public enum Language // Lanaguages for different locales
    {
        english,
        french,
        german,
        spanish,
        eskimo,
        drunk,
    }
    public Language lang;




    // GLOBAL INPUT VARIABLES
    public bool iDown;
    public bool iUp;
    public bool iLeft;
    public bool iRight;
    public bool iFire;
    public bool iUse;
    public bool iToggleLeft;
    public bool iToggleRight;
    public bool iMenu;
    public bool iQuit;
    public bool iPause;
    //public bool iCameraIn;
    //public bool iCameraOut;
    //public bool ICameraSmartOn;


    // GLOBAL KEY BINDING VARIABLES (mainly for computers not mobile)
    public KeyCode bIDown;
    public KeyCode bIUp;
    public KeyCode bILeft;
    public KeyCode bIRight;
    public KeyCode bIFire;
    public KeyCode bIUse;
    public KeyCode bIToggleLeft;
    public KeyCode bIToggleRight;
    public KeyCode bIMenu;
    public KeyCode bIQuit;
    public KeyCode bIPause;
    public KeyCode bICameraIn;
    public KeyCode bICameraOut;
    public KeyCode bICameraSmartOn;

    // DEBUG!!! for Editor
    public KeyCode bToggleWeaponUp;
    public KeyCode bToggleWeaponDown;
   
    // INT CODE FOR PLATFORM
    public int hardware;

    // DIRECTORY  PATH SPECIFICS
    public string pathToResourceFolder;
    public string pathSlash;
    public string pathSlashEnd;



    // IOS & ANDROID & ALL RAY CASTS
    private string iOSString;
    private Ray iOSRay;
    public RaycastHit iOSRayHit; // Will be read from MENU and HUD Scripts
    private LayerMask iOSlayerMask;
    public Transform tapObject; // the object a screen tap has hit
    public Vector3 tapObjectPosition; // the position
    public float tapObjectDistance; // So bullets can calculate their life without the need for real collisions
    public string tapObjectTag; // Unity editor tag for object type..such as 'enemy', 'building' etc...
    public float tapObjectDamageDelay; //Delay for when 'Bullet' is supposed ot hit target - 
    private Vector2 screenCentre; // for ray casts from Camera

    //------------------------------------------
    //------------------------------------------



    //***********************		
    void Awake()
    {
        //--------------------
        // GET IT ROLLING
        //--------------------
        game = State.start;
     

        Get_Screen_Centre();// Doesn't have to be called here - but right now there is no menu system.. for Gunsight ray casts

        stage = 0; // start stage from zero in all 'STATES' so can then be incremented at each game state
        stageTime = Time.time + 1.5F; // Pause for no other reason than I like the sound of silence
        backFromGame = false; // Gets set from HUD subMenu when returning to Main Menu;

        maxLevelNumber = 5;
        LEVELNAME = new string[maxLevelNumber];

        Set_Level_Names();


        if (Application.platform == RuntimePlatform.OSXEditor) { hardware = -1; if (debugOn) { Debug.Log(">>> OSX EDitor identified from: " + this); } }
        if (Application.platform == RuntimePlatform.WindowsEditor) { hardware = 0; if (debugOn) { Debug.Log(">>> Windows Editor identified from: " + this); } }
        if (Application.platform == RuntimePlatform.IPhonePlayer) { hardware = 1; if (debugOn) { Debug.Log(">>> IPhone Player identified from: " + this); } }
        if (Application.platform == RuntimePlatform.OSXPlayer) { hardware = 2; if (debugOn) { Debug.Log(">>> Apple Mac Player identified from: " + this); } }
        if (Application.platform == RuntimePlatform.OSXWebPlayer) { hardware = 3; if (debugOn) { Debug.Log(">>> Apple Mac 'Web' Player identified from: " + this); } }
        if (Application.platform == RuntimePlatform.WindowsPlayer) { hardware = 4; if (debugOn) { Debug.Log(">>> Windows Player identified from: " + this); } }
        if (Application.platform == RuntimePlatform.WindowsWebPlayer) { hardware = 5; if (debugOn) { Debug.Log(">>> Windows 'Web'Player identified from: " + this); } }
        if (Application.platform == RuntimePlatform.Android) { hardware = 6; if (debugOn) { Debug.Log(">>> Android Player identified from: " + this); } }
        if (Application.platform == RuntimePlatform.FlashPlayer) { hardware = 7; if (debugOn) { Debug.Log(">>> Flash Player identified from: " + this); } }

        //--------------------
        // PATHS
        //--------------------
        SetUp_Platform_Specifics(); // Path strings and stuff

        //--------------------
        // SET SCRIPT HANDLES
        //--------------------
        Set_Game_Object_Handles();

        //--------------------
        //HEALTH:
        //--------------------
         playerHealth =100f; // Updated from Helicopter script
         playerLife=150f; // = Health + Armour
         playerArmour = 50f; ;


         //--------------------
         // MAX AMMO/BULLET VALUES
         //--------------------
         standardAmmoMax = 99999;  // TEMPORARY VALUES!! just to get us going for Testing purposes
         machineGunAmmoMax = 400;
         heavyMachineGunAmmoMax = 400 ;
         expSmallAmmoMax = 100;
         expLargeAmmoMax = 100;
         hellfireMissileAmmoMax = 4 ;
         incendaryMissileAmmoMax = 4;
         bunkerMissileAmmoMax = 2;

         //--------------------
         // SET MAXIMUM PERMITTED BULLETS FOR NOW.. need to think about what we start with
         //--------------------
         standardAmmo = standardAmmoMax;
         machineGunAmmo= machineGunAmmoMax;
         heavyMachineGunAmmo=heavyMachineGunAmmoMax;
         expSmallAmmo=expSmallAmmoMax;
         expLargeAmmo=expLargeAmmoMax;
         hellfireMissileAmmo=hellfireMissileAmmoMax;
         incendaryMissileAmmo= incendaryMissileAmmoMax;
         bunkerMissileAmmo = bunkerMissileAmmoMax;


         currentAmmo = Ammo.standard; //
         currentAmmoAmount = standardAmmoMax; // Probably better to be get/set when loaded in player prefs in case of better Ammo level having been saved - but it's set here for now;
        


    }

    private GameObject mainMenuParent; //for deactivating the Main Menu Empty Game Object while the game is running..and its children

    private Camera playerCamera; // Main game camera
    private ScriptPlayerCamera cameraScript;
   
  


    // MENU OBJECTS FOR ACTIVATING / DEACTIVATING

    private GameObject NGUIRoot;
    private GameObject panelLoading;
    private GameObject panelQuitSure;
    private GameObject panelPressAnyKey;
    private GameObject panelTapScreen;
    private GameObject buttonBackToGame;

  
    //***********************		
    void Set_Game_Object_Handles()
    {

            Screen.showCursor = false;

            // SETUP NGUI STUFF

            //SetUP_NGUI();

            // FIND PLAYER CAMERA
            GameObject tempObject = GameObject.Find("PlayerCamera");
            playerCamera = tempObject.GetComponent<Camera>();
            cameraScript = tempObject.GetComponent<ScriptPlayerCamera>();
            sfx = tempObject.GetComponent<FXManager>(); // SOUND

            // FIND PLAYER GUN
            tempObject = GameObject.Find("Gun");
            gunScript = tempObject.GetComponent< ScriptGunControls>();

            // HUD
            HUD = GameObject.Find("UI Root (2D)").GetComponent("ScriptGameHUD") as ScriptGameHUD;

       
    }


    void Get_Screen_Centre() // Called best after menu is finished - this is useful for Ray Casts from Camera
    {
        //Vector2
        screenCentre.x = Screen.width / 2;
        screenCentre.y = Screen.height / 2;

    }




    //***********************		
    void Set_Level_Names()
    {
        LEVELNAME[0] = "L1: Art of Artery";
        LEVELNAME[1] = "L2: HappyEndix";
        LEVELNAME[2] = "L3: Gas of Naz :)";
        LEVELNAME[3] = "L4: Matt's Piles";
        LEVELNAME[4] = "L5: Mike's Mumps";

    }

    // CALLED FROM NGUI DELEGATE BUTTON LISTENER IN THIS CODE
    //***********************		
    private string Change_Level_Names()
    {
        string tempString = "";

        if (levelNumber == 1) { tempString = LEVELNAME[0]; }
        if (levelNumber == 2) { tempString = LEVELNAME[1]; }
        if (levelNumber == 3) { tempString = LEVELNAME[2]; }
        if (levelNumber == 4) { tempString = LEVELNAME[3]; }
        if (levelNumber == 5) { tempString = LEVELNAME[4]; }

        return tempString;
    }



    //***********************		
    void SetUp_Platform_Specifics()
    {



        switch (hardware)
        {
            case -1: // OSX UNITY EDITOR
                {

                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";



                }
                break;

            case 0: // WINDOWS UNITY EDITOR
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "//";

                    // INPUT_TOUCHES
                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                  
                    
                }
                break;

            case 1: // IPHONE
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";

                    // INPUT_TOUCHES

                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                    //Gesture.onLongTapE += OnLongTap;
                    //Gesture.onChargeEndE += OnChargeEnd;
                    //Gesture.onDraggingStartE += OnDraggingStart;
                    //Gesture.onDraggingE += OnDragging;
                    //Gesture.onDraggingEndE += OnDraggingEnd;

                }
                break;
            case 2: // APPLE MAC COMPUTER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";

                    // INPUT_TOUCHES
                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                }
                break;
            case 3: // APPLE MAC WEB PLAYER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";

                    // INPUT_TOUCHES
                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                }
                break;
            case 4: // WINDOWS PLAYER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "//";

                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                    //Gesture.onLongTapE += OnLongTap;
                    //Gesture.onChargingE += OnCharging;
                    //Gesture.onChargeEndE += OnChargeEnd;
                    //Gesture.onDraggingStartE += OnDraggingStart;
                    //Gesture.onDraggingE += OnDragging;
                    //Gesture.onDraggingEndE += OnDraggingEnd;
                }
                break;
            case 5: // WINDOWS 'WEB' PLAYER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "//";

                    // INPUT_TOUCHES
                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                    //Gesture.onLongTapE += OnLongTap;
                    //Gesture.onChargingE += OnCharging;
                    //Gesture.onChargeEndE += OnChargeEnd;
                    //Gesture.onDraggingStartE += OnDraggingStart;
                    //Gesture.onDraggingE += OnDragging;
                    //Gesture.onDraggingEndE += OnDraggingEnd;
                }
                break;
            case 6: // ANDROID
                {
                    pathToResourceFolder = "";
                    pathSlash = "/";

                    // INPUT_TOUCHES
                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                }
                break;
            case 7: // FLASH PLAYER
                {
                    pathToResourceFolder = "";
                    pathSlash = "/";

                    // INPUT_TOUCHES
                    Gesture.onMultiTapE += OnMultiTapE;
                    Gesture.onChargingE += OnPressed;
                }
                break;



        }



    }


    //***********************		
    void Start()
    {

        // SET AMMO & WEAPON VALUES
        Set_Current_Weapon(currentAmmo);

        ammoZero = false;

        // SET HEALTH BAR UP
        HUD.Do_Health_Bar(playerLife);  // HEALTH BAR UPDATE - script is on NGUI UI Root (2D)

        // SET AMMO BAR UP
        HUD.Do_Ammo_Bar(currentAmmoAmount, currentAmmoMax);  //Ammo BAR UPDATE - script is on NGUI UI Root (2D)

    }




    //***********************	
    // Steady Garbage collection

    public int frameFreq = 30;  

    void Update()
    {


        Game_Stages();

        Scan_Inputs();


        if (Time.frameCount % frameFreq == 0) { System.GC.Collect(); } 
    

    }

   





    //****************************************************************************************
    // MAIN GAME STUFF 
    //****************************************************************************************

    public bool backFromGame; // Gets set from HUD subMenu when returning to Main Menu;

    void Game_Stages() // Called from Update()
    {


        switch (game)
        {

            // GAME START SCREENS
            case State.start:
                {
                    Do_Game_Start();  // PROCESS THE START SCREENS - LOGO ETC..
                }
                break;

            // MAIN MENU
            case State.menu:
                {
                    if (backFromGame)
                    {
                        Do_Main_Menu_From_Game();  // PROCESS THE MAIN MENU - tweaked if coming back from game
                    }
                    else
                    {
                        Do_Main_Menu();  // PROCESS THE MAIN MENU
                    }


                }
                break;

            // GAME
            case State.game:
                {

                    Do_Game();  // PROCESS THE GAME


                }
                break;

            // SAVE & QUIT
            case State.quit:
                {

                    Do_Save_And_Quit();  // PROCESS THE GAME QUIT SEQUENCE
                    //Debug.Log (" +++++++++ Saved Player Data & Quit from: " + this.name);
                }
                break;



            // END OF GAME - if any
            case State.end:
                {

                }
                break;

        }

    }
    //****************************************************************************************
    //****************************************************************************************







    //SCAN CORRECT HARDWARE INPUTS
    //***********************		
    void Scan_Inputs()// Called from Update()
    {

        switch (hardware)
        {
            case -1: // OSX UNITY EDITOR
                {
                    Scan_AppleMac_Inputs();
                }
                break;

            case 0: // WINDOWS UNITY EDITOR
                {
                    Scan_Windows_Inputs();
                }
                break;

            case 1: // IPHONE
                {
                    Scan_Iphone_Inputs();
                }
                break;
            case 2: // APPLE MAC COMPUTER
                {
                    Scan_AppleMac_Inputs();
                }
                break;
            case 3: // APPLE MAC WEB PLAYER
                {
                    Scan_AppleMac_WebPlayer_Inputs();
                }
                break;
            case 4: // WINDOWS PLAYER
                {
                    Scan_Windows_Inputs();
                }
                break;
            case 5: // WINDOWS 'WEB' PLAYER
                {
                    Scan_Windows_WebPlayer_Inputs();
                }
                break;
            case 6: // ANDROID
                {
                    Scan_Windows_Android_Inputs();
                }
                break;
            case 7: // FLASH PLAYER
                {
                    Scan_Flash_Inputs();
                }
                break;

        }

    }







    //----------------------
    // HEALTH
    //----------------------

    public void Update_Player_Health(float damage)
    {
        // playerHealth; // 100 Updated from Helicopter script
        // playerLife; // = Health + Armour
        // playerArmour; //50
        // playerDamage; // The damage value that was just inflicted

        if (damage > 15) { };

        if (playerArmour > 100) { playerArmour = 100; } // Armour can be collected
        //Damage Saved from inputs script..gets set when raycast hits something
        if (playerArmour > 0)
        {
            playerArmour = playerArmour - damage;

            if (playerArmour > 1)
            {
                playerLife = (playerHealth + playerArmour);
            }
            else
            {
                playerArmour = 0;

                playerLife = playerLife - damage;
            }
        }
        else
        {
            playerLife = playerLife - damage;
        }



        //---------------------
        // GAME OVER!!!..Do yo thang from here on in
        if (playerLife < 1) { game = State.gameOver; } else { HUD.Do_Health_Bar(playerLife); } // HEALTH BAR UPDATE - script is on NGUI UI Root (2D)
        //---------------------

        //Debug.Log("HEALTH: " + playerLife);



    }



    //***********************			
    //CHECK FOR A TOUCH/TAP OF SCREEN using Input.touches
    //***********************	
	
    void OnMultiTapE(Tap tap)
    {




       
            if (Time.time > pressedTime)

            {

                if (currentAmmoAmount > 0)
                {



                    if (bulletID < 3) // Standard, machine gun & heavy machine gun only
                    {
                        pressedTime = Time.time + 0.1f; // DELAY FOR REPEAT FIRE
                    }
                    else
                    {
                        pressedTime = Time.time + 0.4f; // - this is only for non-machine gun weapons

                    }

                    if (iFire == false)
                    {
                        tapObject = null; //RESET always after fire is registered in whatever function/Code/ uses it!
                    }

                    iFire = true; //Tapped so FIRE! Will be Reset in Turret code

                    //do a raycast base on the position of the tap

                    Ray ray = playerCamera.ScreenPointToRay(screenCentre);

                    RaycastHit hit;

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        //if the tap lands on the shortTapObj, then shows the effect.
                        if (hit.collider.transform)
                        {
                            tapObject = hit.collider.transform; // RESET this PUBLIC value at top of function

                            tapObjectPosition = hit.point; // vector position

                            tapObjectDistance = Vector3.Distance(playerCamera.transform.position, tapObjectPosition);// Gun script can calculate life of bullet from distance and 'Fake' a collision

                            tapObjectTag = tapObject.tag; // tags are useful in some scripts

                            hitDamage = Set_Bullet_Damage(); //Set hit point for this Ammo


                        }
                    }
                }
                else
                {

                    sfx.PlaySound(27, Vector3.zero); // PLAY EMPTY CLIP
                }
            
        }
           
       
    }



    private float pressedTime;
    void OnPressed(ChargedInfo pressed)
    {

        // MACHINE GUN AUTO FIRE ONLY INPUT

        if (currentAmmoAmount > 0)
        {

            if (currentAmmo == Ammo.machineGun || currentAmmo == Ammo.heavyMachineGun)
            {

                tapObject = null; //RESET always in pressed input!

                if (!iFire)
                {



                    iFire = true; //Pressed so FIRE! Will be Reset in other (Gun) code

                    //do a raycast base on the position of the tap

                    Ray ray = playerCamera.ScreenPointToRay(screenCentre);

                    RaycastHit hit;

                    //pressedDelay = false;

                    if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                    {
                        //if the tap lands on the shortTapObj, then shows the effect.
                        if (hit.collider.transform)
                        {
                            tapObject = hit.collider.transform; // RESET this PUBLIC value at top of function

                            tapObjectPosition = hit.point; // vector position

                            tapObjectDistance = Vector3.Distance(playerCamera.transform.position, tapObjectPosition);// Gun script can calculate life of bullet from distance and 'Fake' a collision

                            tapObjectTag = tapObject.tag; // tags are useful in some scripts

                            hitDamage = Set_Bullet_Damage(); //Set hit point for this Ammo

                        }
                    }
                }
            }
            else
            {

                if (Time.time > pressedTime)
                {
                    pressedTime = Time.time + 0.33f; // DELAY FOR AUTOFIRE - this is only for non-machine gun weapons

                    if (!iFire)
                    {
                        //Debug.Log("SIngle shot");

                        tapObject = null; //RESET always in pressed input!


                        iFire = true; //Pressed so FIRE! Will be Reset in other (Gun) code

                        //do a raycast base on the position of the tap

                        Ray ray = playerCamera.ScreenPointToRay(screenCentre);

                        RaycastHit hit;

                        //pressedDelay = false;

                        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
                        {
                            //if the tap lands on the shortTapObj, then shows the effect.
                            if (hit.collider.transform)
                            {
                                tapObject = hit.collider.transform; // RESET this PUBLIC value at top of function

                                tapObjectPosition = hit.point; // vector position

                                tapObjectDistance = Vector3.Distance(playerCamera.transform.position, tapObjectPosition);// Gun script can calculate life of bullet from distance and 'Fake' a collision

                                tapObjectTag = tapObject.tag; // tags are useful in some scripts

                                hitDamage = Set_Bullet_Damage(); //Set hit point for this Ammo

                            }
                        }

                    }


                }

            }
        }
        else
        {
            if (Time.time > pressedTime)
            {
                if (bulletID < 3) // Standard, machine gun & heavy machine gun only
                {
                    pressedTime = Time.time + 0.33f; // DELAY FOR AUTOFIRE - this is only for non-machine gun weapons
                }
                else
                {
                    pressedTime = Time.time + 0.5f; // DELAY FOR AUTOFIRE - this is only for non-machine gun weapons
                }

                sfx.PlaySound(27, Vector3.zero); // PLAY EMPTY CLIP
            }
        }
        
    }
   

    
    // BULLET HIT DAMAGE VALUES
    //**********************

    /* 
     standard,
     hiImpact,
     heavyMetal,
     expSmall,
     expLarge,
     hellfire,
     smartMissile,
    

    private int standardAmmo;
    private int machineGunAmmo;
    private int heavyMachineGunAmmo;
    private int expSmallAmmo;
    private int expLargeAmmo;
    private int hellfireMissileAmmo;
    private int incendaryMissileAmmo;
    private int bunkerMissileAmmo;

    private int standardAmmoMax;
    private int machineGunAmmoMax;
    private int heavyMachineGunAmmoMax;
    private int expSmallAmmoMax;
    private int expLargeAmmoMax;
    private int hellfireMissileAmmoMax;
    private int incendaryMissileAmmoMax;
    private int bunkerMissileMax;
   */

    //**********************

    public int Set_Bullet_Damage()
    {
        switch (currentAmmo)
        {
            case Ammo.standard: hitDamage = 12; break; // TEMPORARY VALUES!! just to get us going for Testing purposes
            case Ammo.machineGun: hitDamage = 15; break;
            case Ammo.heavyMachineGun: hitDamage = 20; break;
            case Ammo.expSmall: hitDamage = 40; break;
            case Ammo.expLarge: hitDamage = 35; break; // X 2 remember!
            case Ammo.hellfireMissile: hitDamage = 150; break;
            case Ammo.incendaryMissile: hitDamage = 250; break;
            case Ammo.bunkerMissile: hitDamage = 400; break;

        }
         return hitDamage;

    }





  

    //----------------------
    // HEALTH
    //----------------------
    public void Set_Current_Weapon(Ammo weapon)
    {

       
       

        // Set up the new weapon values
        switch (weapon)
        {
            case Ammo.standard: 

                
                currentAmmoAmount = standardAmmoMax; currentAmmoMax = standardAmmoMax; bulletID = 0; 
                

                break; // TEMPORARY VALUES!! just to get us going for Testing purposes

            case Ammo.machineGun:

               
                                    
                    currentAmmoAmount = machineGunAmmo; currentAmmoMax = machineGunAmmoMax; bulletID = 1;
               
               

                break;

            case Ammo.heavyMachineGun: 

              
                    currentAmmoAmount = heavyMachineGunAmmo; currentAmmoMax = heavyMachineGunAmmoMax; bulletID = 2; 
               

                break;

            case Ammo.expSmall:
 
               
                    currentAmmoAmount = expSmallAmmo; currentAmmoMax = expSmallAmmoMax; bulletID = 3;
              

                break;

            case Ammo.expLarge: 

              
                    currentAmmoAmount = expLargeAmmo; currentAmmoMax = expLargeAmmoMax; bulletID = 4; 
               
                break;

            case Ammo.hellfireMissile: 

               
                    currentAmmoAmount = hellfireMissileAmmo; currentAmmoMax = hellfireMissileAmmoMax; bulletID = 5; 
               

                break;

            case Ammo.incendaryMissile: 

                
                    currentAmmoAmount = incendaryMissileAmmo; currentAmmoMax = incendaryMissileAmmoMax; bulletID = 6; 
                

                break;

            case Ammo.bunkerMissile:

                
                    currentAmmoAmount = bunkerMissileAmmo; currentAmmoMax = bunkerMissileAmmoMax; bulletID = 7; 
                

                break;


        }
       
        Update_Current_Ammo(0);


    }




    //----------------------
    // UPDATE CURRENT AMMO
    //----------------------
    public void Update_Current_Ammo(float value)
    {

        if (currentAmmoAmount > 0) { currentAmmoAmount = currentAmmoAmount - value; }

        if (currentAmmoAmount < 1) { } else { HUD.Do_Ammo_Bar(currentAmmoAmount, currentAmmoMax); } // HEALTH BAR UPDATE - script is on NGUI UI Root (2D)
   
    }


   

    //----------------------
    // SAVE CURRENT WEAPON STATE
    //----------------------
    public void Save_Current_Weapon_State(Ammo weapon)
    {

        // First save out the current Ammo amount of the current weapon
        switch (weapon)
        {
            case Ammo.standard: standardAmmo = (int)currentAmmoAmount; break; // TEMPORARY VALUES!! just to get us going for Testing purposes
            case Ammo.machineGun: machineGunAmmo = (int)currentAmmoAmount; break;
            case Ammo.heavyMachineGun: heavyMachineGunAmmo = (int)currentAmmoAmount; break;
            case Ammo.expSmall: expSmallAmmo = (int)currentAmmoAmount; break;
            case Ammo.expLarge: expLargeAmmo = (int)currentAmmoAmount; break;
            case Ammo.hellfireMissile: hellfireMissileAmmo = (int)currentAmmoAmount; break;
            case Ammo.incendaryMissile: incendaryMissileAmmo = (int)currentAmmoAmount; break;
            case Ammo.bunkerMissile: bunkerMissileAmmo = (int)currentAmmoAmount; break;
        }

    }




    //***********************			
    //CHECK FOR MOUSE COORDS - not using right now Jaime
    //***********************			
    void GetMouseCoords()
    {


        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            //if the tap lands on the shortTapObj, then shows the effect.
            if (hit.collider.transform)
            {
                tapObject = hit.collider.transform; // RESET this PUBLIC value at top of function
                //tapObjectPosition = tapObject.transform.position; // vector position
                tapObjectPosition = hit.point; // vector position
                hitDamage = Set_Bullet_Damage();
                //check to make sure if the tap count matches
                //if(tap.count == 2){}
                //Debug.Log ("Hit! " + tapObject.name);
            }
        }

    }





    // IPHONE
    //***********************
    void Scan_Iphone_Inputs()
    {
        if (tapObject)
        {

        }


        tapObject = null; //RESET always!

        // GLOBAL INPUT VARIABLES

        /*  iDown;
            iUp;
            iLeft;
            iRight;
            iFire;
            iUse;
            iToggleLeft;
            iToggleRight;
            iMenu;
            iQuit;
            iPause;
            iCameraIn;
            iCameraOut;
            ICameraSmartOn;
		


        iOSlayerMask = 1 << 8;
        iOSString = "";

        for(int i = 0; i < Input.touchCount; ++i)
        {
            if(Input.GetTouch (i).phase.Equals (TouchPhase.Began))
            {
                iOSRay = Camera.main.ScreenPointToRay (Input.GetTouch (i).position);
                if(Physics.Raycast (iOSRay, out iOSRayHit, 10, iOSlayerMask)){iOSString = iOSRayHit.transform.name;}
            }
        }
        
        */



        switch (game)
        {



            // MAIN MENU
            case State.menu:
                {





                }
                break;

            // GAME
            case State.game:
                {




                }
                break;

        }


    }







    // APPLE MAC COMPUTER
    //***********************			
    void Scan_AppleMac_Inputs()
    {




        if (!iQuit) { if (Input.GetKeyDown(bIQuit)) { game = State.quit; stage = 0; } }//

        // if (iQuit) { game = State.quit; stage = 0; iQuit = false; } // CAN BE SET FROM SUB MENU OR MAIN MENU TOO

        DebugToggleWeapons();


    }

    // APPLE MAC WEB PLAYER
    //***********************			
    void Scan_AppleMac_WebPlayer_Inputs()
    {
    }

    // WINDOWS PLAYER
    //***********************			
    void Scan_Windows_Inputs()
    {

        // These are defaults & should be set in KeyBindings Screen


        //if (tapObject)
        //{
            //Debug.Log ("Hit! " + tapObject.name);
       // }

       // if (!tapObject)
       // {

           // GetMouseCoords();
       // }



        //tapObject = null; //RESET always!
        bToggleWeaponDown = KeyCode.F1;
        bToggleWeaponUp = KeyCode.F2;
       
        bIUse = KeyCode.Space;
        bIToggleLeft = KeyCode.Comma;
        bIToggleRight = KeyCode.Period;
        bIMenu = KeyCode.M;
        bIQuit = KeyCode.Escape;
        bIPause = KeyCode.P;




        //***** KEYS PRESSED *****\\
        //************************\\

        //******* KEYS HIT *******\\
        //************************\\
        // TOGGLE LEFT
        //if (Input.GetKeyDown(bIToggleLeft)) { if (iToggleLeft == false) { iToggleLeft = true; } else { iToggleLeft = false; } }
        // TOGGLE RIGHT
        //if (Input.GetKeyDown(bIToggleRight)) { if (iToggleRight == false) { iToggleRight = true; } else { iToggleRight = false; } }
        // USE
        //if (Input.GetKeyDown(bIUse)) { if (iUse == false) { iUse = true; } else { iUse = false; } }
        // PAUSE - not active if Game paused
        // if(Input.GetKeyDown (bIPause)) { if(!HUDDoorsScript.HUDDoorsActive) { if(iPause == false) { iPause = true; if(State.game == game) { HUDDoorsScript.closeTheDoor = true; } } else { iPause = false; } } }// RESET From SubMenu (Back button)
        // QUIT
        if (!iQuit) { if (Input.GetKeyDown(bIQuit)) { game = State.quit; stage = 0; } }//

       // if (iQuit) { game = State.quit; stage = 0; iQuit = false; } // CAN BE SET FROM SUB MENU OR MAIN MENU TOO

        DebugToggleWeapons();
    }



    //----------------------
    // DEBUG WEAPON TOGGLE
    //---------------------		
    void DebugToggleWeapons()
    {

        if (Input.GetKeyUp(bToggleWeaponUp))
        {
            Save_Current_Weapon_State(currentAmmo);

            currentAmmo++;

            if ((int)currentAmmo > 7) { currentAmmo = Ammo.standard; }
            Set_Current_Weapon(currentAmmo); Debug.Log("New Weapon! " + currentAmmo);

           

        }
        if (Input.GetKeyUp(bToggleWeaponDown))
        {
            Save_Current_Weapon_State(currentAmmo);

            currentAmmo--;

            if ((int)currentAmmo < 0) { currentAmmo = Ammo.standard; }

            Set_Current_Weapon(currentAmmo); Debug.Log("New Weapon! " + currentAmmo);

          
        }

    }



    // WINDOWS 'WEB' PLAYER
    //***********************			
    void Scan_Windows_WebPlayer_Inputs()
    {

        if (tapObject)
        {
            //Debug.Log("Hit! " + tapObject.name);
        }

        if (!tapObject)
        {

           // GetMouseCoords();
        }


        //tapObject = null; //RESET always!

        bIUse = KeyCode.Space;
        bIToggleLeft = KeyCode.Comma;
        bIToggleRight = KeyCode.Period;
        bIMenu = KeyCode.M;
        bIQuit = KeyCode.Escape;
        bIPause = KeyCode.P;

       

        //***** KEYS PRESSED *****\\
        //************************\\

        //******* KEYS HIT *******\\
        //************************\\
        // TOGGLE LEFT
       // if (Input.GetKeyDown(bIToggleLeft)) { if (iToggleLeft == false) { iToggleLeft = true; } else { iToggleLeft = false; } }
        // TOGGLE RIGHT
        //if (Input.GetKeyDown(bIToggleRight)) { if (iToggleRight == false) { iToggleRight = true; } else { iToggleRight = false; } }
        // USE
       // if (Input.GetKeyDown(bIUse)) { if (iUse == false) { iUse = true; } else { iUse = false; } }
        // PAUSE - not active if Game paused
        // if(Input.GetKeyDown (bIPause)) { if(!HUDDoorsScript.HUDDoorsActive) { if(iPause == false) { iPause = true; if(State.game == game) { HUDDoorsScript.closeTheDoor = true; } } else { iPause = false; } } }// RESET From SubMenu (Back button)
        // QUIT
        //if (!iQuit) { if (Input.GetKeyDown(bIQuit)) { game = State.quit; stage = 0; } }//

       // if (iQuit) { game = State.quit; stage = 0; iQuit = false; } // CAN BE SET FROM SUB MENU OR MAIN MENU TOO
    }

    // ANDROID
    //***********************			
    void Scan_Windows_Android_Inputs()
    {
    }

    // FLASH PLAYER
    //***********************			
    void Scan_Flash_Inputs()
    {
    }






    // PROCESS THE FIRST FEW START SCREENS - PRE MAIN MENU
    //***********************		
    void Do_Game_Start()
    {
        
    }



    // PROCESS THE MAIN MENU
    //***********************		
    void Do_Main_Menu()
    {

      

    }

    // PROCESS THE MAIN MENU - if coming back from GAME
    //***********************		
    void Do_Main_Menu_From_Game()
    {

        switch (stage)
        {

            // SET MAIN MENU POSITIONS

            case 0:// CURRENTLY BLACK SCREEN
               
                break;

            case 10:// FADE TO CLEAR
              
               

            case 20:// SET MENU TO ACTIVE & SHOW CURSOR (if necessary)

                break; 

            case 30:// 


                break;


            case 40:// 
              
                break;


            case 50:// CHECK FOR ANY INPUT TO CONTINUE
              
                break;


            case 55:// BRIEF PAUSE
               

                break;

            case 60:// 
               

                break;



        }




    }






    // LOAD LAST LEVEL 
    //***********************		
    void Load_New_Level()
    {

        //*****DEBUG VALUE****!!!
        levelNumber = 1;
        //*****DEBUG VALUE****!!!

        string LevelString;

        if (hardware == 6 | hardware == 1) // 1 & 6 are iOS & Android touch devices
        {
            // No need to save Key bindings or load them 

            if (levelNumber < 10)
            { LevelString = "Level0" + levelNumber; }
            else
            { LevelString = "Level" + levelNumber; }


        }
        else
        {
            // Full monty  - we have a PC or Mac

            if (levelNumber < 10)
            { LevelString = "Level0" + levelNumber; }
            else
            { LevelString = "Level" + levelNumber; }


        }


        Application.LoadLevel(LevelString);


        //REMEMBER THIS LEVEL NUMBER FOR CHECKS IN LEVEL LOADING SCREEN
        currentLevelNumber = levelNumber;


    }







    //***********************		
    void Do_Game()  // PROCESS THE GAME
    {

        switch (stage)
        {


            case 0:// CURRENTLY BLACK SCREEN
               
                break;

            case 10:// FADED TO CLEAR
                {


                    stage = 20;

                }
                break;


        }


    }



  
   


    //***********************		
    void Load_Player_Data()  // PLAYER PREFS FILE
    {

        // QUICK TEST


        if (hardware == 3 | hardware == 5) // 3 & 5 are OSX & Windows WebPlayers - must use standard Player Prefs
        { playerName = PlayerPrefs.GetString("playerName"); }
        else
        { playerName = PreviewLabs.PlayerPrefs.GetString("playerName"); }



        // IF NULL FILE THEN MAKE A DEFAULT ONE
        if (playerName == "") { Set_Player_Data_Defaults(); Debug.Log(" No Player Prefs found - making a default one"); }



        // NOW DO IT PROPERLY
        if (hardware == 6 | hardware == 1) // 1 & 6 are iOS & Android touch devices
        {
            // No need to save Key bindings or load them 

            playerName = PreviewLabs.PlayerPrefs.GetString("playerName");
            levelNumber = PreviewLabs.PlayerPrefs.GetInt("levelNumber");
            score = PreviewLabs.PlayerPrefs.GetInt("score");


        }
        else
        {
            if (hardware == 3 | hardware == 5) // 3 & 5 are OSX & Windows WebPlayers - must use standard Player Prefs
            {


                playerName = PlayerPrefs.GetString("playerName");
                levelNumber = PlayerPrefs.GetInt("levelNumber");
                score = PlayerPrefs.GetInt("score");



            }
            else
            {
                // Full monty  - we have a PC or Mac

                playerName = PreviewLabs.PlayerPrefs.GetString("playerName");
                levelNumber = PreviewLabs.PlayerPrefs.GetInt("levelNumber");
                score = PreviewLabs.PlayerPrefs.GetInt("score");
            }




        }

    }






    //***********************		
    void Do_Save_And_Quit()  // PROCESS THE GAME
    {

        switch (stage)
        {
            case 0:// SAVE DATA
                {


                    if (hardware == 6 | hardware == 1) // 1 & 6 are iOS & Android touch devices
                    {
                        // No need to save Key bindings or load them 

                        PreviewLabs.PlayerPrefs.SetString("playerName", "Nick");
                        PreviewLabs.PlayerPrefs.SetInt("levelNumber", levelNumber);
                        PreviewLabs.PlayerPrefs.SetInt("score", 945678654);
                        PreviewLabs.PlayerPrefs.Flush();


                    }
                    else
                    {
                        if (hardware == 3 | hardware == 5) // 3 & 5 are OSX & Windows WebPlayers - must use standard Player Prefs
                        {


                            PlayerPrefs.SetString("playerName", "Nobody");
                            PlayerPrefs.SetInt("levelNumber", levelNumber);
                            PlayerPrefs.SetInt("score", 945678654);
                            PlayerPrefs.Save();


                        }
                        else
                        {
                            // Full monty  - we have a PC or Mac

                            PreviewLabs.PlayerPrefs.SetString("playerName", "Nobody");
                            PreviewLabs.PlayerPrefs.SetInt("levelNumber", levelNumber);
                            PreviewLabs.PlayerPrefs.SetInt("score", 945678654);
                            PreviewLabs.PlayerPrefs.Flush();
                        }


                    }



                    stage = 10;
                }
                break;

            case 10:
                {
                    print("This Player's Name: " + PreviewLabs.PlayerPrefs.GetString("playerName"));


                    stage = 20;

                }
                break;

            case 20:
                {


                    //Destroy (GameObject.Find ("Null_Level"));
                    Application.Quit();


                }

                break;



        }



    }


    //  FOR IF THERE IS NO PLAYER DATA..like a first time run.
    //***********************		
    void Set_Player_Data_Defaults()
    {


        levelNumber = 1; //No level number as yet so set it


        if (hardware == 6 | hardware == 1) // 1 & 6 are iOS & Android touch devices
        {
            // No need to save Key bindings or load them 

            PreviewLabs.PlayerPrefs.SetString("playerName", "Nobody");
            PreviewLabs.PlayerPrefs.SetInt("levelNumber", levelNumber);
            PreviewLabs.PlayerPrefs.SetInt("score", 945678654);
            PreviewLabs.PlayerPrefs.Flush();


        }
        else
        {



            if (hardware == 3 | hardware == 5) // 3 & 5 are OSX & Windows WebPlayers - must use standard Player Prefs
            {


                PlayerPrefs.SetString("playerName", "Nobody");
                PlayerPrefs.SetInt("levelNumber", levelNumber);
                PlayerPrefs.SetInt("score", 945678654);
                PlayerPrefs.Save();


            }
            else
            {
                // Full monty  - we have a PC or Mac

                PreviewLabs.PlayerPrefs.SetString("playerName", "Nobody");
                PreviewLabs.PlayerPrefs.SetInt("levelNumber", levelNumber);
                PreviewLabs.PlayerPrefs.SetInt("score", 945678654);
                PreviewLabs.PlayerPrefs.Flush();
            }

        }

    }








    //****************************************************************************************************
    //****************************************************************************************************
    //   NGUI DELEGATE STUFF FOR BUTTONS

    private GameObject quitBNo;
    private GameObject quitBYes;
    private GameObject loadLevelB;
    private GameObject backToGameB;
    private GameObject buttonArrowLeft;
    private GameObject buttonArrowRight;
    private GameObject levelsLabel;

    // GUI POPUPS AND INFO BOXES - separate NGUI Camera than Main Menu System uses
    /*
    void Set_NGUI2_Listeners()
    {
        quitBNo = GameObject.Find("buttonNo");
        UIEventListener.Get(quitBNo).onClick += Quit_button_No;

        quitBYes = GameObject.Find("buttonYes");
        UIEventListener.Get(quitBYes).onClick += Quit_button_Yes;


        //*************************************
        // THESE ARE ACTUALLY NGUI LISTENERS
        //*************************************
        loadLevelB = GameObject.Find("buttonLevelLoad");
        UIEventListener.Get(loadLevelB).onClick += Load_Level_Button;

        backToGameB = GameObject.Find("buttonBackToGame");
        UIEventListener.Get(backToGameB).onClick += Back_To_Game_Button;

        buttonArrowLeft = GameObject.Find("buttonArrowLeft");
        UIEventListener.Get(buttonArrowLeft).onClick += Level_Left_Button;

        buttonArrowRight = GameObject.Find("buttonArrowRight");
        UIEventListener.Get(buttonArrowRight).onClick += Level_Right_Button;

        levelsLabel = GameObject.Find("Label_LevelBox");


    }
   
   

    //***********************		
    // LEVEL MINUS
    public void Level_Left_Button(GameObject loadLevelB)
    {

        if (levelNumber > 1)
        {
            levelNumber = levelNumber - 1;
            string tempString = Change_Level_Names();// Stored in an Array

            Transform labelTransform = levelsLabel.transform;
            UILabel uiLabel = labelTransform.GetComponent<UILabel>() as UILabel;
            uiLabel.text = tempString;

            //levelChanged = true; 
        }
    }
    //***********************		
    // LEVEL PLUS
    public void Level_Right_Button(GameObject loadLevelB)
    {
        if (levelNumber < maxLevelNumber)
        {
            levelNumber = levelNumber + 1;

            string tempString = Change_Level_Names();

            Transform labelTransform = levelsLabel.transform;
            UILabel uiLabel = labelTransform.GetComponent<UILabel>() as UILabel;
            uiLabel.text = tempString;

            //levelChanged = true;
        }

    }


    //***********************		
    // LOAD LEVEL
    // levelChanged = true;
    // level has changed variable is set by Toggles for Level selection (none yet)
    public void Load_Level_Button(GameObject loadLevelB)
    {

        if (backFromGame) // only neccessary if back from the game  - game is currently 'PAUSED' (deactivated game objects)
        {


            // DESTROY THE LEVEL DATA & GAMEOBJECTS BUT KEEP THE PARENT
            foreach (Transform child in nullPoolParent.transform)
            {
                GameObject g = child.gameObject;

                Destroy(g);

            }

            Destroy(GameObject.Find("Null_Level"));


            currentLevelNumber = levelNumber;
            returnToGame = false;



        }

    }

    //***********************		
    // BACK TO GAME
    public void Back_To_Game_Button(GameObject backToGameB)
    {


        levelNumber = lastLevelNumber; // RESET LEVEL NUMBER TO THE SAME AS ON MENU ENTER
        currentLevelNumber = levelNumber;

        string tempString = Change_Level_Names();// Stored in an Array

        Transform labelTransform = levelsLabel.transform;
        UILabel uiLabel = labelTransform.GetComponent<UILabel>() as UILabel;
        uiLabel.text = tempString;



        cameraScript.Set_Fade_Mode("fadeIn", 0.3f);

        mainMenuScript.optionsActive = false; // Remember these booleans were active in Levels screen
        mainMenuScript.menuGameBegin = false;


        //optionsMenuScript.Hide_Options_Menu_Screen (); 



        levelChanged = false;
        returnToGame = true; // Switch() will react to this

        //Transform labelTransform = backToGameB.transform;
        //foreach(Transform child in labelTransform)
        //{ if(child.name == "Label") { labelTransform = child;}  }
        //UILabel uiLabel = labelTransform.GetComponent<UILabel> () as UILabel;
        //uiLabel.text = "TEST";

    }




    //***********************		
    // QUIT GAME...popup box over SubMenu (in Game)
    public void Quit_button_No(GameObject quitBNo)
    {


    }

    //***********************		
    // QUIT GAME...popup box over SubMenu (in Game)
    public void Quit_button_Yes(GameObject quitBYes)
    {


    }



    //***********************		
    // SHOW AND ENABLE NGUI2
    void Enable_NGUI2()
    {
        NGUIRoot.SetActive(true);// SET 'AFTER' RESETTING ANYTHING ELSE!!!!
    }


    //***********************		
    // HIDE AND DISABLE UNTIL NEEDED
    void Disable_NGUI2()
    {
        NGUIRoot.SetActive(false);// SET 'AFTER' RESETTING ANYTHING ELSE!!!!
    }



    //   NGUI DELEGATE STUFF FOR BUTTONS
    //****************************************************************************************************
    //****************************************************************************************************

    void SetUP_NGUI() // CALLED FROM AWAKE
    {

        NGUIRoot = GameObject.Find("UI Root2 (2D)");

        panelLoading = GameObject.Find("PanelLoading");

        buttonBackToGame = GameObject.Find("buttonBackToGame");// THIS BUTTON ONLY APPEARS IN MENU IF ARRIVED FROM GAME

        panelQuitSure = GameObject.Find("PanelQuitSure");
        panelPressAnyKey = GameObject.Find("PanelPressAnyKey");
        panelTapScreen = GameObject.Find("PanelTapScreen");


        //---------------------------------------------
        // GET NGUI EVENT LISTENER to check for Raycasts and Button Presses etc.
        //Set_NGUI2_Listeners (); // add hooks for Button events
        //---------------------------------------------
        //Disable_NGUI2 (); // Shut it down we don't need it yet

    }
    */

}






