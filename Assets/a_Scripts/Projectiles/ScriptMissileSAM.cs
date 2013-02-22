using UnityEngine;
using System.Collections;

public class ScriptMissileSAM : MonoBehaviour {


    public float bulletLife; // Set from  firing object
    public float bulletSpeed; // - Set from  firing object
    public float bulletHitValue; // - Set from  firing object
    public GameObject fx; // Set from the firing object
    public FXManager sfx; // Script for particle fx manager..set by turret or gun on creation
    private bool playSound;
    private int soundID; // 'Audio Source id' FOR LOOPING SFX ONLY - Gets returned from play_sound() - can then stop from here if necessary
    private float dist;
    public Transform target; // Object that fired this projectile sets the target






    
    // Use this for initialization
	void Start ()
    {
        playSound = false; // Must Reset as object will be repooled & resused with new missile

	}
	
	// Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * bulletSpeed * Time.deltaTime);
        //if (Time.time > bulletLife) {DestroyObject(gameObject);; Debug.Log(" Bullet Destroyed!");}

        if (Time.time > bulletLife)
        {
            Destroy_Self();
        }
        else
        {


            dist = Vector3.Distance(transform.position, target.transform.position);


            if (dist < 30f)
            {
                Destroy_Self();
            }
            else
            {
                if (dist < 140f)
                {
                    if (!playSound)
                    {
                        sfx.PlaySound(3, transform.position); // MISSILE WHOOSH - get return 'Audio Source id' so we can stop sound if Missile explodes
                        //sfx.PlaySound(3, Vector3.zero); // MISSILE WHOOSH
                        playSound = true;
                    }

                }

            }

        }


    }
      
	


    void Destroy_Self()
    {

        fx.transform.parent = null;
        sfx.Destroy_Particle(fx.particleSystem); // Destroy the thrust plume attached to the back of the missile

        // Make a BOOM!
        sfx.Play_One_Shot_Particle(0, transform.position); // 0 is standard Billboard explosion

        ObjectPoolManager.DestroyPooled(this.gameObject); // this ONLY de-activates the object and places back in the POOL LIBRARY

    }


    /*
     dist = Vector3.Distance(transform.position, target.transform.position);


          if (dist < 30f)
          {
              Destroy_Self();
          }
          else
          {
              if (dist < 150f)
              {
                  if (!playSound)
                  {
                      sfx.PlaySound(3, transform.position); // MISSILE WHOOSH
                      playSound = true;
                  }

              }
          }

*/
}
