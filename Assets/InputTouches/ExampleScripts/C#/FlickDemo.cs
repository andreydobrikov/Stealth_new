using UnityEngine;
using System.Collections;

public class FlickDemo : MonoBehaviour {

	public Transform source;
	public GameObject shootObject;
	public float forceModifier=1;
	
	
	void OnEnable(){
		Gesture.onSwipeStartE += OnSwipeStart;
		Gesture.onSwipeEndE += OnSwipeEnd;
		
		Gesture.onSwipeE += OnSwipe;
	}
	
	void OnDisable(){
		Gesture.onSwipeStartE += OnSwipeStart;
		Gesture.onSwipeEndE += OnSwipeEnd;
		
		Gesture.onSwipeE += OnSwipe;
	}
	
	//when a swipe has started, valid or not
	//if the starting position is on the source(turret), scale it up to give some visual feedback
	void OnSwipeStart(SwipeInfo sw){
		Ray ray = Camera.main.ScreenPointToRay(sw.startPoint);
		RaycastHit hit;
		if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
			if(hit.transform==source){
				source.localScale=new Vector3(2.2f, 2.2f, 2.2f);
			}
		}
	}
	
	//when a swipe has end, valid or not
	void OnSwipeEnd(SwipeInfo sw){
		//make sure we adjust the source(turret) scale back to original
		source.localScale=new Vector3(1.8f, 1.8f, 1.8f);
	}
	
	//when a valide swipe is confirm
	void OnSwipe(SwipeInfo sw){
		Ray ray = Camera.main.ScreenPointToRay(sw.startPoint);
		RaycastHit hit;
		//use raycast at the cursor position to detect the object
		if(Physics.Raycast(ray, out hit, Mathf.Infinity)){
			//only if the swipe is started from the source(turret) location 
			if(hit.transform==source){
				//instatiate a new shootObject
				GameObject shootObjInstance=(GameObject)Instantiate(shootObject, source.TransformPoint(new Vector3(0, 0.5f, 0.5f)), Quaternion.identity);
				
				//if using siwpe magnitude as force determining factor
				if(forceFactor==0){
					//calculate the force, clamp the value between 50 and 2200 so that it's not too slow or too fast that the shootObject is not visible
					float force=Mathf.Clamp(sw.direction.magnitude*forceModifier*1.5f, 50, 2200);
					//normalize the direction
					sw.direction.Normalize();
					//apply the force according to the direction
					shootObjInstance.rigidbody.AddForce(new Vector3(sw.direction.x, sw.direction.y, 0)*force);
				}
				//if using siwpe speed as force determining factor
				else if(forceFactor==1){
					//calculate the force, clamp the value between 50 and 2200 so that it's not too slow or too fast that the shootObject is not visible
					float force=Mathf.Clamp(sw.speed*forceModifier*1f, 50, 2200);
					//normalize the direction
					sw.direction.Normalize();
					//apply the force according to the direction
					shootObjInstance.rigidbody.AddForce(new Vector3(sw.direction.x, sw.direction.y, 0)*force);
				}
				
				//make sure the shootObject is destroy after 3 second
				Destroy(shootObjInstance, 3);
			}
		}
	}
	
	
	private bool instruction=false;
	void OnGUI(){
		string title="Shoot the target!!";
		GUI.Label(new Rect(150, 15, 500, 40), title);
		
		if(!instruction){
			if(GUI.Button(new Rect(10, 55, 130, 35), "Instruction On")){
				instruction=true;
			}
		}
		else{
			if(GUI.Button(new Rect(10, 55, 130, 35), "Instruction Off")){
				instruction=false;
			}
			
			GUI.Box(new Rect(10, 100, 330, 65), "");
			
			GUI.Label(new Rect(15, 105, 320, 65), "- press on the fire button and flick it towards a target\n- the flick direction and speed will determine the firing angle and force");
		}
		
		/*
		GUI.Label(new Rect(Screen.width-230, Screen.height-70, 100, 35), "force factor:");
		if(forceFactor==0){
			if(GUI.Button(new Rect(Screen.width-150, Screen.height-75, 130, 35), "swipe magnitude")){
				forceFactor=1;
			}
		}
		if(forceFactor==1){
			if(GUI.Button(new Rect(Screen.width-150, Screen.height-75, 130, 35), "swipe speed")){
				forceFactor=0;
			}
		}
		
		GUI.Label(new Rect(Screen.width-230, Screen.height-120, 100, 35), "force modifier: "+forceModifier.ToString("f1"));
		if(GUI.Button(new Rect(Screen.width-130, Screen.height-120, 35, 35), "-")){
			forceModifier-=0.2f;
			forceModifier=Mathf.Max(0.2f, forceModifier);
		}
		if(GUI.Button(new Rect(Screen.width-90, Screen.height-120, 35, 35), "+")){
			forceModifier+=0.2f;
			forceModifier=Mathf.Min(2.0f, forceModifier);
		}
		*/
		
	}
	
	private int forceFactor=1;
	
}
