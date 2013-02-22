using UnityEngine;
using System.Collections;

public class ScriptMainMenu : MonoBehaviour {

	public int levelNumber = 1;
	
	private Transform camera;
	private GameObject opMenu;
	
	
	private ScriptPlayerCamera cameraScript;
	private GameObject A_FX_Manager;
	private FXManager FXManagerScript;
	private ScriptAInputManager inp;
	//private ScriptHUDDoors HUDDoorsScript;
	//private ScriptOptionsMenu optionsMenuScript;
	

	private float scrWidth; private float widthPercent; private float scrLeft; private float scrRight; private float scrTopCentre;// This is '3D' width of screen not Pixels
	private float scrHeight; private float heightPercent; private float scrBottom; private float scrTop; private float scrBottomCentre;// Ditto '3D'


	private int butBeginOver ;
	private int butHelpOver ;
	private int butMoreOver ;
	private int butScoreOver ;
	private int butShopOver ;
	private int butOptionsOver ;


	private Vector3 menuPosition;
	private Material menuMat;



	// Y Oscillation Variables
	private float buoyTime;
	private float buoyY;
	private float buoyMultiplier = 0.02f;
	private float buoyWaveLength = 90.0f; // how many degrees to count per second using Time.deltaTime - Note not the same formula as 'Time length' * Time.deltaTime
	private float originalPositionY; // Need to conserve the original Y position for accurate delta time between systems

	
	public Texture arrowTex; // Mouse Cursor
	private float arrowScaleX;
	private float arrowScaleY;
	public bool showArrow = false;
	public bool doFloat = false;

	public bool menuActive = false;

	private Transform menuBackground;
	//private Transform menuOptionsBackground;

	private Transform butBegin;
	private Transform butBeginLit ;
	private Transform butHelp ;
	private Transform butHelpLit;
	private Transform butMore ;
	private Transform butMoreLit;
	private Transform butOptions;
	private Transform butOptionsLit;
	private Transform butScore;
	private Transform butScoreLit;
	private Transform butShop;
	private Transform butShopLit;


	// NGUI Handles for Activating various screens to be activated when needed
	private GameObject NGUIRoot;
	private GameObject panelShop;
	private GameObject panelScore;
	private GameObject panelHelp;
	private GameObject panelMore;
	private GameObject panelOptions;
	private GameObject menuOptionsBackground;
	private GameObject menuOptionsBackDarkEdges;
	

	//***********************		
	void Awake()
	{

		showArrow = false;
	    // SET Arrow scale according to percentage of target resolution of 1920 pixels width
		arrowScaleX = (100f / 1920f) * Screen.width;
		arrowScaleY = (arrowTex.height / 80f) * arrowScaleX;
		arrowScaleX = (arrowTex.width / 80f) * arrowScaleX;




		menuActive = false;


		NGUIRoot = GameObject.Find ("UI Root (2D)");

		panelShop = GameObject.Find ("panelShop");
		panelScore = GameObject.Find ("panelScore");
		panelHelp = GameObject.Find ("panelHelp");
		panelMore = GameObject.Find ("panelMore");
		panelOptions = GameObject.Find ("panelOptions");


		//---------------------------------------------
		// GET NGUI EVENT LISTENER to check for Raycasts and Button Presses etc.
		//Set_NGUI_Listeners (); // add hooks for Button events
		//---------------------------------------------


		//Find and get INPUT MANAGER SCRIPT - Totally necessary :)
		GameObject tempObject = GameObject.Find ("A_Input_Manager");
		inp = tempObject.GetComponent<ScriptAInputManager> (); //

		tempObject = GameObject.Find ("HUDDoors");
		//HUDDoorsScript = tempObject.GetComponent<ScriptHUDDoors> (); // Get Handle to Scene Manager Script to access ARRAYS

		

		// GET OPTIONS SCREEN OBJECT
		opMenu = GameObject.Find ("menuOptionsFrame");
		//optionsMenuScript = opMenu.GetComponent<ScriptOptionsMenu> ();

		// GET OPTIONS BACK GROUND OBJECT
		menuOptionsBackground = GameObject.Find ("menuOptionsBackground");
		
		// THE SCREEN EDGE EFFECT FOR THE BACKGROUND LAYER
		menuOptionsBackDarkEdges = GameObject.Find ("menuOptionsBackEdges");


		// WORKS CLOSELY WITH THE SCENE MANAGER AND SO NEEDS HANDLES

		A_FX_Manager = GameObject.Find ("A_FX_Manager");
		if(A_FX_Manager)
		{
			FXManagerScript = A_FX_Manager.GetComponent<FXManager> (); // Get Handle to Scene Manager Script to access ARRAYS
		}

		//Find and get Camera
		tempObject = GameObject.Find ("Player_Camera");
		camera = tempObject.transform;
		cameraScript = tempObject.GetComponent<ScriptPlayerCamera> (); // Get Handle to Scene Manager Script to access ARRAYS

		
		// FIND HUD MATERIALS
		int i = 0; string s = "Nano_MainMenu";
		Material[] tempList = Resources.FindObjectsOfTypeAll (typeof (Material)) as Material[];
		foreach(Material mat in tempList) { if(mat.name == s) { menuMat = mat; Debug.Log ("Found Main Menu Mat"); break; } i++; }


		// GET BACK GROUND OBJECT
		tempObject = GameObject.Find ("mainMenuBackground");
		menuBackground = tempObject.transform;


		

		

		


		string tempString;

		//Component[] thisTransform;
		//thisTransform = GetComponentsInChildren<Transform> ();

		foreach(Transform child in transform)
		{

			child.renderer.sharedMaterial = menuMat;
			tempString = child.name;



			switch(tempString)
			{
				case "butBegin": { butBegin = child; } break;
				case "butBeginLit": { butBeginLit = child; child.renderer.enabled = false; child.collider.enabled = false; child.gameObject.layer = 31; } break;
				case "butHelp": { butHelp = child; } break;
				case "butHelpLit": { butHelpLit = child; child.renderer.enabled = false; child.collider.enabled = false; child.gameObject.layer = 31; } break;
				case "butMore": { butMore = child; } break;
				case "butMoreLit": { butMoreLit = child; child.renderer.enabled = false; child.collider.enabled = false; child.gameObject.layer = 31; } break;
				case "butOptions": { butOptions = child; } break;
				case "butOptionsLit": { butOptionsLit = child; child.renderer.enabled = false; child.collider.enabled = false; child.gameObject.layer = 31; } break;
				case "butScore": { butScore = child; } break;
				case "butScoreLit": { butScoreLit = child; child.renderer.enabled = false; child.collider.enabled = false; child.gameObject.layer = 31; } break;
				case "butShop": { butShop = child; } break;
				case "butShopLit": { butShopLit = child; child.renderer.enabled = false; child.collider.enabled = false; child.gameObject.layer = 31; } break;
			}

		}


		transform.eulerAngles = new Vector3 (0f, 0f, 3.5f);
		Hide_This_Object ();

	}







	//***********************		
	public void Hide_This_Object() // Called from INPUTS or any other script - simply repositions mesh to hide it
	{
		// Menu mesh
		cameraScript.Set_Viewport_Bounds (9f); // Must call in AWAKE Only
		menuPosition = cameraScript.scrCentre;
		transform.position = new Vector3 (menuPosition.x, (menuPosition.y + 10000f), menuPosition.z);
		transform.parent = camera;

		// Background
		Vector3 tp = cameraScript.scrTopCentre;
		menuBackground.transform.position = new Vector3 (tp.x, (tp.y + 10000f), tp.z);
		menuBackground.transform.parent = camera;

		// Options GUI Background
		//menuOptionsBackground.renderer.enabled = false;
		//foreach(Transform child in menuOptionsBackground.transform)	{child.renderer.enabled = false; }
		

		doFloat = false;
	}



	//***********************		
	public void Show_This_Object()// Called from INPUTS or any other script - simply repositions mesh to show it
	{
		// Menu mesh
		cameraScript.Set_Viewport_Bounds (9f); // Must call in AWAKE Only
		menuPosition = cameraScript.scrCentre;
		transform.position = new Vector3 (menuPosition.x, menuPosition.y, menuPosition.z);
		transform.parent = camera;
		originalPositionY = menuPosition.y;  

		// Background
		Vector3 tp = cameraScript.scrTopCentre;
		menuBackground.transform.position = new Vector3 (tp.x, (tp.y ), tp.z);
		menuBackground.transform.parent = camera;

		
				

		FXManagerScript.Play_Looping_Sound (8); //Play background sound

	

		doFloat = true;

	}





	//***********************		
	void Start () 
	{
		// THESE ITEMS SHOULD HAVE BEEN FOUND IN ALL RELEVANT OBJECT AWAKE() FUNCTIONS BY NOW AND SO CAN DE-ACTIVATED	

		menuOptionsBackground.SetActiveRecursively (false);// SET 'AFTER' RESETTING ANYTHING ELSE!!!!
		menuOptionsBackDarkEdges.SetActiveRecursively (false);// SET 'AFTER' RESETTING ANYTHING ELSE!!!!
		NGUIRoot.SetActiveRecursively (false);// SET 'AFTER' RESETTING ANYTHING ELSE!!!!

	
	}


	
	//***********************		
	void Update () 
	{

		if(menuActive)
		{
			Do_Menu();

		
		
		}

		if(doFloat) // Boolean set at 'Show Menu' function
		{

			//----------
			// Oscillating Y effect
			//----------
			buoyTime = buoyTime + (buoyWaveLength * Time.deltaTime); // roughly 90 degrees per second = 1 Y oscillation every 2 seconds
			float tempRad = (buoyTime * Mathf.Deg2Rad);
			buoyY = ((Mathf.Sin (tempRad) * buoyMultiplier));


			transform.position = new Vector3 (transform.position.x, originalPositionY + buoyY, transform.position.z); //Reset positions to original 

		}
	}


	void FixedUpdate()
	{

		if(menuActive)
		{
		
		}

	}


public bool optionsActive = false;
public bool menuGameBegin = false; // Starts load level screen
public bool levelBegin = false; // actually loads the scene selected or the default scene
public bool menuHelpBegin = false;
public bool menuMoreBegin = false;
public bool menuScoreBegin = false; 
public bool menuShopBegin = false; 
public bool menuOptionsBegin = false;

private int  menuStage = 0; // for incrementing menu stages

	// MAIN MENU CODE
	//***********************		
	void Do_Menu()
	{
		if(!levelBegin)
		{
			// ONLY DO THIS MENU IF OPTIONS SCREENS ARE 'OFF'
			if(!optionsActive) { if(showArrow) { Check_Menu_Buttons (); } }



         //SUB OPTIONS SCREENS ARE CONTROLLED FROM HERE:


			// THIS WILL LOAD THE LEVELS SCREEN
			if(menuGameBegin)
			{
				switch(menuStage)
				{
					case 0:
						{
							if(cameraScript.fadeDir == 0 | cameraScript.fadeDir == -2) //0 & -2 is finished fading to either black or is now solid black
							{
								Debug.Log ("Level Load Screen Activated");
								Hide_This_Object (); //  Hide menu

								opMenu.SetActiveRecursively (true); // SET 'BEFORE' CALLING ANYTHING ELSE!!!!
								//optionsMenuScript.Show_Options_Menu_Screen ();


								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)
								menuStage = 10;
							}

						}
						break;


					case 10:
						{
							/*if(!optionsMenuScript.optionsMenuActive)
							{
								optionsActive = false;
								menuGameBegin = false;
								menuStage = 0;
								opMenu.SetActiveRecursively (false);// SET 'AFTER' CALLING ANYTHING ELSE!!!!
								Show_This_Object (); //  Show menu
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

							}
                           */
						}

						break;


				}


				return;


			}




			//SHOP
			if(menuShopBegin)
			{
				switch(menuStage)
				{
					case 0:
						{				
							if(cameraScript.fadeDir == 0 | cameraScript.fadeDir ==-2) //0 & -2 is finished fading to either black or is now solid black

							{
								Debug.Log ("Shop Screen Activated");
								Hide_This_Object (); //  Hide menu
								opMenu.SetActiveRecursively (true); // SET 'BEFORE' CALLING ANYTHING ELSE!!!!
								//optionsMenuScript.Show_Options_Menu_Screen ();
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 10;
							}
				
						}
						break;


					case 10:

						{
							/*if(!optionsMenuScript.optionsMenuActive)
								{
									optionsActive = false;
									menuShopBegin = false;
									opMenu.SetActiveRecursively (false);// SET 'AFTER' CALLING ANYTHING ELSE!!!!
									Show_This_Object (); //  Show menu
									cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

									menuStage = 0;

								}
							*/
						}

						break;


				}


				return;
				

			}




			// MORE GAMES
			if(menuMoreBegin)
			{
				switch(menuStage)
				{
					case 0:
						{
							if(cameraScript.fadeDir == 0 | cameraScript.fadeDir == -2)
							{
								Hide_This_Object (); //  Hide menu
								opMenu.SetActiveRecursively (true); // SET 'BEFORE' CALLING ANYTHING ELSE!!!!
								//optionsMenuScript.Show_Options_Menu_Screen (); //  Show GUI Options 
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 10;
							}

						}
						break;
					case 10:
						{
							/*if(!optionsMenuScript.optionsMenuActive)
							{
								optionsActive = false;
								menuMoreBegin = false;
								opMenu.SetActiveRecursively (false);// SET 'AFTER' CALLING ANYTHING ELSE!!!!
								Show_This_Object (); //  Show menu
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 0;

							}
*/
						}

						break;
				}

				return;
			}




			// OPTIONS
			if(menuOptionsBegin)
			{
				switch(menuStage)
				{
					case 0:
						{
							if(cameraScript.fadeDir == 0 | cameraScript.fadeDir == -2)
							{
								Hide_This_Object (); //  Hide menu
								opMenu.SetActiveRecursively (true); // SET 'BEFORE' CALLING ANYTHING ELSE!!!!
								//optionsMenuScript.Show_Options_Menu_Screen (); //  Show GUI Options 
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 10;
							}

						}
						break;

					case 10:
						{
							/*if(!optionsMenuScript.optionsMenuActive)
							{
								optionsActive = false;
								menuOptionsBegin = false;
								opMenu.SetActiveRecursively (false);// SET 'AFTER' CALLING ANYTHING ELSE!!!!
								Show_This_Object (); //  Show menu
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 0;

							}
*/
						}

						break;
				}

				return;
			}




			//HI SCORE
			if(menuScoreBegin)
			{
				switch(menuStage)
				{
					case 0:
						{
							if(cameraScript.fadeDir == 0 | cameraScript.fadeDir == -2)
							{
								Hide_This_Object (); //  Hide menu
								opMenu.SetActiveRecursively (true); // SET 'BEFORE' CALLING ANYTHING ELSE!!!!
								//optionsMenuScript.Show_Options_Menu_Screen (); //  Show GUI Options 
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 10;
							}

						}
						break;
					case 10:
						{
							/*if(!optionsMenuScript.optionsMenuActive)
							{
								optionsActive = false;
								menuScoreBegin = false;
								opMenu.SetActiveRecursively (false);// SET 'AFTER' CALLING ANYTHING ELSE!!!!
								Show_This_Object (); //  Show menu
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 0;

							}
*/
						}

						break;
				}

				return;
			}




			// HELP
			if(menuHelpBegin)
			{
				switch(menuStage)
				{
					case 0:
						{
							if(cameraScript.fadeDir == 0 | cameraScript.fadeDir == -2)
							{
								Hide_This_Object (); //  Hide menu
								opMenu.SetActiveRecursively (true); // SET 'BEFORE' CALLING ANYTHING ELSE!!!!
								//optionsMenuScript.Show_Options_Menu_Screen (); //  Show GUI Options 
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 10;
							}

						}
						break;
					case 10:
						{
							/*if(!optionsMenuScript.optionsMenuActive)
							{
								optionsActive = false;
								menuHelpBegin = false;
								opMenu.SetActiveRecursively (false);// SET 'AFTER' CALLING ANYTHING ELSE!!!!
								Show_This_Object (); //  Show menu
								cameraScript.Set_Fade_Mode ("fadeOut", 0.6f); // Reshow Options Background Mesh (gets hidden in Show/Hide Menu by default)

								menuStage = 0;

							}
							 * */
						}
						break;
				}

				return;
			}



		}
		else
		{


			
			//if(levelBegin)
			//{
			//}







		}

	}








	//***********************	
	void Check_Menu_Buttons()
	{

		



	}



	//***************************
	
	void OnGUI()
	{

		

	}


	//***************************

	// CALLED FROM SUB MENU IF 'MAIN MENU' BUTTON PRESSED
	public void Reset_Main_Menu() 
	{
		menuStage = 0;
		optionsActive = false;
		menuGameBegin = false;    butBeginOver = 0; 
		menuHelpBegin = false;    butHelpOver= 0; 
		menuMoreBegin = false;    butMoreOver= 0; 
		menuScoreBegin= false;    butScoreOver= 0; 
		menuShopBegin =  false;   butShopOver= 0;  
		menuOptionsBegin= false;  butOptionsOver = 0;  

		
		butBegin.renderer.enabled = true; butBegin.collider.enabled = true; butBegin.gameObject.layer = 8;
		butBeginLit.renderer.enabled = false; butBeginLit.collider.enabled = false; butBeginLit.gameObject.layer = 31;
		butBeginOver = 0;

	
		butMore.renderer.enabled = true; butMore.collider.enabled = true; butMore.gameObject.layer = 8;
		butMoreLit.renderer.enabled = false; butMoreLit.collider.enabled = false; butMoreLit.gameObject.layer = 31;
		butMoreOver = 0;

	
		butShop.renderer.enabled = true; butShop.collider.enabled = true; butShop.gameObject.layer = 8;
		butShopLit.renderer.enabled = false; butShopLit.collider.enabled = false; butShopLit.gameObject.layer = 31;
		butShopOver = 0;

		
		butScore.renderer.enabled = true; butScore.collider.enabled = true; butScore.gameObject.layer = 8;
		butScoreLit.renderer.enabled = false; butScoreLit.collider.enabled = false; butScoreLit.gameObject.layer = 31;
		butScoreOver = 0;

		
		butOptions.renderer.enabled = true; butOptions.collider.enabled = true; butOptions.gameObject.layer = 8;
		butOptionsLit.renderer.enabled = false; butOptionsLit.collider.enabled = false; butOptionsLit.gameObject.layer = 31;
		butOptionsOver = 0;

	
		butHelp.renderer.enabled = true; butHelp.collider.enabled = true; butHelp.gameObject.layer = 8;
		butHelpLit.renderer.enabled = false; butHelpLit.collider.enabled = false; butHelpLit.gameObject.layer = 31;
		butHelpOver = 0;










	}







	//****************************************************************************************************
	//****************************************************************************************************
	//   NGUI DELEGATE STUFF FOR BUTTONS

	private GameObject levelBStart;



	// START LEVEL LOADING
	// PanelLevel GameObject
    /*
	void Set_NGUI_Listeners()
	{
		levelBStart = GameObject.Find ("buttonLevelLoad");
		UIEventListener.Get (levelBStart).onClick += Level_button_Start;


	}



	// START LEVEL LOADING... PanelLevel GameObject

	public void Level_button_Start(GameObject levelBStart)
	{
		
		levelBegin=true; // This Script's Main Update code will pick this up now and hand over to the InputManager for loading

		cameraScript.Set_Fade_Mode ("fadeIn", 0.3f);

		optionsActive = false; // Remember these booleans were active in Levels screen
		menuGameBegin = false;
			
		
		//optionsMenuScript.Hide_Options_Menu_Screen (); 

		Debug.Log ("STARTING LEVEL " + levelNumber );
	}

	
    */

}
