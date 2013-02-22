using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FXManager : MonoBehaviour
{

	public bool debug = false; // SET TO OFF IN EDITOR FOR FINAL BUILD !!!!!!

	//ARRAY & LOOP STUFF
	private int i;
	private int CurrentFXCount;
	
	private Material[] MaterialList; // for applying materials to Generic Flat Quads

	private int fxID;
	public float fxCount; // How may clones required  - Can be set by calling script

	


	private Transform camera;
	private GameObject tempObject;
    private GameObject NullPool;
	private ScriptAInputManager inp;

	public AudioClip[] SFX_ARRAY; // SET SIZE OF SFX ARRAY IN EDITOR - DRAG AUDIO CLIPS ONTO THE ARRAY FROM THERE
    private AudioSource[] SFX_SOURCE; // For multiple sounds to be played at once

    private float maxSoundDistance; // For sounds volume calculation

    public List<ParticleSystem> particleList = new List<ParticleSystem>(); //LIBRARY of particle set in Editor
    private List<ParticleSystem> activeParticleList = new List<ParticleSystem>(); // List of all newly created Particles - Auto Destroy function in Update
    private List<float> ParticleLifeList = new List<float>(); // BODGE!!! Fixes off screen lingering particles that should have expired - Unity is sloppy!!
    private int particleCount; //keep track of active particle systems amount
    private int maxAudioSources;
 

	//--------------
	// AWAKE 
	//--------------
	void Awake()
	{
        maxSoundDistance = 300f;// For sounds volume calculation in Play functions

		int i;

		// SET UP EXTRA AUDIO SOURCES FOR THE SOUND MANAGER
        maxAudioSources = 10;

        SFX_SOURCE = new AudioSource[maxAudioSources]; // LETS SET 10 MAXIMUM SOUNDS PLAYING AT ONCE

        for (i = 0; i < (maxAudioSources); i++)
		{

			SFX_SOURCE[i] = gameObject.AddComponent<AudioSource> ();
			SFX_SOURCE[i].volume = 1.0f;
			SFX_SOURCE[i].priority = 128;
			SFX_SOURCE[i].minDistance = 200f;
			SFX_SOURCE[i].maxDistance = 205f;
			SFX_SOURCE[i].playOnAwake = false;

           
		}
       

		//Find and get INPUT MANAGER SCRIPT - Totally necessary :)
		tempObject = GameObject.Find ("A_GameWrapper");
		inp = tempObject.GetComponent<ScriptAInputManager> (); //

		//Find and get Camera
		tempObject = GameObject.Find ("PlayerCamera");
		camera = tempObject.transform;
		transform.parent = camera;

        // Parent to all pool objects - need to re-parent just in case when destroyed
        NullPool = GameObject.Find("A_Null_Pool_Library");
        

	}




    //************************
    // PREFAB POINTERS
    GameObject expDefault; // Default Explosion
    GameObject expFiery; // Oil barrels etc...
    GameObject expDebris;// Explosion Debris
    void Get_Prefab_Pointers()
    {
        //GameObject tempObject;
        tempObject = GameObject.Find("A_Scene_Manager");
        ScriptASceneManager pre;

        pre = tempObject.GetComponent<ScriptASceneManager>();

        for (int i = 0; i < pre.objectPoolList.Length; i++)
        {
            //Default Bullet
            if (pre.objectPoolList[i].name == "P_exp01") { expDefault = pre.objectPoolList[i]; }
            if (pre.objectPoolList[i].name == "P_exp02") { expFiery = pre.objectPoolList[i]; } // not made yet
            if (pre.objectPoolList[i].name == "exp_debris") { expDebris = pre.objectPoolList[i]; } // Chunks of stuff for big explosions
        }
    }

    //************************
  
 



	//--------------
	// START 
	//--------------
	void Start()
	{
        Get_Prefab_Pointers();
	}

	//--------------
	// UPDATE 
	//--------------
	void Update()
	{
        //Debug.Log("PCount: " + particleCount);
        if (particleCount > 0) { Check_For_Expired_Particles(); }
	}
	


	//***********************		
	// PLAY ONE SHOT FX... Called from anywhere to play a sound from FX manager sound bank
    public void PlaySound(int index, Vector3 dist)
	{
        int i = 0;

		for(i = 0; i < (SFX_SOURCE.Length); i++) 
		{
			if(!SFX_SOURCE[i].isPlaying) // FIND A FREE AUDIO SOURCE
			{
				SFX_SOURCE[i].clip = SFX_ARRAY[index]; 
				SFX_SOURCE[i].loop = false;
				SFX_SOURCE[i].Play ();

                // CHECK DISTANCE
                if (dist.x > 0 | dist.x < 0)
                {
                    float d = Vector3.Distance(dist, transform.position);

                    SFX_SOURCE[i].volume = ((1.0f - (1.0f / maxSoundDistance) * d));

                    if (SFX_SOURCE[i].volume < 0.1f) { SFX_SOURCE[i].Stop(); } // Don't play if volume is too low
                }
                else
                {
                    SFX_SOURCE[i].volume = 1.0f ;
                }

                break;
			}
		}

       
		
	}



    //***********************		
    // PLAY ONE SHOT FX... Fakes the distance to give larger noise
    public void PlaySoundWithBoost(int index, Vector3 dist, float m) // M is sound multiplier
    {
       
        int i = 0;

        for (i = 0; i < (maxAudioSources); i++)
        {
            if (!SFX_SOURCE[i].isPlaying) // FIND A FREE AUDIO SOURCE
            {
                SFX_SOURCE[i].clip = SFX_ARRAY[index];
                SFX_SOURCE[i].loop = false;
                SFX_SOURCE[i].Play();

                // CHECK DISTANCE
                //if (dist.x > 0 | dist.x < 0)
                if (dist != transform.position)
                {
                    float d = Vector3.Distance(dist, transform.position);

                    SFX_SOURCE[i].volume = ((1.0f - (1.0f / maxSoundDistance) * d))*m;

                    if (SFX_SOURCE[i].volume < 0.1f) { SFX_SOURCE[i].Stop(); } // Don't play if volume is too low
                }
                else
                {
                    SFX_SOURCE[i].volume = 1.0f;
                }

                break;
            }
        }



    }

















	//***********************		
	// PLAY LOOPING SOUND - Looping sounds must return an Audio Source Array position so they can be accessed later and stopped
	public int Play_Looping_Sound(int index ) // Index refers to SFX Array number in SFX list Script attached to Player Camera
	{
        int i = 0;

        for (i = 0; i < (maxAudioSources); i++) 
		{
			if(!SFX_SOURCE[i].isPlaying)
			{
				SFX_SOURCE[i].clip = SFX_ARRAY[index];
				SFX_SOURCE[i].loop = true;
				SFX_SOURCE[i].Play ();

                break;
			}
		}

        // Return null value if all audio sources busy
        if (i < maxAudioSources) { } else { i = -1; }

        return (i); // Return id number so THAT SOUND CAN BE STOPPED IF NECESSARY BY OBJECT THAT CALLED THE SOUND
	}




    //***********************		
    // PLAY LOOPING SFX... Fakes the distance to give louder noise
    // Looping sounds must return an Audio source Index so they can be accessed later and stopped

    public int PlayLoopingSoundWithDistance(int index, Vector3 dist, float m) // M is sound multiplier -Index refers to SFX Array number in SFX list Script attached to Player Camera
    {

        int i = 0;

        for (i = 0; i < (maxAudioSources); i++)
        {
            if (!SFX_SOURCE[i].isPlaying) // FIND A FREE AUDIO SOURCE
            {
                SFX_SOURCE[i].clip = SFX_ARRAY[index];
                SFX_SOURCE[i].loop = true;
                SFX_SOURCE[i].Play();

                // CHECK DISTANCE
                //if (dist.x > 0 | dist.x < 0)
                if (dist !=transform.position)
                {

                    float d = Vector3.Distance(dist, transform.position);

                    SFX_SOURCE[i].volume = ((1.0f - (1.0f / maxSoundDistance) * d)) * m;

                    if (SFX_SOURCE[i].volume < 0.1f) { SFX_SOURCE[i].Stop(); } // Don't play if volume is too low
                }
                else
                {
                    SFX_SOURCE[i].volume = 1.0f;
                }

                break;
            }
        }

        // Return null value if all audio sources busy
        if (i < maxAudioSources) { } else { i = -1; }

        return (i); // Return id number so THAT SOUND CAN BE STOPPED IF NECESSARY BY oBJECT THAT CALLED THE SOUND

    }



	//***********************		
	// STOP LOOPING SOUND
    public void Stop_Looping_Sound(int index)// Index refers to SFX Array number in SFX list Script attached to Player Camera
    {
        
        if (index > 0) // index can be set to -1 when sound play function is called & there are no available sound sources
        {

            if (SFX_SOURCE[index].isPlaying)
            {

                SFX_SOURCE[index].loop = false;
                SFX_SOURCE[index].Stop();
                return;

            }
        }
       
    }
	


	//***********************		
	// STOP ALL SOUND.. This is usually called from the Camera Script when a Screen fades or Screen Doors close
    public void Stop_All_Sound()
    {
        for (int i = 0; i < (maxAudioSources); i++)
        {
            if (SFX_SOURCE[i].isPlaying)
            {
                SFX_SOURCE[i].loop = false;
                SFX_SOURCE[i].Stop();
            }
        }
    }



    //***********************		
    // SET VOLUME This is called from Play sound functions

    void Calculate_SFX_Volume()
    {


    }

    //***********************		
    // FOR LOOPING ENVIRONMENT SFX

    void Modulate_Sound_Volume()
    {


    }






    //***********************		
    // PARTICLE STUFF
    //***********************


    public void Play_One_Shot_Particle(int pID,Vector3 pos)
    {
      
    
        //  Need to cast 'ParticleSystem' to 'GameObject' for Pool to work - it deals with GameObjects

        // ParticleSystem exp = Instantiate(activeParticleList[pID], pos, Quaternion.identity) as ParticleSystem; // instantiate random ball
        GameObject expObject = ObjectPoolManager.CreatePooled(particleList[pID].gameObject, pos, Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[
            
        ParticleSystem exp = expObject.particleSystem;

        exp.gameObject.SetActive(true);

        activeParticleList.Add(exp); // in main update - loops and checks for particle that have stopped animating

        ParticleLifeList.Add(Time.time + exp.particleSystem.duration); // for life time  outs check

        Play_Particle_Sound(pID, pos); // Decide distance and then play sound
        
        particleCount++;


    }

    //----------------------
    // Really this is for missiles and other fast moving objects that would give a 'moving' explosion effect

    public void Play_One_Shot_Parented_Particle(int pID, Transform t,Vector3 offset)
    {


        //  Need to cast 'ParticleSystem' to 'GameObject' for Pool to work - it deals with GameObjects

        // ParticleSystem exp = Instantiate(activeParticleList[pID], pos, Quaternion.identity) as ParticleSystem; // instantiate random ball
        GameObject expObject = ObjectPoolManager.CreatePooled(particleList[pID].gameObject, t.transform.position , Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[

        expObject.transform.parent = t; // Attach it to parent - probably a now invisible but fast moving missile, Pre-Deletion

        if (offset != Vector3.zero) // Avoid Quaternion errors to be safe
        {
            //Debug.Log(" Not Zero!");
            expObject.transform.localPosition = offset;
        }

        ParticleSystem exp = expObject.particleSystem;



        exp.gameObject.SetActive(true);

        activeParticleList.Add(exp); // in main update - loops and checks for particle that have stopped animating

        ParticleLifeList.Add(Time.time + exp.particleSystem.duration); // for life time  outs check

        Play_Particle_Sound(pID, t.transform.position); // Decide distance and then play sound

        particleCount++;


    }


    //***********************		
    public GameObject Play_Particle(int pID, Vector3 pos) // Normal looping particle system - We probably don't ever need any
    {
        
        GameObject p = ObjectPoolManager.CreatePooled(particleList[pID].gameObject, pos, Quaternion.identity, 0);// Added a tagID to pass here - probably never going to be needed :[

        activeParticleList.Add(p.particleSystem);

        ParticleLifeList.Add(Time.time + p.particleSystem.duration); // for life time  outs check

        particleCount++;

        return p;

    }


    //***********************		
    void Check_For_Expired_Particles() // Specifically for one shot animations such as explosions , impacts etc..
    {
        for (int i = activeParticleList.Count - 1; i >= 0; i--)
        {

            if (!activeParticleList[i].particleSystem.loop)// if one shot fx and off camera then de-pool regardless
            {


                // WHY DOES THIS NOT WORK?.. Can't check if particle is visible to camera..where is the bastard renderer?
                //if (!activeParticleList[i].renderer.isVisible || !activeParticleList[i].particleSystem.IsAlive()) // Can the camera see this particle renderer ?
                if (Time.time > ParticleLifeList[i] || !activeParticleList[i].particleSystem.IsAlive()) // Can the camera see this particle renderer ?
                {
                    activeParticleList[i].transform.parent = NullPool.transform;

                    activeParticleList[i].Stop();
                    activeParticleList[i].Clear() ;

                    ObjectPoolManager.DestroyPooled(activeParticleList[i].gameObject);

                    activeParticleList.Remove(activeParticleList[i]);

                    ParticleLifeList.Remove(ParticleLifeList[i]); // for life time  outs check

                    particleCount--;
                }
                    
              

            }
            else
            {
                if (!activeParticleList[i].particleSystem.IsAlive())
                {
                    activeParticleList[i].transform.parent = NullPool.transform;

                    activeParticleList[i].Stop();
                    activeParticleList[i].Clear();

                    //Object.Destroy(activeParticleList[i].gameObject);
                    ObjectPoolManager.DestroyPooled(activeParticleList[i].gameObject);

                    activeParticleList.Remove(activeParticleList[i]); // Safe to do - looped backwards through list

                    ParticleLifeList.Remove(ParticleLifeList[i]); // for life time  outs check

                    particleCount--;
                }
            }
        }
    }



    //***********************		
    public void Find_Particle(ParticleSystem p)
    {


    }

   // Public Can be called from anywhere
    //***********************		
    public void Destroy_Particle(ParticleSystem p)
    {
        p.transform.parent = NullPool.transform;


        for (int i = activeParticleList.Count - 1; i >= 0; i--)
        {
            if (p == activeParticleList[i])
            {
                ParticleLifeList.Remove(ParticleLifeList[i]); // for life time outs check

                activeParticleList[i].Stop();
                activeParticleList[i].Clear();

                ObjectPoolManager.DestroyPooled(p.gameObject);

                activeParticleList.Remove(p);

                particleCount--;

                break;
            }
        }

     

    }



    //***********************		
    // PARTICLE SFX

    void Play_Particle_Sound(int pID, Vector3 dist)
    {


        switch (pID)
        {
            case 0: // STANDARD AIR EXPLOSION SFX

                PlaySound(2, dist); // '2' is standard explosion SFX..look in inspector at this script: SFX_ARRAY[]
                break;


            case 2: //  LARGE EXPLOSION  - Final Death of armoured enemy
               
                PlaySoundWithBoost(7, dist,1.7f); // '2' is standard explosion SFX..look in inspector at this script: SFX_ARRAY[]
                break;

            case 3: // SMALL NON SERIOUS CLOUD PUFF EXPLOSION
               
                PlaySound(8, dist); // '2' is standard explosion SFX..look in inspector at this script: SFX_ARRAY[]
                break;

            case 8: // STANDARD GROUND EXPLOSION SFX

                PlaySound(2, dist); // '2' is standard explosion SFX..look in inspector at this script: SFX_ARRAY[]
                break;

            default:
                break;
        }


    }


    //***********************		
    //METAL DEBRIS
   public  void Make_Metal_Debris(Vector3 pos)
    {
        pos.y = pos.y + 3.0f;// Raise debris a little

        GameObject p = ObjectPoolManager.CreatePooled(expDebris, pos, Quaternion.identity, 0);

    }


} // Class end
