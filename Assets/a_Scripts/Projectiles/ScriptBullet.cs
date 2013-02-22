using UnityEngine;
using System.Collections;

public class ScriptBullet : MonoBehaviour
{

    public int bulletID; // Tells us which projectile - Set from Gun script
    //public int activeBulletCount; //How many bullets are flying around still - decrease as bullets are destroyed on impact  - Set from Gun script
    public float bulletLife; // Set from Gun script
    public float bulletSpeed; // - Set from Gun script
    public float bulletHitValue; // - Set from Gun script
    public Transform bulletHitTarget; // - Set from Gun script
    public string bulletHitTargetTag; // - Set from Gun script, but all obtained from inputs script raycast
    private FXManager sfx;
    private ScriptASceneManager SceneManager;
    private int damageRadius; //Varies for missiles & Exp Shells




    //****************
    void Awake()
    {

        GameObject tempObject = GameObject.Find("PlayerCamera");
        Camera playerCamera = tempObject.GetComponent<Camera>(); //
        sfx = tempObject.GetComponent<FXManager>(); //

        tempObject = GameObject.Find("A_Scene_Manager");
        SceneManager = tempObject.GetComponent<ScriptASceneManager>();
       
    }





    //****************
    void Start()
    {

        //bulletHitTarget = null; // I should nullify these Values but something is going wrong;
        //bulletHitTargetTag = "";

    }


    //****************
    // Update is called once per frame

    private Vector3 t;
    void Update()
    {
       
       

        switch (bulletID)
        {


            
            //---------------------
            case 0: // STANDARD
                //---------------------
               

                if (Time.time >= bulletLife)
                {

                    if (bulletHitTarget)
                    {

                        //Debug.Log("HIT!!!" +(bulletHitTarget.name));

                        // SOME GAMEOBJECTS HAVE NO SCRIPT & WILL NOT MAKE THEIR OWN EXPLOSION OR EFFECT ON IMPACT
                        // SO THE BULLET HERE WILL MAKE THE EXPLOSION UPON ITS SCHEDULED IMPACT

                        switch (bulletHitTargetTag)
                        {
                            case "Untagged":

                                t = transform.position;
                                t.y = t.y + 1.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                                sfx.Play_One_Shot_Particle(5, t); // 5 is a small IMPACT


                                break;

                            case "En_Armoured":



                                sfx.Play_One_Shot_Particle(4, transform.position); //45 is a SPARK FX
                              

                                break;

                            case "En_Default":



                                sfx.Play_One_Shot_Particle(4, transform.position); //45 is a SPARK FX


                                break;

                            case "En_Soldier":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(6, transform.position); // 6 is a Blood Splat

                                break;

                            default:
                                // Do something defaulty

                                break;


                        }

                        bulletHitTarget = null; // nullify this Value HERE ONLY!!

                    }

                    ObjectPoolManager.DestroyPooled(this.gameObject);

                    bulletHitTarget = null;

                }

                else
                {

                    //transform.collider.rigidbody.isKinematic = false;
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }







                break;




           //---------------------
            case 1: //Machine Gun
           //---------------------
                if (Time.time >= bulletLife)
                {

                    if (bulletHitTarget)
                    {
                        


                        // SOME GAMEOBJECTS HAVE NO SCRIPT & WILL NOT MAKE THEIR OWN EXPLOSION OF EFFECT ON IMPACT
                        // SO THE BULLET HERE WILL MAKE THE EXPLOSION ON ITS SCHEDULED IMPACT
                        switch (bulletHitTargetTag)
                        {
                            case "Untagged":

                                t = transform.position;
                                t.y = t.y + 1.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                                sfx.Play_One_Shot_Particle(5, t); // 5 is a small IMPACT


                                break;

                            case "En_Armoured":

                                
                              
                                sfx.Play_One_Shot_Particle(4, transform.position); //45 is a SPARK FX


                                break;

                            case "En_Default":



                                sfx.Play_One_Shot_Particle(4, transform.position); //45 is a SPARK FX


                                break;

                            case "En_Soldier":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(6, transform.position); // 6 is a Blood Splat

                                break;

                            default:
                                // Do something defaulty

                                break;


                        }

                     
                    }

                    bulletHitTarget = null;

                    ObjectPoolManager.DestroyPooled(this.gameObject);

                }

                else
                {

                    //transform.collider.rigidbody.isKinematic = false;
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }



                break;


            //---------------------
            case 2: //HEAVY Machine Gun
                //---------------------


                if (Time.time >= bulletLife)
                {

                    if (bulletHitTarget)
                    {



                        // SOME GAMEOBJECTS HAVE NO SCRIPT & WILL NOT MAKE THEIR OWN EXPLOSION OF EFFECT ON IMPACT
                        // SO THE BULLET HERE WILL MAKE THE EXPLOSION ON ITS SCHEDULED IMPACT
                        switch (bulletHitTargetTag)
                        {
                            case "Untagged":

                                t = transform.position;
                                t.y = t.y + 1.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                                sfx.Play_One_Shot_Particle(5, t); // 5 is a small IMPACT


                                break;

                            case "En_Armoured":



                                sfx.Play_One_Shot_Particle(4, transform.position); //45 is a SPARK FX


                                break;

                            case "En_Default":



                                sfx.Play_One_Shot_Particle(4, transform.position); //45 is a SPARK FX


                                break;

                            case "En_Soldier":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(6, transform.position); // 6 is a Blood Splat

                                break;

                            default:
                                // Do something defaulty

                                break;


                        }



                    }

                    bulletHitTarget = null;

                    ObjectPoolManager.DestroyPooled(this.gameObject);

                }

                else
                {

                    //transform.collider.rigidbody.isKinematic = false;
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }






                break;
            //---------------------
            case 3 : //LOW EXPLOSIVE SHELL
            //---------------------

                if (Time.time > bulletLife)
                {
                   
                    if (bulletHitTarget)
                    {

                        damageRadius = 13; // 17 metre damage test

                        Enemy_Proximity_Distance_check();

                        // SOME GAMEOBJECTS HAVE NO SCRIPT & WILL NOT MAKE THEIR OWN EXPLOSION OF EFFECT ON IMPACT
                        // SO THE BULLET HERE WILL MAKE THE EXPLOSION ON ITS SCHEDULED IMPACT
                        switch (bulletHitTargetTag)
                        {
                            case "Untagged":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a Whatever!
                                t = transform.position;
                                t.y = t.y + 1.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                                sfx.Play_One_Shot_Particle(3, t); // 3 is a small explosion and cloud
                             

                                break;

                            case "En_Armoured":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(8, transform.position); // 3 is a small explosion and cloud
                              
                                break;


                            case "En_Default":



                                sfx.Play_One_Shot_Particle(8, transform.position); //45 is a SPARK FX
                              

                                break;

                            case "En_Soldier":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(8, transform.position); // 6 is a Blood Splat
                               
                                break;

                            default:

                            // Do something defaulty

                           
                                break;


                        }
                    }

                    bulletHitTarget = null;

                    ObjectPoolManager.DestroyPooled(this.gameObject);

                } // this ONLY de-activates the object and places back in the POOL LIBRARY

                else
                {

                    //transform.collider.rigidbody.isKinematic = false;
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }

                break;




            //---------------------
            case 4: //HIGH EXPLOSIVE SHELL
                //---------------------

                if (Time.time > bulletLife)
                {
                   
                    if (bulletHitTarget)
                    {

                        

                        damageRadius = 20; // 20 metre damage test
                        Enemy_Proximity_Distance_check();

                        t = transform.position;
                        //t.y = t.y + 1.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                        t.x = t.x + Random.Range(-4f, 4f);
                        t.z = t.z + Random.Range(-4f, 4f);


                        // SOME GAMEOBJECTS HAVE NO SCRIPT & WILL NOT MAKE THEIR OWN EXPLOSION OF EFFECT ON IMPACT
                        // SO THE BULLET HERE WILL MAKE THE EXPLOSION ON ITS SCHEDULED IMPACT
                        switch (bulletHitTargetTag)
                        {
                            case "Untagged":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a Whatever!
                                t.y = t.y + 1.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                                sfx.Play_One_Shot_Particle(3, t); // 3 is a small explosion and cloud


                                break;

                            case "En_Armoured":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(8, t); // 3 is a small explosion and cloud

                                break;


                            case "En_Default":



                                sfx.Play_One_Shot_Particle(8, t); //45 is a SPARK FX


                                break;

                            case "En_Soldier":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(8, t); // 6 is a Blood Splat

                                break;

                            default:

                                // Do something defaulty


                                break;


                        }
                    }

                    bulletHitTarget = null;

                    ObjectPoolManager.DestroyPooled(this.gameObject);

                } // this ONLY de-activates the object and places back in the POOL LIBRARY

                else
                {

                    //transform.collider.rigidbody.isKinematic = false;
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }

                break;




            //---------------------
            default:
            //---------------------

                if (Time.time > bulletLife)
                {

                    if (bulletHitTarget)
                    {

                        // SOME GAMEOBJECTS HAVE NO SCRIPT & WILL NOT MAKE THEIR OWN EXPLOSION OF EFFECT ON IMPACT
                        // SO THE BULLET HERE WILL MAKE THE EXPLOSION ON ITS SCHEDULED IMPACT
                        switch (bulletHitTargetTag)
                        {
                            case "Untagged":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a Whatever!
                                t = transform.position;
                                t.y = t.y + 4.0f; // OFFSET TO REDUCE CLIPPING PROBLEMS
                                sfx.Play_One_Shot_Particle(3, t); // 3 is a small explosion and cloud

                                break;

                            case "En_Armoured":

                                // TEMPORARY - NEED OTHER SIMPLER EFFECTS HERE
                                //Make a BOOM!
                                sfx.Play_One_Shot_Particle(3, transform.position); // 3 is a small explosion and cloud

                                break;

                            default:
                                // Do something defaulty

                                break;


                        }
                    }

                    bulletHitTarget = null;

                    ObjectPoolManager.DestroyPooled(this.gameObject);

                } // this ONLY de-activates the object and places back in the POOL LIBRARY

                else
                {

                    //transform.collider.rigidbody.isKinematic = false;
                    transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
                }

                break;

        }


    }




    //---------------------------------
    // PROXIMITY DISTANCE.. CHECKS ACTIVE LIST IN SCENE MANAGER
    //---------------------------------

    private float dist;
    private int nearMissDamage;

    void Enemy_Proximity_Distance_check()
    {

        foreach (GameObject g in SceneManager.activeList)
        {

            //if (g.layer == 12) // Enemies Layer - set in editor along with Tags
            //{
            if (g.transform != bulletHitTarget)
            {
                dist = Vector3.Distance(transform.position, g.transform.position);

                if (dist < damageRadius)
                {
                    nearMissDamage = (int)((bulletHitValue / dist) * 1.5f);

                    Do_Proximity_Damage(g);
                }
            }
            // }


        }
        
    }






    //---------------------------------
    // PROXIMITY DAMAGE FROM EXPLOSIVES
    //---------------------------------
    
    private string st;
    private ScriptEmptyAI SAI; // Used multiple times for Generic enemies - may change at later date

    void Do_Proximity_Damage(GameObject nearMiss)
    {
                
        st = nearMiss.name.Remove(0, 6);
        st = st.Remove(st.Length-7, 7);
        

        switch (st)
        {

            case "AAMobile":
                ScriptAAMobile sc1 = nearMiss.GetComponent<ScriptAAMobile>();
                sc1.Calculate_Damage(nearMissDamage);
                break;
            case "AASAM":
                ScriptAASAM sc2 = nearMiss.GetComponent<ScriptAASAM>();
                sc2.Calculate_Damage(nearMissDamage);
                break;
            case "AARapier":
                ScriptAARapier sc3 = nearMiss.GetComponent<ScriptAARapier>();
                sc3.Calculate_Damage(nearMissDamage);
                break;
            
            case  "tank_big" :
                SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);

                break;
            case  "tank_small" :
                SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);
                break;
            case "truck_brown" :
               SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);
                break;
            case  "truck_medic" :
               SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);
                break;
            case  "scout" :
                SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);
                break;
            case  "mobile_radar" :
                ScriptMobileRadar scMB = nearMiss.GetComponent<ScriptMobileRadar>();
                scMB.Calculate_Damage(nearMissDamage);
                break;
            case  "mobile_SAM" :
                ScriptMobileSAM scMBS = nearMiss.GetComponent<ScriptMobileSAM>();
                scMBS.Calculate_Damage(nearMissDamage);
                break;
            //case  "comm_dish" :
               // ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
               // sc.Calculate_Damage(nearMissDamage);
               // break;
           // case  "comm_building" :
               // ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
               // sc.Calculate_Damage(nearMissDamage);
                //break;
            //case  "comm_mast" :
                //ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
                //sc.Calculate_Damage(nearMissDamage);
                //break;
            case  "jeep" :
                SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);
                break;
            case  "scud" :
                SAI = nearMiss.GetComponent<ScriptEmptyAI>();
                SAI.Calculate_Damage(nearMissDamage);
                break;
            case  "soldier" :
                ScriptSoldier sol = nearMiss.GetComponent<ScriptSoldier>();
                sol.Calculate_Damage(nearMissDamage);
               // Debug.Log("nearMissDamage: " + nearMissDamage);
                break;
            case  "jet" :
                //ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
                //sc.Calculate_Damage(nearMissDamage);
                break;
            case  "jet_land" :
                //ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
                //sc.Calculate_Damage(nearMissDamage);
                break;
            case  "helicopter" :
                //ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
                //sc.Calculate_Damage(nearMissDamage);
                break;
            case "helicopter_land":
               // ScriptAAMobile sc = nearMiss.GetComponent<ScriptAAMobile>();
               // sc.Calculate_Damage(nearMissDamage);
                break;
       
        }






    }












}
