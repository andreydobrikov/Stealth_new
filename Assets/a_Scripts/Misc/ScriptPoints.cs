using UnityEngine;
using System.Collections;

public class ScriptPoints : MonoBehaviour
{


    public int points;

    public Vector3 tPos; // Original target object position
    private UILabel UI;

    public string pointsText;
   

    private GameObject playerCamera;
    private GameObject NGUICamera;
    private UIRoot uirootScript;
    //private float multiplier;

    private float yOffset;
    private float alphaStep;
    private float alphaValue;

    public bool reset;


   
    private ScriptPointsManager pointsManagerScript;


    //---------------------
    void Awake()
    {
        playerCamera = GameObject.Find("PlayerCamera");


        NGUICamera = GameObject.Find("Camera");

        GameObject tempObject = GameObject.Find("UI Root (2D)");
        uirootScript = tempObject.GetComponent<UIRoot>(); //

        UI = GetComponent<UILabel>();

        tempObject = GameObject.Find("A_Null_Labels");
        pointsManagerScript = tempObject.GetComponent<ScriptPointsManager>(); //



        yOffset = 12f;

        alphaStep = 0.33f;

    }



    //---------------------
    void Start()
    {

       // multiplier = uirootScript.manualHeight / Screen.height; // Must be in Start() to update correctly

        Vector3 pos = new Vector3(0, 0, 0);
        pos.x = 1.1f;
        pos.y = 1.1f;

        transform.position = NGUICamera.camera.ViewportToWorldPoint(pos);
    }



    //---------------------
    void Update()
    {


        //-----------
        if (reset)// reset from manager script
        {
            alphaValue = 0;
            UI.alpha = 1.0f;
            reset = false;
        }

        else
        {



            //-------------
            tPos.y = tPos.y + (yOffset * Time.deltaTime);

            Vector3 pos = playerCamera.camera.WorldToViewportPoint(tPos);

            //pos.x = pos.x * multiplier;
            //pos.y = pos.y * multiplier;



            if (pos.z > 0) // Must be greater than World Viewport zero to be visible
            {
                pos.z = 0;

                // Clamp sprites to respond only to near 'in view' positions
                if ((pos.x < 1.1f && pos.x > -0.1f) & (pos.y < 1.1f && pos.y > -0.1f))
                {
                    transform.position = NGUICamera.camera.ViewportToWorldPoint(pos);
                }
                else
                {
                    // Bodge:  makes sure we can see code is clamping sprite positions
                    // In order to stop 'Phantom' sprites showing when enemy is behind camera 

                    transform.position = NGUICamera.camera.ViewportToWorldPoint(pos);

                    pos.x = 1.1f;
                    pos.y = 1.1f;

                    transform.position = NGUICamera.camera.ViewportToWorldPoint(pos);
                }

            }
            else
            {
                transform.position = tPos;

            }


            //-----------
            if (UI.alpha > 0f)
            {
                alphaValue = alphaValue + (alphaStep * Time.deltaTime);
                UI.alpha = (1.0f - alphaValue);

            }
            else
            {
                UI.alpha = 0f;


                pointsManagerScript.Reset_Points_Label(this.gameObject);


            }






        }

    }



}