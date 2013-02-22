using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptTargetIDManager : MonoBehaviour {

    // HUD Target Identifier Sprites Manager:
    //
    //
    // This Script acts as a 'Mini-Object Pool' .. activating sprites only when needed and adding them to an 'Active List' to update

    private float rotz;

    public List<GameObject> farList = new List<GameObject>(); // Far enemies

    public List<GameObject> nearList = new List<GameObject>(); // urgent & close enemies  - rotating cursor

    public List<GameObject> activeList = new List<GameObject>(); // so we only loop through a smaller active list of sprites in the main update

    private ScriptTargetID targetIDScript; // access to sprite scripts

    private FXManager sfx; // Script for Sound & Particle fx manager..


    //private bool debug;
  

    //---------------------
    void Awake()
    {
        GameObject tempObject;

        UISprite targetID = GameObject.Find("Sprite (targetID)").GetComponent("UISprite") as UISprite;
        UISprite targetID2 = GameObject.Find("Sprite (targetID_far)").GetComponent("UISprite") as UISprite;
        for (int i = 0; i < 9; i++)
        {
            //rotating near sprite
            tempObject = Instantiate(targetID.gameObject, targetID.transform.position, targetID.transform.rotation) as GameObject;

            tempObject.transform.parent = transform;

            tempObject.SetActive(false);

            nearList.Add(tempObject);



            // Far sprite
            tempObject = Instantiate(targetID2.gameObject, targetID.transform.position, targetID.transform.rotation) as GameObject;

            tempObject.transform.parent = transform;

            tempObject.SetActive(false);

            farList.Add(tempObject);

        }

        // Add the current Editor sprites to the list too
        farList.Add(targetID2.gameObject);
        targetID2.gameObject.SetActive(false);


        nearList.Add(targetID.gameObject);
        targetID.gameObject.SetActive(false);



        tempObject = GameObject.Find("PlayerCamera");
        sfx = tempObject.GetComponent<FXManager>(); //


    }





    //---------------------
    // CALLED FROM ENEMY WHEN IT GETS DESTROYED OR IS TOO FAR..can be called by SceneManager too.
    public void Free_TargetID_Sprite(Transform t)
    {
      
        foreach (GameObject g in activeList)
        {

            targetIDScript = g.GetComponent<ScriptTargetID>();

            if (targetIDScript.targetFocus==t )
            {

                targetIDScript.targetFocus = null;

                targetIDScript.spinTrigger = false;

                g.transform.rotation = Quaternion.identity;

                g.SetActive(false);

                activeList.Remove(g); // Quicker for A_SceneManager Script to call and loop through shorter list if target object goes out of view

                break;
            }

        }

    }


   
    //---------------------
    // CALLED FROM ENEMY WHEN IT COMES CLOSER TO THE PLAYER - EXCHANGES FOR NEW 'NEARLIST' SPRITE
    public void Attach_Near_TargetID_Sprite(Transform t)
    {

        foreach (GameObject g in nearList)
        {
            if (!g.activeSelf)
            {

                g.SetActive(true);

                targetIDScript = g.GetComponent<ScriptTargetID>();

                targetIDScript.targetFocus = t;

                targetIDScript.spinTrigger = true;

                activeList.Add(g); // Quicker for A_SceneManager Script to call and loop through shorter list if target object goes out of view


                break;
            }
        }

    }

    //---------------------
    // CALLED FROM ENEMY WHEN IT COMES WITHIN RANGE
    public void Attach_Far_TargetID_Sprite(Transform t)
    {

        foreach (GameObject g in farList)
        {
            if (!g.activeSelf)
            {

                g.SetActive(true);

                targetIDScript = g.GetComponent<ScriptTargetID>();

                targetIDScript.targetFocus = t;

                targetIDScript.spinTrigger = false;

                activeList.Add(g); // Quicker for A_SceneManager Script to call and loop through shorter list if target object goes out of view

                sfx.PlaySound(4, Vector3.zero); // FAR BLEEP


                break;
            }
        }

    }

        
     
    








}
