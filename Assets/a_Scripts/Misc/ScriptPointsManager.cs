using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScriptPointsManager : MonoBehaviour {
    // HUD Target Identifier Sprites Manager:
    //
    //
    // This Script acts as a 'Mini-Object Pool' .. activating sprites only when needed and adding them to an 'Active List' to update

    private float rotz;

    private List<GameObject> pointsList = new List<GameObject>(); // Far enemies

    //public List<GameObject> activeList = new List<GameObject>(); // so we only loop through a smaller active list of sprites in the main update

   

    private ScriptPoints pointsScript; // access to sprite scripts

    private FXManager sfx; // Script for Sound & Particle fx manager..

    public int points;

    public UILabel scoreText;
    
    private int score;

    private ScriptGameHUD HUD;


    //private bool debug;


    //---------------------
    void Awake()
    {
        GameObject tempObject;

        UILabel pointsLabel = GameObject.Find("Label (points)").GetComponent("UILabel") as UILabel;

        //scoreText = GameObject.Find("Label_Score").GetComponent("UILabel") as UILabel;

        HUD = GameObject.Find("UI Root (2D)").GetComponent("ScriptGameHUD") as ScriptGameHUD;


        for (int i = 0; i < 9; i++)
        {
            //rotating near sprite
            tempObject = Instantiate(pointsLabel.gameObject, pointsLabel.transform.position, pointsLabel.transform.rotation) as GameObject;

            tempObject.transform.parent = transform;

            tempObject.SetActive(false);

            pointsList.Add(tempObject);

        }




        pointsList.Add(pointsLabel.gameObject);
        pointsLabel.gameObject.SetActive(false);



        //tempObject = GameObject.Find("PlayerCamera");
        //sfx = tempObject.GetComponent<FXManager>(); //


    }





    //---------------------
   

    


    int i;
    public void Reset_Points_Label(GameObject l)
    {

        i = 0;
        foreach (GameObject g in pointsList)
        {
            if (g == l)
            {
                pointsList[i].SetActive(false);

                break;
            }

            i++;
        }
    
    }


    
    //---------------------
    // CALLED FROM ENEMY WHEN IT GETS DESTROYED & POINTS ARE AWARDED

    public void Activate_Points_Label(Vector3 pos, int p)
    {

        foreach (GameObject g in pointsList)
        {
            if (!g.activeSelf)
            {

                g.SetActive(true);

                pointsScript = g.GetComponent<ScriptPoints>();

                pos.y = pos.y + 6.0f; // Nudge the Y start position up a little
                               
                pointsScript.tPos = pos;

                pointsScript.reset = true; //Reset some stuff
                              
                pointsScript.GetComponent<UILabel>().text = p.ToString();

                //score = score + p; // UPDATE GAME SCORE

                HUD.Do_Score_Count(p); // UPDATE GAME SCORE

                break;
            }
        }

    }






   

   


           
}

    



















