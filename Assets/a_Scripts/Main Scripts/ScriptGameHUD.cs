using UnityEngine;
using System.Collections;

public class ScriptGameHUD : MonoBehaviour {

    bool debugTest = true;

    public UILabel scoreText;
    public UIPanel healthPanel;
    public UIPanel ammoPanel;
   

    private int score;


    //***********************		
    // START
    //***********************		
	void Start ()
    {

       

        scoreText = GameObject.Find("Label_Score").GetComponent("UILabel") as UILabel;
        healthPanel = GameObject.Find("Panel_Health").GetComponent("UIPanel") as UIPanel;
        ammoPanel = GameObject.Find("Panel_Ammo").GetComponent("UIPanel") as UIPanel;
       

        //scoreText.text = "The String You Want To Assign";
        score = 0;
        Do_Score_Count(0);
      
	}



    //***********************		
    // UPDATE
    //***********************		
	//void Update ()
    //{

        
	//}



    //***********************		
    // HEALTH
    //***********************		
    public void Do_Health_Bar(float value)
    {

        Vector4 v = healthPanel.clipRange;

        float x = (241f / 200f) * value; // 200 is maximum health bar value: 100 health + 100 armour: Game starts at 75%: 150 - 241 IS X SIZE OF SPRITE IN NGUI

        healthPanel.clipping = UIDrawCall.Clipping.HardClip;

        healthPanel.clipRange = new Vector4((-(241f-x)/2f), v.y, x, v.w);


    }




    //***********************		
    // AMMO
    //***********************		
    public void Do_Ammo_Bar(float value,float valueMax) // * Different max bullets for different types of ammo 
    {

        Vector4 v = ammoPanel.clipRange;

        float x = (220f / valueMax) * value; // 220F IS X SIZE OF SPRITE IN NGUI

        ammoPanel.clipping = UIDrawCall.Clipping.HardClip;

        ammoPanel.clipRange = new Vector4((-(220f - x) / 2f), v.y, x, v.w);

    }




    //***********************	
	// SCORE
    //***********************		
    public void Do_Score_Count(int value)
    {
        score = score + value;


        string tempString = score.ToString();

        string t;

        if (score < 10) { t = "00000000"; tempString = t + tempString; }
        else
        {
            if (score > 9 && score < 100) { t = "0000000"; tempString = t + tempString; }
            else
            {
                if (score > 99 && score < 1000) { t = "000000"; tempString = t + tempString; }
                else
                {
                    if (score > 999 && score < 10000) { t = "00000"; tempString = t + tempString; }
                    else
                    {
                        if (score > 9999 && score < 100000) { t = "0000"; tempString = t + tempString; }
                        else
                        {
                            if (score > 99999 && score < 1000000) { t = "000"; tempString = t + tempString; }
                            else
                            {
                                if (score > 999999 && score < 10000000) { t = "00"; tempString = t + tempString; }
                                else
                                {
                                    if (score > 9999999 && score < 100000000) { t = "0"; tempString = t + tempString; }
                                }
                            }
                        }
                    }
                }
            }
        }


        scoreText.text = tempString;
    }

                






   














}
