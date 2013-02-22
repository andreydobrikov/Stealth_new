using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptTargetID : MonoBehaviour
{

    
    private float rotz;

    public Transform targetFocus;// Object this sprite will be tracking

    public bool spinTrigger;// Trigger to set sprite rotating - only 'near' sprites do this

    private GameObject playerCamera;
    private GameObject NGUICamera;
    private GameObject cube;
    private UIRoot uirootScript;
    //private float multiplier;
    private bool visible;



    //---------------------
    void Awake()
    {
        playerCamera = GameObject.Find("PlayerCamera");
      
        NGUICamera = GameObject.Find("Camera");

        GameObject tempObject = GameObject.Find("UI Root (2D)");
        uirootScript = tempObject.GetComponent<UIRoot>(); //

      

    }



    //---------------------
    void Start()
    {

        //multiplier = uirootScript.manualHeight / Screen.height; // Must be in Start() to update correctly

        Vector3 pos = new Vector3(0,0,0);
        pos.x = 1.1f;
        pos.y = 1.1f;

        transform.position = NGUICamera.camera.ViewportToWorldPoint(pos);
    }

  

    //---------------------
    void Update()
    {


        if (targetFocus)
        {
            Vector3 tPos = targetFocus.position;

            tPos.y = tPos.y + 0.5f;

            Vector3 pos = playerCamera.camera.WorldToViewportPoint(tPos);
           
            // pos.x = pos.x *multiplier;..was a fix for an older version of NGUI
            // pos.y = pos.y *multiplier;
            //pos.z = 0;

         
            // Clamp sprites to respond only to near 'in view' positions
            // If 'Z' is  Minus then it is behind world viewpoint
            if (pos.z > 0)
            {
                pos.z = 0;

                if ((pos.x < 1.1f && pos.x > -0.1f) && (pos.y < 1.1f && pos.y > -0.1f))
                {

                    transform.position = NGUICamera.camera.ViewportToWorldPoint(pos);

                    if (spinTrigger)
                    {
                        Spin_Sprite();
                    }

                }
                else
                {
                   

                    transform.position = targetFocus.transform.position; // simply stick on its parent transform as it is out of view

                }
            }
            else
            {

             

                transform.position = targetFocus.transform.position; // simply stick on its parent transform as it is out of view

            }


        }
    }



    //---------------------
    void Spin_Sprite()
    {

       rotz = rotz + 45f * Time.deltaTime;

       Vector3 tempVector = new Vector3(0, 0, (int)rotz);

       transform.rotation = Quaternion.Euler(tempVector);

    }


   




}
