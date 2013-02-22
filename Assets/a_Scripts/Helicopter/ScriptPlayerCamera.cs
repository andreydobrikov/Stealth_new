using UnityEngine;
using System.Collections;




public class ScriptPlayerCamera : MonoBehaviour
{

	private int debug = 1; // My own debug switch
	private int fps;
	private int MsecsCount;
	private float Msecs;

	private ScriptAInputManager inp;
    private ScriptASceneManager asm;
	private FXManager FXManagerScript;

	private Transform target; //Camera Target
	private Transform blackScreen;
	private bool blackScreenOn = false;
	private GameObject tempObject;
	private Color fogColor;

	public Vector3 scrTopLeft;
	public Vector3 scrTopRight;
	public Vector3 scrBottomLeft;
	public Vector3 scrBottomRight;
	public Vector3 scrCentre;
	public Vector3 scrTopCentre;
	public Vector3 scrBottomCentre;
    private float  scrCentreWidth;
    private float  scrHeight;

    public float clipDistance;

    private Vector2[] FrustrumView;
    public Vector2[] camView;
	
    private Transform frustrum;
  
	

	//--------------------------------------------------------
	// Use this for initialization
	//--------------------------------------------------------
    //GameObject cube;
	void Awake()
	{
        //cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        layerMask = 1 << 13; // Terrain layer hits only



      	//  DontDestroyOnLoad (transform.gameObject);// KEEP CAMERA AND ALL CHILDREN THROUGH OUT GAME CYCLE

		//  CAMERA HAS A FRUSTRUM CHILD ATTACHED as a mesh so i can use the verts to calculate waht objects are in view
		foreach(Transform child in transform)
		{
            //For fadeScreens
			//if(child.name == "blackScreen") { blackScreen = child; child.renderer.enabled = true; blackScreenOn = true; break; }

            if (child.name == "cameraFrustrum_prefabs")
            {
                foreach (Transform c in child.transform)
                {
                    frustrum = c;
                    frustrum.parent = null;
                    frustrum.parent = transform;
                   
                    Destroy(child.gameObject); //Debug.Log(" Found Camera Frustrum");

                    Mesh mesh = frustrum.GetComponent<MeshFilter>().mesh; 
                    Vector3[] vertices = mesh.vertices;
                    FrustrumView = new Vector2[vertices.Length]; //this remains static
                    camView = new Vector2[vertices.Length]; // this will be updated

                    int i = 0;
                    while (i < vertices.Length)
                    {
                       //Add verts to camera frustrum custom array
                        FrustrumView[i].x = vertices[i].x;
                        FrustrumView[i].y = vertices[i].z; // Convert the vector2 Y to the vector3 Z
                       //Debug.Log(" Verts: " + FrustrumView[i]);
                        i++;       
                    }

                    //c.renderer.enabled = false;
                    Destroy(c.gameObject);

                    // REORDER VERTS TO SEQUENTIAL;
                    i = 0;

                    while (i < FrustrumView.Length)
                    {
                        if (i == 0) { camView[i].x = FrustrumView[0].x; camView[i].y = FrustrumView[0].y; }
                        if (i == 1) { camView[i].x = FrustrumView[3].x; camView[i].y = FrustrumView[3].y; }
                        if (i == 2) { camView[i].x = FrustrumView[1].x; camView[i].y = FrustrumView[1].y; }
                        if (i == 3) { camView[i].x = FrustrumView[2].x; camView[i].y = FrustrumView[2].y; }

                        i++;
                    }

                    FrustrumView = camView;
                    camView = null;
                    camView = new Vector2[vertices.Length]; // this will be updated
                    //i = 0;
                    //  while (i < FrustrumView.Length)
                    // {

                          //Debug.Log(" Verts " + i + ": " + FrustrumView[i] );

                       // i++;
                      
                    //  }

                }

            }

		}

		//Find and get INPUT MANAGER SCRIPT - Totally necessary :)
		tempObject = GameObject.Find ("A_GameWrapper");
		inp = tempObject.GetComponent<ScriptAInputManager> (); //

        tempObject = GameObject.Find("A_Scene_Manager");
        asm = tempObject.GetComponent<ScriptASceneManager>(); //
     
        tempObject = null;

        Set_Fog();
       
	}








    //***********************	
	public void Set_Viewport_Bounds(float zDistance = 20) // Called from HUD Script in Awake() 
	{
		scrBottomLeft = camera.ScreenToWorldPoint (new Vector3 (0, 0, camera.nearClipPlane + zDistance));

		scrTopLeft = camera.ScreenToWorldPoint (new Vector3 (0, Screen.height, camera.nearClipPlane + zDistance));

		scrBottomRight = camera.ScreenToWorldPoint (new Vector3 (Screen.width, 0, camera.nearClipPlane + zDistance));

		scrTopRight = camera.ScreenToWorldPoint (new Vector3 (Screen.width, Screen.height, camera.nearClipPlane + zDistance));

		scrCentre = camera.ScreenToWorldPoint (new Vector3 ((float)Screen.width / 2f, (float)Screen.height / 2f, camera.nearClipPlane + zDistance));

		scrTopCentre = camera.ScreenToWorldPoint (new Vector3 ((float)Screen.width / 2f, Screen.height, camera.nearClipPlane + zDistance));

		scrBottomCentre = camera.ScreenToWorldPoint (new Vector3 ((float)Screen.width / 2f, 0, camera.nearClipPlane + zDistance));
	}




	//--------------------------------------------------------
	// START
	//--------------------------------------------------------
	void Start()
	{

	}


	//--------------------------------------------------------
	// FOG
	//--------------------------------------------------------
	void Set_Fog()
	{

		// SET FOG Colour
		RenderSettings.fog = true;
		//fogColor = new Color (0.8F, 0.0F, 0.0F, 1.0F); // Set to Reddish
		//RenderSettings.fogColor = fogColor;
		//Need to also update and re-adjust fog distance here
		RenderSettings.fogStartDistance = camera.farClipPlane/2f;
        RenderSettings.fogEndDistance = camera.farClipPlane - (camera.farClipPlane/20f);

	}


	

    // Update Camera Frustrum - called from Pool/Scene Object Handler script
    public void UpdateCameraFrustrum()
    {
       
        Vector3 tv;

        int i = 0;
        while (i < camView.Length)
        {
            camView[i].x = FrustrumView[i].x;
            camView[i].y = FrustrumView[i].y;

            tv = new Vector3(camView[i].x, transform.position.y, camView[i].y);

            // Some Vector2 to Vector3 bodging going on here
            tv = transform.TransformPoint(tv);

            camView[i].x = tv.x;
            camView[i].y = tv.z;
                    
            i++;
        }
  
    }



	
	//***********************	
   	void Update()
	{
		if(debug == 1) { CalculateRealFPS (); }
       // AdjustFrustrumClippingDistance();
	}




    //***********************			
    //CHECK FOR MOUSE COORDS - not using right now Jaime
    //***********************	

    private int layerMask ; // Terrain layer hits only
    private RaycastHit hit;
    private Ray ray;

    void AdjustFrustrumClippingDistance()
    {
       
        ray = camera.ViewportPointToRay(new Vector3(0.5F, 1.1F, 0));
      
        if (Physics.Raycast(ray,  out hit, Mathf.Infinity, layerMask))
        {

            asm.clipDistance = Vector3.Distance(hit.point, transform.position);

           // cube.transform.position = hit.point;
        }

        else
        {
            asm.clipDistance = camera.farClipPlane;

        }

 

    }















	public Texture2D fadeOutTexture; // Set in Editor
	private float fadeSpeed = 0.3f;
	private float fadeAlpha = 1.0f;
	private int drawDepth = -2000;
	public int fadeDir = -2;

	//***********************		
	void OnGUI()
	{

		// fadeIn  =  1 ..Clear to black
		// fadeOut = -1 ..Black to Clear

		// BLACK SCREEN FADER

		if(fadeDir == 0) { } else { Do_Fade_Screens (); }
		//if(debug == 1) { GUI.depth = -1001; GUI.Label (new Rect (20, 20, 500, 250), "FrameRate:  " + fps); }
	}

	//***********************		
	public void Set_Fade_Mode(string s, float speed = 0.3f)
	{
		fadeSpeed = speed;
		if(s == "fadeIn") { fadeDir = 1; }
		if(s == "fadeOut") { fadeDir = -1; }

	}


	// CALLED FROM OnGUI()
	//***********************	
	private float GUITimeout; // bodge for NGUI draw order

	void Do_Fade_Screens()
	{

		switch(fadeDir)
		{

			case -1: // FADE OUT
				{
					if(blackScreenOn) { blackScreenOn = false; blackScreen.renderer.enabled = false; }

					fadeAlpha += fadeDir * fadeSpeed * Time.deltaTime;
					//fadeAlpha = Mathf.Clamp01 (fadeAlpha);

					Color tempColor = new Color ();
					tempColor.a = fadeAlpha;

					GUI.color = tempColor;
					GUI.depth = drawDepth;
					GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeOutTexture);



					if(fadeAlpha <= 0) { fadeDir = 0; fadeAlpha = 0f; }// Black to Clear
				}
				break;

			case 1: // FADE IN 
				{


					fadeAlpha += fadeDir * fadeSpeed * Time.deltaTime;
					//fadeAlpha = Mathf.Clamp01 (fadeAlpha);

					Color tempColor = new Color ();
					tempColor.a = fadeAlpha;

					GUI.color = tempColor;
					GUI.depth = drawDepth;
					GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeOutTexture);



					if(fadeAlpha >= 1) { fadeDir = -2; fadeAlpha = 1f; FXManagerScript.Stop_All_Sound (); blackScreenOn = true; blackScreen.renderer.enabled = true; GUITimeout = Time.time + 0.35f; }// Clear to black -2 sets OnGUI to draw in solid black only

				}
				break;
			case -2: // SHOW SOLID BLACK
				{
					if(Time.time < GUITimeout)
					{
						fadeAlpha = 1f;
						Color tempColor = new Color ();
						tempColor.a = fadeAlpha;

						GUI.color = tempColor;
						GUI.depth = drawDepth;
						GUI.DrawTexture (new Rect (0, 0, Screen.width, Screen.height), fadeOutTexture);
					}
				}
				break;
		}

	}



    //***********************	
	void CalculateRealFPS()
	{
		Msecs = Msecs + Time.deltaTime;
		MsecsCount++;
		if(Msecs >= 1.0f) { Msecs = 0; fps = MsecsCount; MsecsCount = 0; }
	}




} // Class end
