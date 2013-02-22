using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;


public class ScriptASceneManager : MonoBehaviour
{


    public bool debug = true; // SET TO OFF IN EDITOR FOR FINAL BUILD !!!!!!
    private GameObject tempObject;
    private GameObject mapObjectsFile;
    private GameObject camera;
    private ScriptPlayerCamera cameraScript;
    private ScriptSoldier soldierScript;
    private ScriptPrefabList prefabListScript;
    private ScriptTargetIDManager targetIDManagerScript;
    private PrefabLabData scriptPLD;
    private int nameIDCount; //simple counter for loaded object ids in editor array

    private int objectCount;
    public int arraySize;
    private int arrayLoopSize = 18; //This represents how many separate pieces (Columns) of data we wish to save: POSITION X,Y,Z + ROTATION X,Y,Z,W etc..
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
    //private GameObject StaticBatchingRoot;

    // INT CODE FOR PLATFORM
    private int hardware;

    // DIRECTORY  PATH SPECIFICS
    private string pathToResourceFolder;
    private string pathSlash;
    private string pathSlashEnd;
    private string LevelString;


    private List<GameObject> prefabList = new List<GameObject>(); // List of allPrefabs in resources Folder

    public List<GameObject> activeList = new List<GameObject>(); // activated enemies are added on spawn

    private float cullTimer; // Timer for delaying the Culling check in Update
    public float cullTime;
    private int camViewLength; // Length of the Camera Frustrum Array[] for culling - should be 4
    public float clipDistance; // For culling objects beyond clip ...clip and fog should be the set the same

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

       
        string tempString = "";


        Get_Platform(); // Find out what we're running on


        sceneName = Application.loadedLevelName;
        camera = GameObject.Find("PlayerCamera");
        cameraScript = camera.GetComponent<ScriptPlayerCamera>(); // Get Handle to camera script & variables
        camView = new Vector2[cameraScript.camView.Length]; //camera Frustrum for culling
        clipDistance = camera.camera.farClipPlane; // No objects spawned beyond this point
        camViewLength = 4; //Frustrum Array[] Length;


        stringDataEnd = 2;// For when file is read in - so far only have 2 real strings at start of each txt line
        // I Strip out and convert the string data - after the second chunk I know I can convert
        // the strings to Floats for the array Data

        //Find and get INPUT MANAGER SCRIPT - Totally necessary :)
        GameObject tempObject = GameObject.Find("A_GameWrapper");
        inp = tempObject.GetComponent<ScriptAInputManager>(); //

        //StaticBatchingRoot = GameObject.Find("A_Null_Pool_Holder");

        tempObject = GameObject.Find("A_Null_Pool_Library");
        prefabListScript = tempObject.GetComponent<ScriptPrefabList>(); //


        // TARGET IDENTIFIERS in NGUI HUD stuff
        tempObject = GameObject.Find("A_Null_TargetID"); //in  NGUI Hierarchy
        targetIDManagerScript = tempObject.GetComponent<ScriptTargetIDManager>(); //
        //if (inp.hardware == -1 | inp.hardware == 0)

        //{ debug = true; Debug.Log("TRUE"); }
        //else

        // { debug = false; }

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

            Debug.Log(" EDITOR LEVEL DATA TEXT FILE PATH: " + (Application.dataPath + pathSlash + pathToResourceFolder + pathSlashEnd + LevelString + ".txt"));

            foreach (GameObject objects in objectsArray)
            {


                //----------------------

                //tag = "";
                tempString = "";
                string tag = "UNKNOWN"; // Default Tag name = MapOBject.. may not even need these 'tags'
                int grab = 0;
                int loopingSound = 0;

                 //if (objects.name.Contains("_prefabs")) { Debug.Log(" Name: " + objects.name);}
                //if (objects.name.Contains("soldier")) { Debug.Log(" Name: " + objects.name);}


                tempString = Check_If_Nasty_Merlin_Prefabs_Are_Causing_Problems_Again(objects.name, objects.transform);



                if (tempString == "NULL") // Means the function above tested and found a duplicate named Merlin prefab & deleted it
                {

                }
                else
                {


                    // Get rid of any Blank Spaces - 3DSMAX adds numbers with a space to duplicate named meshes
                    string blank = objects.name.Replace(" ", "");

                    // Get rid of any 3dsMax number suffixes from Model names.
                    string trimmedName = blank.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

                    //Debug.Log(" Name: " + trimmedName);

                    //----------------------

                    if (trimmedName.Length > 3)
                    {
                        tempString = (trimmedName.Substring(0, 4));
                        // Debug.Log(" Name: " + tempString);
                    }



                    //case "NEUTRAL STATIC": tagID = 0; break; // Default Static object - buildings etc..
                    //case "NEUTRAL MOBILE": tagID = 1; break; // Maybe Civilian Vehicles
                    //case "ENEMY STATIC": tagID = 2; break; // Non moving enemies
                    //case "ENEMY MOBILE": tagID = 3; break; // Moving ground based enemies
                    //case "ENEMY MOBILE AIR": tagID = 4; break; // Moving AIR enemies
                    //case "PICKUP": tagID = 5; break; // Collectables



                    switch (tempString)
                    {
                        case "bunk": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "sand": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "tent": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "watc": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "lo_B": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "lo_M": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "lo_R": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "lo_S": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "lo_T": grab = 1; tag = "NEUTRAL STATIC"; break;

                        case "barr": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "crat": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "Lamp": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "Tele": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "Pylo": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "lo_W": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "Rock": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "Sign": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "sign": grab = 1; tag = "NEUTRAL STATIC"; break;

                        case "tree": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "wall": grab = 1; tag = "NEUTRAL STATIC"; break;
                        case "enem": grab = 1;

                            //-------------------

                            // TAGS - these are quick lookup tag values for Level Object Array only

                            if (
                                trimmedName == "enemy_tank_big" ||
                                trimmedName == "enemy_tank_small" ||
                                trimmedName == "enemy_truck_brown" ||
                                trimmedName == "enemy_truck_medic" ||
                                trimmedName == "enemy_scout" ||
                                trimmedName == "enemy_mobile_radar" ||
                                trimmedName == "enemy_mobile_SAM" ||
                                trimmedName == "enemy_comm_dish" ||
                                trimmedName == "enemy_comm_building" ||
                                trimmedName == "enemy_comm_mast" ||
                                trimmedName == "enemy_jeep" ||
                                trimmedName == "enemy_scud" ||
                                trimmedName == "enemy_soldier" ||
                                trimmedName == "enemy_jet" ||
                                trimmedName == "enemy_jet_land" ||
                                trimmedName == "enemy_helicopter" ||
                                trimmedName == "enemy_helicopter_land"

                                )
                            {
                                tag = "ENEMY MOBILE";
                                //Debug.Log(" Mobile Name: " + objects.name);
                            }
                            else
                            {
                                //Static Enemy is classed as an installation that is an AA and attacks the player - it is highlighted by the HUD Early Warning System
                                tag = "ENEMY STATIC";
                                //Debug.Log(" Static Name: " + objects.name);
                            }


                           //-------------------

                            // LOOPING SOUNDS.. if object uses looping sounds we need to know for culling to switch them off directly
                             switch(trimmedName)
                             {
                                 case "enemy_soldier": loopingSound = 1; break; // machine gun sfx loops
                             }

                            break;

                        //-------------------




                    }





                    // Only write the objects we want to instantiate with the Pool Manager

                    if (grab == 1)
                    {

                        // Make one big fat long string and use 'comma' to separate the data chunks.

                        tempString = tag + separator;// tag + separator; // This is the object TAG set in Editor: a "REAL STRING" (put any real strings first in list for easy parsing later)
                        tempString = tempString + trimmedName + separator; // NAME OF PREFAB: a "REAL STRING"


                        tempString = tempString + "0" + separator; // LOD MESH IDENTIFIER - gets set further down in code

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


                        tempString = tempString +"0" + separator; // IF this Object uses looping sounds then the culling system must know about it to stop them playing


                        //-----------
                        //  IF BOUNDARY IS LARGE MARK AS LARGE OBJECT = 1

                        int largeObject = 0;

                        if (trimmedName.Contains("barrier") || trimmedName.Contains("PylonCableLOD")) { largeObject = 1; } // KNOWN LARGE OBJECTS

                        tempString = tempString + largeObject + separator; // As some objects are huge, their centres are no good for checking if they are really visible

                        // Looping SFX
                        tempString = tempString + loopingSound + separator; // IF this Object uses looping sounds then the culling system must know about it to stop them playing

                        //Destroy(objects); // DESTROY EDITOR OBJECT no longer needed - will be respawned by the Camera Viewport Manager code below in Update()

                        sw.WriteLine(tempString);

                        objectCount++; // This could/can be the unique identifier for each object - can jump straight to it in the Data Array  - Maybe useful???

                        grab = 0; //just in case - Reset
                    }

                }
                //----------------------


            }

            // Free up original level GameObjects Array

            objectsArray = null; 

            // close write stream

            sw.Close();


            // NOW WE HAVE A TEXT LEVEL OBJECTS DATA FILE
            // We can compile the Prefabs we will use for instantiating
            // from the gameobject pool from the Resources folder
            // Trim their (too long) names and generally make a tidy usable list.

           


        }

        // RESOURCES FOLDER prefabs are listed
        Compile_Level_Prefab_List();

        // IF DEBUG IS OFF IN THE EDITOR WE STILL NEED TO DELETE UNITY 3DSMAX OBJECTS FROM SCENE.. final build is not necessary as the file will not be there..removed manually


        // TEST and see if max map objects file is still there - if so then delete
        tempString = LevelString + "_Objects";

        string temp = tempString.Substring(0, 1);

        temp = temp.ToUpper();// The Max object Prefab starts with upper case 'L'..sigh
        tempString = tempString.Remove(0, 1);
        tempString = temp + tempString;
       
        mapObjectsFile = GameObject.Find(tempString); 

       
        if (mapObjectsFile)
        {
            Debug_3dsmax_Level_Objects_Remover(); Debug.Log(" Scene Manager Unity Editor - Deleting 3DSMAX level objects from Scene. No longer needed.");
        }

        //************************************
        // LOAD IN LEVEL DATA FILE & ASSIGN DATA TO A 2D ARRAY - 2d not really necessary, but as I wrote the TXT file using lines I thought I'd make them ROWS
        //************************************

        // SIMPLY LOAD IN   

        if (!debug) // Means there was no Stuff grabbed and placed in the Editor - we are reading & placing Objects from file data only
        {
            //DeleteAllFinalBuildPooledObjects();
            Debug.Log(" WARNING!! EDITOR DEBUG MODE IS OFF!! >>> LEVEL: " + LevelString + " : " + name);

            resFile2 = (TextAsset)Resources.Load(LevelString, typeof(TextAsset)); // Was using sceneName

            //StringReader str = new StringReader (resFile.text);
            sr = new StringReader(resFile2.text);

            string dataStr;
            while ((dataStr = sr.ReadLine()) != null)
            { objectCount++; } // Each line in the file represents a single object

        }

       

        // LOAD IN   

      
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

            int d = prefabList.Count;

            objectPoolList = new GameObject[d];

            int k = 0;
            foreach (GameObject g in prefabList)
            {

               // Debug.Log(" HERE!!: " + objectPoolList[k] + "  " + k);  
                objectPoolList[k] = g;
                k++;
               
            }

           // Debug.Log("objectPoolList.Length: " + objectPoolList.Length);  

            //if (debug)
            //{
                // SET LOD TRIGGER IN LEVEL ARRAY
                Find_If_Object_Has_LOD_Mesh_Attached();
            //}


            MakeObjectPool();

            prefabList = null;
          

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


    // FIND & MARK LOD MESHES
    //****************************
    void Find_If_Object_Has_LOD_Mesh_Attached()
    {
       
        /*
        for (int i = 0; i < (arraySize); i++) // Count rows.
        {



            foreach (GameObject p in objectPoolList)
            {
               
                if (levelData[i, 1] == k)
                {
                    if (p.transform.childCount > 0)
                    {

                        foreach (Transform child in p.transform)
                        {


                            string lastFourChars = p.name.Substring(p.name.Length - 4);

                            if (lastFourChars == "_LOD")
                            {
                                Debug.Log("  LOD Name: " + child.name  + "  :  " + i);
                                levelData[i, 2] = 1f; // SET TO TRUE THEN

                                break;
                            }


                        }
                    }
                }

                k++;
            }
        }
        */

       
        for (int i = 0; i < (arraySize); i++) // Count rows.
        {
            GameObject p = objectPoolList[(int)levelData[i, 1]];

            if (p.transform.childCount > 0)
            {

                if (p.renderer)
                {
                    p.renderer.receiveShadows = false;
                    p.renderer.castShadows = true;

                    foreach (Transform child in p.transform)
                    {
                        string lastFourChars = child.name.Substring(child.name.Length - 3);




                        if (lastFourChars == "LOD")
                        {
                            //Debug.Log("  LOD Name: " + child.name + "  :  " + i);
                            levelData[i, 2] = 1; // SET TO TRUE THEN
                            child.renderer.receiveShadows = false;
                            child.renderer.castShadows = true;

                            break;
                        }


                    }
                }

            }


        }


    }



    // TRIM STRING NAMES OF ALL CURRENT PREFABS
    //****************************
    void Compile_Level_Prefab_List()
    {

        //Null Parent holder of all prefabs
        GameObject prefabParent = GameObject.Find("A_Null_Pool_Library");


        int i = -1;
        string tempString = "";
        string prefabName = "";
        //string[] prefabNameList;
        GameObject gob;



        //GameObject[] objectsArray = Resources.FindObjectsOfTypeAll(typeof(GameObject)) as GameObject[]; // Temporary Array
        //GameObject[]  objectsArray = new GameObject[6];
        //prefabNameList = new string[prefabListScript.prefabList.Count];
        // i=0;

        // FIRST 


        foreach (GameObject p in prefabListScript.prefabList)
        {
            i++;
            // Get rid of any Blank Spaces - 3DSMAX adds numbers with a space to duplicate named meshes
            //Debug.Log(" PrefabName: " + p.name  + "  :  " + i);
            prefabName = p.name.Replace("_prefabs", "");
           

            //prefabName = p.name.Replace("(Clone)", "");



            //Debug.Log(" PrefabName: " + p.name  + "  :  " + i);

            int numChildren = p.transform.childCount;

            //***********************************************
            // FIND BASTARD LOST OBJECTS FROM PREFABS LAB
            // THEY ARE A BASTARD NUISCANCE
            // USE DESTROYIMMEDIATE() WITH 'TRUE' FLAG
            // AND THE EDITOR OBJECTS WILL BE DESTROYED FOREVER - HURRAHH!
            for (int j = 0; j < numChildren; j++)
            {
               
                if (p.transform.GetChild(j).name == "Lost GameObjects")
                {
                    Debug.Log(" Lost Object Found on: " + p.name + " in " + name + ".script --- Object Destroyed");

                    DestroyImmediate(p.transform.GetChild(j).gameObject,true);
                  
                }

            }
            //***********************************************

            p.name = prefabName;

           
          

                       
            GameObject prefab = Instantiate(p) as GameObject;

           


        
            if (numChildren > 0)
            {




                for (int n = 0; n < numChildren; n++)
                {

                   
                    tempString = prefab.transform.GetChild(n).name.Replace("(Clone)", "");
                    prefab.transform.GetChild(n).name = tempString;
                    tempString = prefab.transform.GetChild(n).name;



                    if (tempString == "")
                    {
                        // Do Nothing
                    }

                    else
                    {



                        //if (prefabName.Contains(tempString))
                        if (tempString == prefabName)
                        {
                            gob = prefab.transform.GetChild(n).gameObject;

                            gob.transform.parent = null;

                            Destroy(prefab); // KILL MERLIN LABS PARENT

                            prefab = gob;

                            int nc = prefab.transform.childCount;

                            //Debug.Log("Destroyed Parent: " + gob.name);

                            // Just in case there is phantom 'LostGameObject'
                            // search remaining Children and delete it

                            for (int k = 0; k < nc; k++)
                            {
                                if (prefab.transform.GetChild(k).name == "Lost GameObjects")
                                {
                                    Debug.Log(" Lost Object Found on: " + prefab.name + " in " + name + ".script --- Object Destroyed");

                                    Destroy(prefab.transform.GetChild(k));

                                }
                            }

                           

                            gob = null;

                            break;
                        }

                        else
                        {
                            Debug.Log("MERLIN Child/Parent Name problem: " + tempString + " with : " + prefab.name );
                            Debug.Log("Possibly no MERLIN Script attached - HURRAH!");
                            Debug.Log("So Don't Worry - Be Happy! :)");
                        }

                    }
                }

            }
            else
            {
                prefab.SetActive(false);

            }


            scriptPLD = prefab.GetComponent<PrefabLabData>(); if (scriptPLD) { Destroy(scriptPLD);  } //
            Animator a = prefab.GetComponent<Animator>() as Animator; if (a) { Destroy(a); } //

            Animator[] t = prefab.GetComponentsInChildren<Animator>();
           
            foreach (Animator b in t)
            {
                //Debug.Log("Animator Destroyed : " + b.gameObject.name);
                Destroy(b);

            }


            PrefabLabData[] d = prefab.GetComponentsInChildren<PrefabLabData>();
           
            foreach (PrefabLabData c in d)
            {

                //Debug.Log("Child Prefab Script Destroyed : " + prefab.gameObject.name);

                Destroy(c);

            }



            
            //if (t) { Destroy(t); Debug.Log("Animator Destroyed : " + prefab.name); } //

            // FINALLY ADD TO OBJECTS LIST
            prefab.transform.parent = prefabParent.transform;
            prefabList.Add(prefab);
            prefab.SetActive(false);  // Don't forget to de-activate!!

           
        }

        prefabListScript.prefabList = null;

       
      
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
            case "NEUTRAL STATIC": tagID = 0; break; // Default Static object - buildings etc..
            case "NEUTRAL MOBILE": tagID = 1; break; // Maybe Civilian Vehicles
            case "ENEMY STATIC": tagID = 2; break; // Non moving enemies
            case "ENEMY MOBILE": tagID = 3; break; // Moving ground based enemies
            case "ENEMY MOBILE AIR": tagID = 4; break; // Moving AIR enemies
            case "PICKUP": tagID = 5; break; // Collectables
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
        //Debug.Log(" HERE!!: " + objectPoolList.Length);  
 
        foreach (GameObject objects in objectPoolList)
        {
            //Debug.Log(" objectPoolList: " + objects.name + "  " + i);   
            if (objects)
            {
               
                // Get First 4 Letters of name
                NameString = (objects.name.Substring(0, 4));
              

                switch (NameString)
                {
                    case "bunk": grab = 5; break; // Army Bunker
                    case "sand": grab = 5; break; // Army SandBags
                    case "tent": grab = 5; break; // Army Tents
                    case "watc": grab = 5; break; // Army Watch Tower
                    case "lo_B": grab = 8; break; // Building
                    case "lo_M": grab = 3; break; // Mosque
                    case "lo_R": grab = 3; break; // Ruins
                    case "lo_S": grab = 8; break; // Shops
                    case "lo_T": grab = 4; break; // Tower

                    case "barr": grab = 6; break; // Road Barriers
                    case "crat": grab = 10; break; // Crates
                    case "Lamp": grab = 8; break;
                    case "Tele": grab = 8; break;
                    case "Pylo": grab = 5; break;
                    case "lo_W": grab = 1; break; // Water Tower
                    case "Rock": grab = 5; break;
                    case "Sign": grab = 6; break;
                    case "sign": grab = 6; break;

                    case "tree": grab = 10; break;
                    case "wall": grab = 3; break;
                    case "enem": grab = 2; break;
                    case "miss": grab = 10; break; // Missile

                    default: grab = 3; break;

                }

                CreatePoolInstances(i, grab);

                i++;


            }

        }




       
        // VITAL!! - FREE UP ACTIVE POOL OBJECTS NOW! return them to the library
        foreach ( GameObject poolObject in tempList)
        {
           ObjectPoolManager.DestroyPooled(poolObject); // Remove from Active pool list - doesn't affect the Pool library
        }

        tempList = null;




    }




    // CREATE A POOL OBJECT AND ADD TO INTERNAL POOL LIST
    //****************************
    private List<GameObject> tempList = new List<GameObject>(); // List of allPrefabs in resources Folder

    void CreatePoolInstances(int objectPoolListID, int maxNumber)
    {

        //Debug.Log("MAKING POOL OBJECT: " + objectPoolList[objectPoolListID] + " MAX COUNT: " + maxNumber);
       
        if (maxNumber > 0)
        {
            GameObject poolObject;

            for (int i = 0; i < maxNumber; i++)
            {

                poolObject = ObjectPoolManager.CreatePooled(objectPoolList[objectPoolListID], new Vector3(0, 0, 0), Quaternion.identity, 0);

                //ObjectPoolManager.DestroyPooled(poolObject); // Remove from Active pool list - doesn't affect the Pool library

                // Strip out PrefabLabs crap - the editor is adding these components and they are totally unnecessary.



                poolObject.SetActive(true);  //Set True for Child componenet search

                scriptPLD = poolObject.GetComponent<PrefabLabData>(); if (scriptPLD) { Destroy(scriptPLD); } //
                Animator a = poolObject.GetComponent<Animator>() as Animator; if (a) { Destroy(a); } //

                Animator[] t = poolObject.GetComponentsInChildren<Animator>();

                foreach (Animator b in t)
                {
                    //Debug.Log("Animator Destroyed : " + b.gameObject.name);
                    Destroy(b);


                }

                PrefabLabData[] p = poolObject.GetComponentsInChildren<PrefabLabData>();

                foreach (PrefabLabData c in p)
                {
                    //Debug.Log("Child Prefab Script Destroyed : " + c.gameObject.name);
                    Destroy(c);

                }


                poolObject.SetActive(false);
                tempList.Add(poolObject);// Collect all objects for removal from Pool 

               
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





    // IF ENEMY VISIBLE IT CAN BE TRACKED
    //****************************
    void HUD_Targeting(int i, float dist, bool freeTarget)
    {

        if (!freeTarget)
        {
            if (dist < 400)
            {

                if (dist < 200)
                {

                    // Maybe object has come in from side of frustrum and is already very close?

                    if (levelData[i, 15] == 0) // If nothing curently assigned
                    {
                        levelData[i, 15] = 2; //Near Tracking active now;
                    
                        targetIDManagerScript.Attach_Near_TargetID_Sprite(levelObjectArray[i].transform);
                    }

                    else
                    {
                        if (levelData[i, 15] == 1)
                        {

                            //Free up far targetID sprite
                            targetIDManagerScript.Free_TargetID_Sprite(levelObjectArray[i].transform);
                            // Set near Target ID sprite
                            targetIDManagerScript.Attach_Near_TargetID_Sprite(levelObjectArray[i].transform);

                            levelData[i, 15] = 2; //CLOSE Tracking active now;

                        }
                    }


                }

                else
                {
                    if (levelData[i, 15] == 0)
                    {
                        levelData[i, 15] = 1; //FAR Tracking active now;

                        targetIDManagerScript.Attach_Far_TargetID_Sprite(levelObjectArray[i].transform);
                    }


                    if (levelData[i, 15] == 2)
                    {

                        targetIDManagerScript.Free_TargetID_Sprite(levelObjectArray[i].transform);

                        levelData[i, 15] = 1; //FAR Tracking active now;

                        targetIDManagerScript.Attach_Far_TargetID_Sprite(levelObjectArray[i].transform);
                    }



                }

            }

            else
            {
                if (levelData[i, 15] > 0)
                {
                    levelData[i, 15] = 0; //Tracking OFF;

                    targetIDManagerScript.Free_TargetID_Sprite(levelObjectArray[i].transform);
                }
            }

        }
        else
        {
            if (levelData[i, 15] > 0)
            {
                levelData[i, 15] = 0; //Tracking OFF;

                targetIDManagerScript.Free_TargetID_Sprite(levelObjectArray[i].transform);
            }
        }



    }


    

    // IF ENEMY RESPAWNED UPDATE THE HEALTH VARIABLE IN THE RESPAWNED POOL SCRIPT - late update not from Start()!!
    //****************************
    public int Update_GameObject_Health_OnShow(GameObject g)
    {

        float  health = 0;

        for (int i = 0; i < (arraySize); i++) 
        {

            if (levelObjectArray[i] == g)
            {
                health = levelData[i, 14];

                break;
            }
      
        }

        if (health == 0)
        {
            Debug.Log(" No Match Found for: " + g.name); // IF THIS DISPLAYS..weep
        }

        return (int)health;


    }


    // IF ENEMY IS HIT, SAVE THE HEALTH VARIABLE FROM THE RESPAWNED POOL SCRIPT
    //****************************
    public void Update_GameObject_Health_OnHit(GameObject g, int health)
    {

        for (int i = 0; i < (arraySize); i++) 
        {
            if (levelObjectArray[i] == g)
            {
                levelData[i, 14] = (float) health;

                break;
            }

        }
       
    }



    

    // LOOP THOUGH MAIN LEVEL OBJECT ARRAY
    //****************************
    void Check_If_Objects_Are_Visible()
    {

        // ARRAY COLUMN DATA
        // 0  = Tag ID
        // 1  = Object Name ID
        // 2  = LOD MESH IDENTIFIER
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
        // 15 = TARGET IDENTIFIER - gets set to 1 & 2 for near and far id sprite phases (Only for Enemies)
        // 16 = Large Object Identifier - set to 1 if classed as large and troublesome for culling - was 'Unique Integer Identifier '
        // 17 = Looping Sounds used - set to 1 or greater if uses looping sfx..increments so can identifiy gameobject

        arraySize = (levelData.Length / arrayLoopSize); // GET NUMBER OF DATA ROWS/LINES 

        cameraScript.UpdateCameraFrustrum();

        camView = cameraScript.camView;

        // for 2 vector distance checking
        cameraPosition.x = camera.transform.position.x;
        cameraPosition.y = camera.transform.position.z;
       
        bool inView = false;

        for (int i = 0; i < (arraySize); i++) // Count rows.
        {
          
            float exists = levelData[i, 13];

       

            if (exists > -1) // IF NOT PERMANENTLY DESTROYED
            {


                inView = false;

                int tempID = ((int)(levelData[i, 0]));
                float x = levelData[i, 3];
                float y = levelData[i, 4];
                float z = levelData[i, 5];
                //float bx = levelData[i, 15];
                //float by = levelData[i, 16];

                objectPosition = new Vector2(x, z);

                float dist = Vector2.Distance(objectPosition, cameraPosition);



                if (dist < clipDistance) // public ClipDistance now oscillates with camera angle in camera code (raycast)
                {

                    if (levelData[i, 16] == 1) // First check for Objects previously tagged as 'LARGE'  - too large for pivot/frustrum culling
                    {
                        inView = true;
                    }
                    else
                    {
                        inView = Check_If_Object_Is_In_Frustrum(objectPosition);
                    }

                    if (inView)
                    {

                        if (exists == 0)
                        {
                            CreateNewObject(i); // SPAWN A GAME OBJECT it is in view

                            //CHECK IF HAS LOD MESH
                            if (levelData[i, 2] == 1.0f)
                            {
                                Perform_LOD(levelObjectArray[i], dist);
                            }
                        }

                        else // If the object now exists we need to update the X & Y positions in case it is a moving entity
                        {

                          
                            //CHECK IF HAS LOD MESH
                            if (levelData[i, 2] == 1.0f)
                            {
                                Perform_LOD(levelObjectArray[i], dist);
                            }



                            //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' CODE HERE!!!!- may endup making a case select for different types of object
                            //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' CODE HERE!!!! 

                            switch (tempID)
                            {

                                case 0://  NEUTRAL  STATIC MAP OBJECTS


                                    // NO UPDATING OF TRANSFORMS AND ROTATION NEEDED


                                    break;


                                case 1: // NEUTRAL  MOBILE- positions need constant updating as we move them out of view box to ' Virtually' destroy them

                                    levelData[i, 3] = levelObjectArray[i].transform.position.x; levelData[i, 4] = levelObjectArray[i].transform.position.y;
                                    levelData[i, 5] = levelObjectArray[i].transform.position.z;

                                    break;



                                case 2: // ENEMIES  STATIC -  // NO UPDATING OF TRANSFORMS AND ROTATION NEEDED

                                    HUD_Targeting(i, dist, false);
                                  

                                    break;


                                case 3: // ENEMIES MOBILE - can move for the most part and so need X & Y's updating.. and maybe a few other things

                                    //HUD_Targeting(i, dist, false);

                                                                       
                                    levelData[i, 3] = levelObjectArray[i].transform.position.x; levelData[i, 4] = levelObjectArray[i].transform.position.y;
                                    levelData[i, 5] = levelObjectArray[i].transform.position.z;
                                   

                                    break;


                                case 4: // COLLECTABLE PICKUP 

                                    // IF TagID = ADDON  - it can be collected - and needs to be processed differently if so
                                    //if (exists == -1) // This value gets set to -1 in Addons Propulsion script Update() when it is collected so it can no longer be 
                                    //processed by this Viewport Checker using the levelObjectArray[] list, but I still want to draw it as it is 
                                    //now attached to the player (-1 'Exists' switch declares this) so I need to update its X & Y transforms so that it
                                    //stays in camera view along with the player(or else it will disappear as it would be using it's original X & Y positions
                                    //that were placed in the Unity Editor - obviously non moving Props and stuff are simpler to manage as they remain static.
                                    //*** SET MOVING ENEMIES TO ANOTHER VALUE!!! -2 would be good
                                    // {
                                    //levelData[i, 3] = levelObjectArray[i].transform.position.x; levelData[i, 4] = levelObjectArray[i].transform.position.y;
                                    // levelData[i, 5] = levelObjectArray[i].transform.position.z;
                                    // }
                                    break;



                            }


                            //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' END!!!!
                            //*** SPECIAL ADDON 'REAL-TIME TRANSFORM UPDATE' END!!!!


                        }

                    }
                    else
                    {
                        if (exists == 1)
                        {
                            if (levelData[i, 15] > 0f)
                            {
                                if (tempID == 2 )//|| tempID == 3)
                                {
                                    HUD_Targeting(i, dist, true); // THIS WILL ENEMY FREE TARGET ID
                                }
                            }

                            // If has looping sound sfx
                            if (levelData[i, 17] > 0) { Stop_Looping_SFX(levelObjectArray[i], levelData[i, 17]); }

                            DestroyCurrentObject(levelObjectArray[i]); levelObjectArray[i] = null; levelData[i, 13] = 0;
                        }

                    }
                }

                else
                {
                    if (exists == 1)
                    {
                        if (levelData[i, 15] > 0f)
                        {
                            if (tempID == 2 )//|| tempID == 3)
                            {
                                HUD_Targeting(i, dist, true); // THIS WILL ENEMY FREE TARGET ID
                            }
                        }

                        // If has looping sound sfx
                        if (levelData[i, 17] > 0) { Stop_Looping_SFX(levelObjectArray[i], levelData[i, 17]); }

                        DestroyCurrentObject(levelObjectArray[i]); levelObjectArray[i] = null; levelData[i, 13] = 0;

                       
                    }

                }
            }

        }

        //GameObject[] gos = activePoolObjectList.ToArray();            
        //StaticBatchingUtility.Combine(gos, StaticBatchingRoot);
        //cullObject.SetActive(false);
    }



    void Perform_LOD(GameObject g, float dist)
    {

        if (dist < 210f)
        {
            g.transform.renderer.enabled = true;

            foreach (Transform child in g.transform)
            {
                child.renderer.enabled = false;
              
                break;
            }
        }
        else
        {
            g.transform.renderer.enabled = false;

            foreach (Transform child in g.transform)
            {
                child.renderer.enabled = true;
               
                break;
            }

        }
    }


    








    # endregion

    # region ++ Make and Destroy Instances 

	// ARRAY COLUMN DATA
	// 0  = Tag ID
	// 1  = Object Name ID
    // 2  = LOD MESH IDENTIFIER
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
    // 15 = TARGET IDENTIFIER - gets set to 1 & 2 for near and far id sprite phases (Only for Enemies)
    // 16 = Large Object Identifier - set to 1 if classed as large and troublesome for culling - was 'Unique Integer Identifier '
    // 17 = Looping Sounds used - set to 1 or greater if uses looping sfx..increments so can identifiy gameobject


    // CREATE NEW OBJECT FROM OBJECT POOL
    //****************************
	public void CreateNewObject(int i )
	{
		// FINAL BUILD DOES NOT NEED THESE LOCAL VARIABLES - they're here for current convenience
	
		int tagID = (int)levelData[i, 0];
		int tempObjectID = (int)levelData[i, 1];
		//string tempstring = ReturnObjectIdString (tempObjectID);
		//int LOD ID = (int)levelData[i, 2];
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
		//float bx = levelData[i, 15];
		//float by = levelData[i, 16];

		Vector3 posVector = new Vector3 (px, py, pz);
		//Vector3 scaleVector = new Vector3 (scx,scy,scz);
		Quaternion rot;
		rot.x = rx;
		rot.y = ry;
		rot.z = rz;
		rot.w = rw;


		// THIS IS THE 'GAME OBJECT' ARRAY - it is synched to the 'DATA' array index

		levelObjectArray[i] = ObjectPoolManager.CreatePooled (objectPoolList[tempObjectID], posVector, rot, tagID);// Added a tagID to pass here - probably never going to be needed :[
        
        if (levelObjectArray[i].layer == 12) // Enemies
        {
            activeList.Add(levelObjectArray[i]); // Add this list for proximity checks for player missiles.. to do damage if nearby etc..
        }
        //levelObjectArray[i].renderer.castShadows = true;
        //levelObjectArray[i].renderer.receiveShadows = false;
        //levelObjectArray[i].SetActive(true);


       
	}


    // STOP LOOPING SOUNDS
    //****************************
    void Stop_Looping_SFX(GameObject tempObject, float tempID)
    {

        int i = (int)tempID;

        switch (i)
        {
            case 1 :
                soldierScript = tempObject.GetComponent<ScriptSoldier>();
                if (soldierScript.sfxArrayIndex > 0) // sfxArrayIndex is set to -1 after looping sound stopped
                {
                    soldierScript.Stop_Looping_SFX();
                }
                break; // "enemy_soldier": machine gun sfx loops
        }


    }


    // HIDE OBJECT FROM THE WORLD VIEW
    //****************************
	void DestroyCurrentObject(GameObject tempObject)
	{
        activeList.Remove(tempObject);
        // LOOPING SOUNDS.. if object uses looping sounds we need to know for culling to switch them off directly
       

        tempObject.SetActive(false);
        ObjectPoolManager.DestroyPooled (tempObject); // this ONLY de-activates the object and places back in the POOL LIBRARY

	}



    // PERMANENTLY REMOVE OBJECT FROM THE GAME
    //****************************
    public void Permanently_Destroy_GameObject(GameObject tempObject)
    {

        activeList.Remove(tempObject); 

        for (int i = 0; i < (arraySize); i++) // Count rows.
        {
            if (levelObjectArray[i] == tempObject)
            {
               

                // If has looping sound sfx
                if (levelData[i, 17] > 0) { Stop_Looping_SFX(levelObjectArray[i], levelData[i, 17]); }

                // NOW SET LEVEL DATA ARRAY TO NEGATIVE VALUE SO IT WILL NEVER SPAWN AGAIN

                levelData[i, 13] = -99; // Will now be ignored by  'IN VIEW' code

                if (levelData[i, 15] > 0) // If has Target ID sprite following it
                {
                    //Free up far targetID sprite
                    targetIDManagerScript.Free_TargetID_Sprite(levelObjectArray[i].transform);
                }
                              
                levelObjectArray[i] = null;

                break;
            }

        }


        //tempObject.SetActive(false);

        ObjectPoolManager.DestroyPooled(tempObject); // this ONLY de-activates the object and places back in the POOL LIBRARY

     


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

    # region ++ Temporary debug stuff
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





    //***********************

    private string Check_If_Nasty_Merlin_Prefabs_Are_Causing_Problems_Again(string s, Transform t) // Called in Awake where Level Data is sorted
    {
      

        foreach (Transform child in t)
        {
            if (child.name == s)
            {
                child.parent = null;
                DestroyImmediate(t.gameObject);
                //tempObject = child.gameObject;
                Debug.Log(" Nasty Merlin duplicate object deleted: " + child.name);

                return "NULL";
            
            }

        }

        return s;

    }




    //***********************
    void Debug_3dsmax_Level_Objects_Remover()
    {

        GameObject[] objectsArray = FindObjectsOfType(typeof(GameObject)) as GameObject[]; // Temporary Array

       
        foreach (GameObject objects in objectsArray)
        {
            //----------------------

            //tag = "";
            string tempString = "";
            int remove = 0;

            //----------------------

            if (objects.name.Length > 3)
            {
                tempString = (objects.name.Substring(0, 4));
                //Debug.Log(" Name: " + tempString);
                //Debug.Log(" Name: " + objects.name);
            }

            switch (tempString)
            {
                case "bunk": remove = 1; break;
                case "sand": remove = 1; break;
                case "tent": remove = 1; break;
                case "watc": remove = 1; break;
                case "lo_B": remove = 1; break;
                case "lo_M": remove = 1; break;
                case "lo_R": remove = 1; break;
                case "lo_S": remove = 1; break;
                case "lo_T": remove = 1; break;

                case "barr": remove = 1; break;
                case "crat": remove = 1; break;
                case "Lamp": remove = 1; break;
                case "Tele": remove = 1; break;
                case "Pylo": remove = 1; break;
                case "lo_W": remove = 1; break;
                case "Rock": remove = 1; break;
                case "Sign": remove = 1; break;
                case "sign": remove = 1; break;

                case "tree": remove = 1; break;
                case "wall": remove = 1; break;
                case "enem": remove = 1; break;

            }


            if (remove == 1)
            {
                Destroy(objects); // DESTROY EDITOR OBJECT no longer needed - will be respawned by the Camera Viewport Manager code below in Update()
            }

        }

        objectsArray = null;

    }


    # endregion



}
