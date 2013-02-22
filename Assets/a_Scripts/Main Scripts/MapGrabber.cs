using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class MapGrabber : MonoBehaviour {


    public bool debug = true; // SET TO OFF IN EDITOR FOR FINAL BUILD !!!!!!
    private GameObject tempObject;
    private GameObject camera;
    private ScriptPlayerCamera cameraScript;
    private PrefabLabData scriptPLD;
    private int nameIDCount; //simple counter for loaded object ids in editor array

    private int objectCount;
    public int arraySize;
    private int arrayLoopSize = 17; //This represents how many separate pieces (Columns) of data we wish to save: POSITION X,Y,Z + ROTATION X,Y,Z,W etc..
    private string separator = ","; //Generic string separator
    private string sceneName;
    private int stringDataEnd = 2; // this the position where the actual string Data turns into Float Data - need to know in order to parse Read-in txt file data correctly
    public float[,] levelData; // The size of this is set after DATA file is read in using 'objectCount' variable
    public GameObject[] levelObjectArray; // Very simple somewhat clumsy way of keeping exact track of all level objects
                                          //Make an array the exact same length as the Level data array and when I need to 'show' a pooled object 
    public GameObject[] objectPoolList;
    private StringReader sr;
    private TextAsset resFile2;
    private ScriptAInputManager inp;
    private GameObject StaticBatchingRoot;

    // INT CODE FOR PLATFORM
    private int hardware;

    // DIRECTORY  PATH SPECIFICS
    private string pathToResourceFolder;
    private string pathSlash;
    private string pathSlashEnd;
    private string LevelString;


    private List<GameObject> prefabList = new List<GameObject>(); // List of allPrefabs in resources Folder
    //private List<GameObject> activePoolObjectList = new List<GameObject>(); // Debug stuff

    private float cullTimer; // Timer for delaying the Culling check in Update
    public float cullTime;
    private int camViewLength; // Length of the Camera Frustrum Array[] for culling - should be 4
    private float clipDistance; // For culling objects beyond clip ...clip and fog should be the set the same

    // Main culling function
    Vector2[] camView;
    Vector2 objectPosition; 
    Vector2 cameraPosition;
    GameObject cullObject;

    //GameObject[] cube; // Debug stuff
    
    # region ++ Awake() finds all Level Map objects & writes a data file to the Resources folder


    //GET PLATFORM & FOLDER PATHS THEN FIND RELEVANT LEVEL MESH & WRITE TEXT DATA FILE
    //****************************
    void Awake()
    {

        if (cullTime == 0) { cullTime = 0.25f; }


        string tag = "mob"; // Default Tag name = MapOBject.. may not even need these 'tags'
        string tempString = "";

      
        Get_Platform(); // Find out what we're running on


        sceneName = Application.loadedLevelName;
        camera = GameObject.Find("PlayerCamera");
        cameraScript = camera.GetComponent<ScriptPlayerCamera>(); // Get Handle to camera script & variables
        camView = new Vector2[cameraScript.camView.Length]; //camera Frustrum for culling
        camViewLength = 4; //Frustrum Array[] Length;
               

        stringDataEnd = 2;// For when file is read in - so far only have 2 real strings at start of each txt line
        // I Strip out and convert the string data - after the second chunk I know I can convert
        // the strings to Floats for the array Data

        //Find and get INPUT MANAGER SCRIPT - Totally necessary :)
        GameObject tempObject = GameObject.Find("A_GameWrapper");
        inp = tempObject.GetComponent<ScriptAInputManager>(); //

        StaticBatchingRoot = GameObject.Find("A_Null_Pool_Holder");



        if (inp.hardware == -1 | inp.hardware == 0)

        { debug = true; }
        else

        { debug = false; }

        if (inp.levelNumber == 0) // Will be the case if just editing the level perhaps
        {
            LevelString = sceneName;
        }
        else
        {

            if (inp.levelNumber < 10)
            {
                LevelString = "Level0" + inp.levelNumber;
            }

            else
            {
                LevelString = "Level" + inp.levelNumber;
            }

        }
        //Debug.Log (Application.dataPath + inp.pathSlash + inp.pathToResourceFolder + inp.pathSlashEnd + sceneName + ".txt");

       

        //-------------------------------------------------------------
        // GRAB EDITOR MAP OBJECTS (by Tag) & WRITE A TEXT DATA FILE
        //-------------------------------------------------------------


        if (debug) // Set to Null for final build or simply delete this whole section
        {
                     

            GameObject[] objectsArray = FindObjectsOfType(typeof(GameObject)) as GameObject[]; // Temporary Array

            StreamWriter sw = new StreamWriter(Application.dataPath + pathSlash + pathToResourceFolder + pathSlashEnd + LevelString + ".txt");

           

            foreach (GameObject objects in objectsArray)
            {
                //----------------------

                //tag = "";
                tempString = "";
                int grab = 0;

                //----------------------

                if (objects.name.Length > 3)
                {
                    tempString = (objects.name.Substring(0, 4));
                    //Debug.Log(" Name: " + tempString);
                }

                switch (tempString)
                {
                    case "bunk": grab = 1; break;
                    case "sand": grab = 1; break;
                    case "tent": grab = 1; break;
                    case "watc": grab = 1; break;
                    case "lo_B": grab = 1; break;
                    case "lo_M": grab = 1; break;
                    case "lo_R": grab = 1; break;
                    case "lo_S": grab = 1; break;
                    case "lo_T": grab = 1; break;

                    case "barr": grab = 1; break;
                    case "crat": grab = 1; break;
                    case "Lamp": grab = 1; break;
                    case "Tele": grab = 1; break;
                    case "Pylo": grab = 1; break;
                    case "lo_W": grab = 1; break;
                    case "Rock": grab = 1; break;
                    case "Sign": grab = 1; break;
                    case "sign": grab = 1; break;

                    case "tree": grab = 1; break;
                    case "wall": grab = 1; break;

                }


                // Get rid of any Blank Spaces - 3DSMAX adds numbers with a space to duplicate named meshes
                string blank = objects.name.Replace(" ", ""); 

                // Get rid of any 3dsMax number suffixes from Model names.
                string trimmedName = blank.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });
             

                // Only write the objects we want to instantiate with the Pool Manager

                if (grab == 1) 
                {
                  
                    // Make one big fat long string and use 'comma' to separate the data chunks.

                    tempString = tag + separator;// tag + separator; // This is the object TAG set in Editor: a "REAL STRING" (put any real strings first in list for easy parsing later)
                    tempString = tempString + trimmedName + separator; // NAME OF PREFAB: a "REAL STRING"


                    tempString = tempString + objectCount + separator; // UNIQUE identifier for each object: "REAL NUMBER" (start of number values)

                    tempString = tempString + objects.transform.position.x.ToString() + separator; // POSITION
                    tempString = tempString + objects.transform.position.y.ToString() + separator;
                    tempString = tempString + objects.transform.position.z.ToString() + separator;

                    tempString = tempString + objects.transform.rotation.x.ToString() + separator; // ROTATION
                    tempString = tempString + objects.transform.rotation.y.ToString() + separator;
                    tempString = tempString + objects.transform.rotation.z.ToString() + separator;
                    tempString = tempString + objects.transform.rotation.w.ToString() + separator;

                    tempString = tempString + 1 + separator; //objects.transform.localScale.x.ToString() + separator; // SCALE
                    tempString = tempString + 1 + separator;//objects.transform.localScale.y.ToString() + separator;
                    tempString = tempString + 1 + separator;//objects.transform.localScale.z.ToString() + separator;

                    tempString = tempString + "0" + separator; // SPECIAL SWITCH - will tell whether object exists or not - set to 1 on Instantiate() & 0 on Destroy()
                    tempString = tempString + "100" + separator; //HEALTH - not needed for all but for enemies it is vital

                    //--------------------------- 
                    // Get boundary box offsets for Rotation Pivot re-adjustments later on in code
                    // Not all objects have a Mesh or a Renderer or a Collision Box so I use all
                    // Object & Child Objects X & Ys to find an approximate width and height
                    // It will fail I think if all pivots of a hierarchy are in the same place
                    // But as I really only want to use this data for rare huge Game Objects 
                    // it doesn't really matter.
                    //--------------------------- 

                    float maxX = 0;
                    float maxZ = 0;
                    float minX = 0;
                    float minZ = 0;
                    float tX = 0;
                    float tZ = 0;
                    float bX = 0;
                    float bZ = 0;
                    float childCount = 0;
                    Vector3 tempV;

                    foreach (Transform child in objects.transform)
                    {
                        childCount++;
                        Transform c = child.GetComponent<Transform>();
                        if (c != null)
                        {
                            tX = c.transform.position.x;
                            tZ = c.transform.position.z;

                            if (minX == 0) { minX = tX; maxX = tX; minZ = tZ; maxZ = tZ; }

                            if (tX < minX) { minX = tX; }
                            if (tX > maxX) { maxX = tX; }
                            if (tZ < minZ) { minZ = tZ; }
                            if (tZ > maxZ) { maxZ = tZ; }

                            bX = Mathf.Abs(maxX - minX) / 1.5f;
                            bZ = Mathf.Abs(maxZ - minZ) / 1.5f;

                        }
                        else
                        {
                            //if(debug) { print (objects.name + ": Child Transform Has no Child Object!"); }

                            tempV = renderer.bounds.extents; // Just use use maximum render mesh boundarZ /2
                            bX = (tempV.x);
                            bZ = (tempV.z);

                        }
                    }

                    if (childCount == 0)
                    {
                        //if(debug) { print (objects.name + ": Object Has absolutelZ no Child Objects!"); }

                        tempV = objects.renderer.bounds.extents; // Just use use maximum render mesh boundarZ /2
                        bX = (tempV.x);
                        bZ = (tempV.z);
                    }


                    tempString = tempString + bX + separator; // This will allow the camera viewport check to adapt to the size of the object...
                    tempString = tempString + bZ + separator; // As some objects are huge, their centres are no good for checking if they are really visible

                    Destroy(objects); // DESTROY EDITOR OBJECT no longer needed - will be respawned by the Camera Viewport Manager code below in Update()

                    sw.WriteLine(tempString);

                    objectCount++; // This could/can be the unique identifier for each object - can jump straight to it in the Data Array  - Maybe useful???

                    grab = 0; //just in case - Reset
                }

                //----------------------


            }

            // Free up original level GameObjects Array

            //objectsArray = null; 

            // close write stream

            sw.Close(); 


            // NOW WE HAVE A TEXT LEVEL OBJECTS DATA FILE
            // We can compile the Prefabs we will use for instantiating
            // from the gameobject pool from the Resources folder
            // Trim their (too long) names and generally make a tidy usable list.

            Compile_Level_Prefab_List();




            //************************************
            // LOAD IN LEVEL DATA FILE & ASSIGN DATA TO A 2D ARRAY - 2d not really necessary, but as I wrote the TXT file using lines I thought I'd make them ROWS
            //************************************

            // SIMPLY LOAD IN   
     
            if (!debug) // Means there was no Stuff grabbed and placed in the Editor - we are reading & placing Objects from file data only
         
            {
                //DeleteAllFinalBuildPooledObjects();

                resFile2 = (TextAsset)Resources.Load(LevelString, typeof(TextAsset)); // Was using sceneName

                //StringReader str = new StringReader (resFile.text);
                sr = new StringReader(resFile2.text);

                string dataStr;
                while ((dataStr = sr.ReadLine()) != null)
                { objectCount++; } // Each line in the file represents a single object

            }

            else
            {
             
            }

            // LOAD IN   

            if (debug)
            {

                //AssetDatabase.ImportAsset (file);

                // SET SIZE FOR LEVEL DATA ARRAY 
                arraySize = objectCount;
                levelData = new float[objectCount, arrayLoopSize]; // SIZE OF ARRAY
                levelObjectArray = new GameObject[objectCount]; // List of Objects in the 'levelObjectArray' will equal the 'levelData' Array one has 'Gameobjects' and the other 'Transform data'
                //ObjectStringNames = new string[objectCount]; //For Grabbing string names

                //MakeObjectPool();


                resFile2 = (TextAsset)Resources.Load(LevelString, typeof(TextAsset));// Was using sceneName
                sr = new StringReader(resFile2.text);



                //--------------
                string dataString;
                int lineCount = 0;
                //--------------


                //--------------

                while ((dataString = sr.ReadLine()) != null)
                {

                    //--------------
                    lineCount = lineCount + 1;
                    //--------------

                    //STRIP LINE DOWN TO DATA COMPONENTS using separator
                    String[] str = dataString.Split(char.Parse(separator));
                    int stringLength = (str.Length) - 1; // GET LENGTH - should always actually equal 'arrayLoopSize' variable set at top of page

                    //--------------


                    //--------------
                    float tempData = 0;
                  
                    for (int i = 0; i < stringLength; i++)
                    {

                        if (i > stringDataEnd - 1) // No Words - so will be real float values after 'stringDataEnd-1'
                        {
                            tempData = Single.Parse(str[i]); // Convert string to float
                            levelData[lineCount - 1, i] = tempData;
                        }
                        else // Real string words so they need converting to our own number ids to fit in this Float Array
                        {

                            int t = 0;

                            switch (i)
                            {
                                case 0:// FIRST REAL STRING IS THE OBJECT EDITOR TAG TYPE

                                  
                                    t = ConvertTagString(str[i]); // Set a (psuedo)integer value for Tag type - correlates to Tag numbers(Elements) in Editor also
                                    levelData[lineCount - 1, i] = t;
                                    break;

                                case 1:// SECOND REAL STRING IS THE OBJECT NAME (Prefab)

                                  
                                    t = Assign_Object_Integer_ID(str[i]); // set to ID number
                                   // Debug.Log(str[i] + " : "  + t);
                                   // Debug.Log(" GameObject listed as: " +objectPoolList[i].name );

                                    levelData[lineCount - 1, i] = t;



                                    break;
                            }

                        }

                    } //for end

                }//while() end

                //--------------
                sr.Close();
                //--------------


                // FINALLY PREPARE A TIDY POOL LIST OF OUR NICELY NAMED PREFAB OBJECTS
                            
                int c = prefabList.Count;

                objectPoolList = new GameObject[c];

                int k = -1;
                foreach (GameObject g in prefabList)
                {
                  k++;
                  objectPoolList[k] = g;
                }
                prefabList = null;
                             
            }

 
        }
       
      
    }

     # endregion

    # region ++ Assign Integer ID to Level Data Objects Array[]

    // TAKE STRING NAME AND RETURN SIMPLE INTEGER ID
    //****************************
    private int Assign_Object_Integer_ID(string s)
    {
        int b = 0;
        int i = -1;
        foreach (GameObject g in prefabList)
        {


            if (g.name == s)
            {
              
                i++;
                // Debug.Log("txt String: " + s + " PoolName: " + g.name + " ...ID: " + i);
                b = 1;
                break;
            }
            else
            {
                i++;
            }

           
        }

        if (b == 0)
        {



            Debug.Log("UNMATCHED TXT OBJECT: " + s);
        }

        return i;


    }

     # endregion

    # region ++ Find all exisiting Level Prefabs to be used by Pool system then trim their names for proper referencing



    // TRIM STRING NAMES OF ALL CURRENT PREFABS
    //****************************
    void Compile_Level_Prefab_List()
    {


        int i = 0;
        string tempString = "";
        string prefabName = "";
        GameObject gob;

        // Prefabs will all be in the Resources directory for easy runtime access - Copied manually from the 'a_Prefabs' folder

        string prefabPath = Application.dataPath + pathSlash + "Resources" + pathSlashEnd;
        string[] prefabNameList = Directory.GetFiles(prefabPath, "*.*");


        // Remove ".prefab" extension from string listings

        foreach (string str in prefabNameList)
        {
            prefabNameList[i] = Path.GetFileName(str);

            prefabNameList[i] = Path.GetFileNameWithoutExtension(prefabNameList[i]);

            //Debug.Log("prefabNameList: " + prefabNameList[i]);

            i++;

        }

        // Further trim strings and remove the Merlin PrefabLabs suffixes '_prefabs' as well as the Unity '(Clone)' one 
        // First instantiate a new GameObject list from the prefabs in 'Resources' folder which we will later make our Object Pool from
        // Also - Prefab Labs adds a PrefabLab script Component and makes a parent object for each linked Prefab
        // We don't need these on our Pool list objects and so remove them here.
        // Also Prefab labs adds a 'LostObjects' Transform to some prefabs - we don't need this wasted component either.
      
       
        foreach (string str in prefabNameList)
        {
            //  PUT EXCEPTION PREFABS HERE!!! We don't want to add things like the Level Mesh to Object Pool

            if (str.Contains("Level") | str.Contains("level"))
            {
                //Debug.Log(" Name: " + str); // DO NOTHING HERE - DON'T WANT TO GRAB PRIMARY LEVEL MESHES..as they are single items anyway
            }
            else
            {
                GameObject prefab = Instantiate(Resources.Load(str)) as GameObject;

                prefabName = prefab.name;

                // Debug.Log("prefabName: " + prefab.name);

                // MERLIN PREFABLAB adds extra suffixes, parents & components
                // So find GameObjects containing children, unparent & then destroy
                // The (Merlin)parent as it is an unused empty Transform

                int numChildren = prefab.transform.childCount;

                if (numChildren > 0)
                {

                    for (int n = 0; n < numChildren; n++)
                    {
                        tempString = prefab.transform.GetChild(n).name;

                        if (tempString == "")
                        {
                            // Do Nothing
                        }

                        else
                        {

                            if (prefabName.Contains(tempString))
                            {

                                gob = prefab.transform.GetChild(n).gameObject;

                                gob.transform.parent = null;

                                // Kill unwanted Merlin Parent

                                Destroy(prefab);

                                prefab = gob;

                                int nc = gob.transform.childCount;

                                //Debug.Log("Parentless Object name: " + gob.name);

                                // Just in case there is phantom 'LostGameObject'
                                // search remaining Children and delete it

                                for (int k = 0; k < nc; k++)
                                {
                                    if (gob.transform.GetChild(k).name == "LostGameObjects")
                                    {
                                        Debug.Log(" Lost Object Found on: " + gob.name + " in " + name + ".script");

                                        Destroy(gob.transform.GetChild(k));

                                    }
                                }

                                prefab.SetActive(false);  // Don't forget to de-activate!!

                                gob = null;

                                break;
                            }
                            else
                            {


                                Debug.Log("MERLIN Child/Parent Name problem: " + prefab.name);

                            }
                        }

                    }

                }

                else
                {

                    // Manually trim the String of the prefab as it has no children

                    //Debug.Log("No Children: " + prefab.name);

                    tempString = "_prefabs(Clone)";
                    string s = prefabName.Replace(tempString, "");
                    prefab.name = s;
                }


                scriptPLD = prefab.GetComponent<PrefabLabData>(); if (scriptPLD) { Destroy(scriptPLD); } //
                Animator a = prefab.GetComponent<Animator>(); if (a) { Destroy(a); } //
               
                // FINALLY ADD TO OBJECTS LIST

                prefabList.Add(prefab); 
            }

          
        }

    }

    # endregion
        
    # region ++ Convert text file tag string to integer for storage in main Level Objects Data 'float' Array[]

   
    // RETURN INTEGER TAG ID
    //****************************
    int ConvertTagString(string tempString)
    {

        // These Tag ID's may not ever be needed - I've inlcuded them just in case
        // something crops up later and we need a fast way of getting tags from
        // The Level Data Array - it will be much quicker than using Unity commands to look up.
        // And as there are only a handful of tags they are easy to check and remember

        int tagID = 0;

        switch (tempString)
        {
            case "mob": tagID = 0; break; // DEFAULT TAG
            case "Pickup": tagID = 1; break; // Things that can be used such as Shield or Screen-Burst etc.. Mainly inventory
            case "FX": tagID = 2; break; // all FX spawners
            case "Prop_NonActive": tagID = 3; break; // totally static, non moveable
            case "Prop_Active": tagID = 4; break; // Animates or rotates or whatever
            case "Prop_InterActive": tagID = 5; break; // Can be interacted with in some way
            case "Enemy": tagID = 6; break; // Guess?
            case "Projectile": tagID = 7; break; // Not sure we need this one at all - but what the heck.
            case "Special": tagID = 8; break; // Spawners, triggers and other weird objects
        }

        return tagID;

    }


    # endregion

    # region ++ Create & add all necessary Prefabs to the Pool Manager Dictionary list

    // LOOP THROUGH LEVEL OBJECTS LIST & PASS EACH ONE TO POOL CREATOR
    //****************************
    void MakeObjectPool()
    {
        // THIS FUNCTION will create the actual library of Instances for each object       
        string NameString = "";
        int grab = 0;
        int i = 0;

        foreach (GameObject objects in objectPoolList)
        {

            if (objects != null)
            {
               
                // Get First 4 Letters of name
                NameString = (objects.name.Substring(0, 4));
                           

                switch (NameString)
                {
                    case "bunk": grab = 5; break; // Army Bunker
                    case "sand": grab = 5; break; // Army SandBags
                    case "tent": grab = 5; break; // Army Tents
                    case "watc": grab = 8; break; // Army Watch Tower
                    case "lo_B": grab = 8; break; // Building
                    case "lo_M": grab = 3; break; // Mosque
                    case "lo_R": grab = 3; break; // Ruins
                    case "lo_S": grab = 8; break; // Shops
                    case "lo_T": grab = 4; break; // Tower

                    case "barr": grab = 10; break; // Road Barriers
                    case "crat": grab = 10; break; // Crates
                    case "Lamp": grab = 10; break;
                    case "Tele": grab = 10; break;
                    case "Pylo": grab = 10; break;
                    case "lo_W": grab = 1; break; // Water Tower
                    case "Rock": grab = 5; break;
                    case "Sign": grab = 6; break;
                    case "sign": grab = 6; break;

                    case "tree": grab = 15; break;
                    case "wall": grab = 10; break;

                    default: grab = 3; break;

                }

                i++;

                CreatePoolInstances(i, grab);


            }

        }

    }

    // CREATE A POOL OBJECT AND ADD TO INTERNAL POOL LIST
    //****************************
    void CreatePoolInstances(int objectPoolListID, int maxNumber)
    {
        if (maxNumber > 0)
        {
            GameObject poolObject;

            for (int i = 0; i < (maxNumber); i++)
            {

                poolObject = ObjectPoolManager.CreatePooled(objectPoolList[objectPoolListID], new Vector3(0, 0, 0), Quaternion.identity, 0);

                ObjectPoolManager.DestroyPooled(poolObject); // Remove from Active pool list - doesn't affect the Pool library



            }
        }

        else
        {
            return;
        }
        //if(debug) { print ("True pool count is now :" + (truePoolCount-1)); }
        //if(objectPoolListID == 22) print ("WeaponIce :" + (truePoolCount - 1));
    }



    # endregion

    # region ++ Start function

    //****************************
   	void Start () { }

    # endregion

    # region ++ Check Camera Viewport Boundaries

    // UPDATE - ONLY CALLS THE VISIBILTY TEST FUNCTION
    //****************************
    void Update()
    {

        if (Time.time > cullTimer)
        {
            cullTimer = Time.time + cullTime;

            Check_If_Objects_Are_Visible();
        }

    }


    // TEST IF OBJECT IS IN CAMERA FRUSTRUM
    //****************************
    private bool Check_If_Object_Is_In_Frustrum(Vector2 point)
    {

        int i; 
        int j = camViewLength - 1;
        bool visible = false;

        for (i = 0; i < camViewLength; i++)
        {
          
                
             if ((camView[i].y < point.y && camView[j].y >=point.y ||   camView[j].y < point.y && camView[i].y >= point.y)  &&  (camView[i].x <=point.x ||camView[j].x <=point.x))
            {
                visible ^= (camView[i].x + (point.y - camView[i].y) / (camView[j].y - camView[i].y) * (camView[j].x - camView[i].x) < point.x);
            }
            j = i;
        }

        return visible;
    }


    

    // LOOP THOUGH MAIN LEVEL OBJECT ARRAY
    //****************************
    void Check_If_Objects_Are_Visible()
    {
      
        // ARRAY COLUMN DATA
        // 0  = Tag ID
        // 1  = Object Name ID
        // 2  = Unique Integer Identifier - still a float though obviously
        // 3  = Position X
        // 4  = Position Y
        // 5  = Position Z
        // 6  = Rotation X
        // 7  = Rotation Y
        // 8  = Rotation Z
        // 9  = Rotation W
        // 10 = Scale    X
        // 11 = Scale    Y
        // 12 = Scale    Z
        // 13 = Exists? Integer Switch - still a float
        // 14 = HEALTH  - not needed for all but for enemies it is vital
        // 15 = Boundary X offset  - added to camera viewport boundaries
        // 16 = Boundary Y offset  - added to camera viewport boundaries

        arraySize = (levelData.Length / arrayLoopSize); // GET NUMBER OF DATA ROWS/LINES 
               
        clipDistance = camera.camera.farClipPlane;

        cameraScript.UpdateCameraFrustrum();

        camView = cameraScript.camView;

        // for 2 vector distance checking
        cameraPosition.x = camera.transform.position.x;
        cameraPosition.y = camera.transform.position.z;
             

        bool inView = false;

        for (int i = 0; i < (arraySize); i++) // Count rows.
        {
           
            int tempID = ((int)(levelData[i, 0]));
            float x = levelData[i, 3];
            float y = levelData[i, 4];
            float z = levelData[i, 5];
            float bx = levelData[i, 15];
            float by = levelData[i, 16];
            float exists = levelData[i, 13];
                      
            objectPosition = new Vector2(x, z);



            inView = Check_If_Object_Is_In_Frustrum(objectPosition);
          
            if (inView)
            {

                if (Vector2.Distance(objectPosition, cameraPosition) < clipDistance)
                {


                    if (exists == 0) 
                    {
                        CreateNewObject(i); // SPAWN A GAME OBJECT it is in view
                    } 

                    else // If the object now exists we need to update the X & Y positions in case it is a moving entity
                    {

                        //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' CODE HERE!!!!- may endup making a case select for different types of object
                        //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' CODE HERE!!!! 

                        switch (tempID)
                        {

                            case 0://  DEFAULT - STATIC MAP OBJECTS


                                // NO UPDATING OF TRANSFORMS AND ROTATATION NEEDED


                                break;


                            case 1: // ENEMIES  STATIC - positions need constant updating as we move them out of view box to ' Virtually' destroy them



                                break;


                            case 2: // ENEMIES MOBILE - can move for the most part and so need X & Y's updating.. and maybe a few other things

                                // *Note Exists switch is automatically set to '1' here: CreateNewObject (i) - so this code will always be used
                                {
                                    levelData[i, 3] = levelObjectArray[i].transform.position.x; levelData[i, 4] = levelObjectArray[i].transform.position.y;
                                    levelData[i, 5] = levelObjectArray[i].transform.position.z;
                                }

                                break;


                            case 3: // COLLECTABLE PICKUP 

                                // IF TagID = ADDON  - it can be collected - and needs to be processed differently if so
                                if (exists == -1) // This value gets set to -1 in Addons Propulsion script Update() when it is collected so it can no longer be 
                                //processed by this Viewport Checker using the levelObjectArray[] list, but I still want to draw it as it is 
                                //now attached to the player (-1 'Exists' switch declares this) so I need to update its X & Y transforms so that it
                                //stays in camera view along with the player(or else it will disappear as it would be using it's original X & Y positions
                                //that were placed in the Unity Editor - obviously non moving Props and stuff are simpler to manage as they remain static.
                                //*** SET MOVING ENEMIES TO ANOTHER VALUE!!! -2 would be good
                                {
                                    levelData[i, 3] = levelObjectArray[i].transform.position.x; levelData[i, 4] = levelObjectArray[i].transform.position.y;
                                    levelData[i, 5] = levelObjectArray[i].transform.position.z;
                                }
                                break;



                        }


                        //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' END!!!!
                        //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' END!!!!


                    }

                }
                else
                {
                   //levelData[i, 13] = 'Exists' Integer switch - gets' reset to zero on the end here
                    if (exists == 1) { DestroyCurrentObject(levelObjectArray[i]); levelObjectArray[i] = null; levelData[i, 13] = 0; } // OUT OF CAMERA VIEW  - if exists destroy  

                }
            }

            else
            {
                //levelData[i, 13] = 'Exists' Integer switch - gets' reset to zero on the end here
                if (exists == 1) { DestroyCurrentObject(levelObjectArray[i]); levelObjectArray[i] = null; levelData[i, 13] = 0; } // OUT OF CAMERA VIEW  - if exists destroy  

            }

        }

        //GameObject[] gos = activePoolObjectList.ToArray();            
        //StaticBatchingUtility.Combine(gos, StaticBatchingRoot);
        //cullObject.SetActive(false);

    }

    # endregion

    # region ++ Make and Destroy Instances 

	// ARRAY COLUMN DATA
	// 0  = Tag ID
	// 1  = Object Name ID
	// 2  = Unique Integer Identifier - still a float though obviously
	// 3  = Position X
	// 4  = Position Y
	// 5  = Position Z
	// 6  = Rotation X
	// 7  = Rotation Y
	// 8  = Rotation Z
	// 9  = Rotation W
	// 10 = Scale    X
	// 11 = Scale    Y
	// 12 = Scale    Z
	// 13 = Exists? Integer Switch - still a float
	// 14 = HEALTH  - not needed for all but for enemies it is vital
	// 15 = Boundary X offset  - added to camera viewport boundaries
	// 16 = Boundary Y offset  - added to camera viewport boundaries



    // CREATE NEW OBJECT FROM OBJECT POOL
    //****************************
	public void CreateNewObject(int i )
	{
		// FINAL BUILD DOES NOT NEED THESE LOCAL VARIABLES - they're here for current convenience
	
		int tagID = (int)levelData[i, 0];
		int tempObjectID = (int)levelData[i, 1];
		//string tempstring = ReturnObjectIdString (tempObjectID);
		int tempUniqueID = (int)levelData[i, 2];
		float px = levelData[i,3];
		float py = levelData[i, 4];
		float pz = levelData[i, 5];
		float rx = levelData[i, 6];
		float ry = levelData[i, 7];
		float rz = levelData[i, 8];
		float rw = levelData[i, 9];
		float scx = levelData[i, 10];
		float scy = levelData[i, 11];
		float scz = levelData[i, 12];
		levelData[i, 13] = 1; // OFFICIALLY EXISTS... if an object needs destroying 'PERMANENTLY' this must be set to -1 so it will not respawn
		int Health = (int)levelData[i, 14];
		float bx = levelData[i, 15];
		float by = levelData[i, 16];

		Vector3 posVector = new Vector3 (px, py, pz);
		//Vector3 scaleVector = new Vector3 (scx,scy,scz);
		Quaternion rot;
		rot.x = rx;
		rot.y = ry;
		rot.z = rz;
		rot.w = rw;


		// THIS IS THE 'GAME OBJECT' ARRAY - it is synched to the 'DATA' array index

		levelObjectArray[i] = ObjectPoolManager.CreatePooled (objectPoolList[tempObjectID], posVector, rot, tagID);// Added a tagID to pass here - probably never going to be needed :[

        levelObjectArray[i].renderer.castShadows = true;
        levelObjectArray[i].renderer.receiveShadows = false;
        levelObjectArray[i].SetActive(true);

       
	}



	void DestroyCurrentObject(GameObject tempObject)
	{
        tempObject.SetActive(false);
     	ObjectPoolManager.DestroyPooled (tempObject); // this ONLY de-activates the object and places back in the POOL LIBRARY

	}

	# endregion

    # region ++ Set up hardware platform specifics

    // FIND CURRENT HARDWARE PLATFORM
    //****************************
    void Get_Platform()
	{

		if(Application.platform == RuntimePlatform.OSXEditor) { hardware = -1;  }
		if(Application.platform == RuntimePlatform.WindowsEditor) { hardware = 0;  }
		if(Application.platform == RuntimePlatform.IPhonePlayer) { hardware = 1;  }
		if(Application.platform == RuntimePlatform.OSXPlayer) { hardware = 2;  }
		if(Application.platform == RuntimePlatform.OSXWebPlayer) { hardware = 3;  }
		if(Application.platform == RuntimePlatform.WindowsPlayer) { hardware = 4;  }
		if(Application.platform == RuntimePlatform.WindowsWebPlayer) { hardware = 5;  }
		if(Application.platform == RuntimePlatform.Android) { hardware = 6;  }
		if(Application.platform == RuntimePlatform.FlashPlayer) { hardware = 7;  }

		SetUp_Platform_Specifics (); // Path strings and stuff



	}


    // ADJUST FOLDER PATH SPECIFICS PER PLATFORM
    //****************************
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
                }
                break;

            case 1: // IPHONE
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";
                }
                break;
            case 2: // APPLE MAC COMPUTER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";
                }
                break;
            case 3: // APPLE MAC WEB PLAYER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "/";
                }
                break;
            case 4: // WINDOWS PLAYER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "//";
                }
                break;
            case 5: // WINDOWS 'WEB' PLAYER
                {
                    pathToResourceFolder = "Resources";
                    pathSlash = "/";
                    pathSlashEnd = "//";
                }
                break;
            case 6: // ANDROID
                {
                    pathToResourceFolder = "";
                    pathSlash = "/";
                }
                break;
            case 7: // FLASH PLAYER
                {
                    pathToResourceFolder = "";
                    pathSlash = "/";
                }
                break;



        }

    }

    # endregion

    # region ++ Temoprary debug stuff
    /* // DEBUGGING STUFF
     
       cube = new GameObject[camView.Length]; // DEBUG - WILL SHOW EXTENTS OF FRUSTRUM
       for (int i = 0; i < camView.Length; i++)
       {
           cube[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
           cube[i].transform.position = new Vector3(camView[i].x, camera.transform.position.y, camView[i].y);
           cube[i].transform.localScale = new Vector3(i + 1 * 20f, i + 1 * 20f, i + 1 * 20f);
           cube[i].name = "cube" + (i);
       }
      for (int i = 0; i < camView.Length; i++)
        {
            
            cube[i].transform.position = new Vector3(camView[i].x, camera.transform.position.y, camView[i].y);
          
        } 
       */

    # endregion


}
