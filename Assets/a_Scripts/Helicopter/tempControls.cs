using UnityEngine;
using System.Collections;

public class tempControls : MonoBehaviour {
/// This is a modified javascript conversion of the Standard Assets MouseLook script.
/// Also added is functionallity of using a key to look up, down, left and right in addition to the Mouse.
/// Everything is on by default. You will want to turn off/on stuff depending on what you're doing.
 
/// You can also modify the script to use the KeyLook functions to control an object's rotation.
/// Try using MouseXandY on an object. Actually it works as is but you'll want to clean it up ;)
 
/// As of this version the key and mouse fight if used at the same time (ie up key and down mouse jitters).
 
/// Minimum and Maximum values can be used to constrain the possible rotation
 
/// To make an FPS style character:
/// - Create a capsule.
/// - Add a rigid body to the capsule
/// - Add the MouseLookPlus script to the capsule.
///   -> Set the script's Axis to MouseX in the inspector. (You want to only turn character but not tilt it)
/// - Add FPSWalker script to the capsule
 
/// - Create a camera. Make the camera a child of the capsule. Reset it's transform.
/// - Add the MouseLookPlus script to the camera.
///   -> Set the script's Axis to MouseY in the inspector. (You want the camera to tilt up and down like a head. The character already turns.)
 
enum Axes {MouseXandY, MouseX, MouseY}
Axes Axis = Axes.MouseXandY;
 
float sensitivityX = 1.5f;
float sensitivityY = 1.5f;
float minimumX = -42.0f;
float maximumX = 42.0f;
float minimumY = -60.0f;
float maximumY = 60.0f;
 
float rotationX = 0.0f;
float rotationY = 0.0f;
float lookSpeed = 2.0f;


float axisX;
float axisY;

public    int recoilTrigger; // Different interger values give different recoils depending on bullet  - set from GunView script
private float recoilTime;

 
public Transform cameraReference;
 
void Update (){

    axisX =Input.GetAxis("Horizontal");
    axisY= Input.GetAxis("Vertical" );
  

   
    if ( axisY < 0 |  axisY > 0)
    {
       float translationZ= Input.GetAxis("Vertical")*50*Time.deltaTime;
        transform.localPosition += cameraReference.transform.localRotation *new Vector3(0,0,translationZ);
    }
 
    // Strafe the camera
    if (axisX < 0 | axisX > 0)
    {
       float translationX= Input.GetAxis("Horizontal")*50*Time.deltaTime;
        transform.localPosition += cameraReference.transform.localRotation * new Vector3(translationX,0,0);
    }       
 
    if (Axis == Axes.MouseXandY)
    {
        // Read the mouse input axis
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
 
        // Call our Adjust to 360 degrees and clamp function
        Adjust360andClamp();
 
        // Most likely you wouldn't do this here unless you're controlling an object's rotation.
        // Call our look left and right function.
        KeyLookAround();
 
        // Call our look up and down function.
        KeyLookUp();
    }
    else if (Axis == Axes.MouseX)
    {
        // Read the mouse input axis
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;
 
        // Call our Adjust to 360 degrees and clamp function
        Adjust360andClamp();
 
        // if you're doing a standard X on object Y on camera control, you'll probably want to 
        // delete the key control in MouseX. Also, take the transform out of the if statement.
        // Call our look left and right function.
        //KeyLookAround();
 
        // Call our look up and down function.
        //KeyLookUp(); 
    }
    else
    {
        // Read the mouse input axis
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
 
        // Call our Adjust to 360 degrees and clamp function
        Adjust360andClamp();
 
        // Call our look left and right function.
        //KeyLookAround();
 
        // Call our look up and down function.
        //KeyLookUp();
    }
}
 
void KeyLookAround (){
    //      If you're not using it, you can delete this whole function. 
    //      Just be sure to delete where it's called in Update.
 
    // Call our Adjust to 360 degrees and clamp function
    Adjust360andClamp();
 
    // Transform our X angle
    transform.localRotation = Quaternion.AngleAxis (rotationX, Vector3.up);
}
 
void KeyLookUp (){
    // Adjust for 360 degrees and clamp
    Adjust360andClamp();
 
    // Transform our Y angle, multiply so we don't loose our X transform
    transform.localRotation *= Quaternion.AngleAxis (rotationY, Vector3.back);
}
 
void Adjust360andClamp (){
    //      This prevents your rotation angle from going beyond 360 degrees and also 
    //      clamps the angle to the min and max values set in the Inspector.
 
    // During in-editor play, the Inspector won't show your angle properly due to 
    // dealing with floating points. Uncomment this Debug line to see the angle in the console.
 
    // Debug.Log (rotationX);
 
    // Don't let our X go beyond 360 degrees + or -
    if (rotationX < -360)
    {
        rotationX += 360;
    }
    else if (rotationX > 360)
    {
        rotationX -= 360;
    }   
 
    // Don't let our Y go beyond 360 degrees + or -
    if (rotationY < -360)
    {
        rotationY += 360;
    }
    else if (rotationY > 360)
    {
        rotationY -= 360;
    }
 
    // Clamp our angles to the min and max set in the Inspector
    rotationX = Mathf.Clamp (rotationX, minimumX, maximumX);
    rotationY = Mathf.Clamp (rotationY, minimumY, maximumY);
}
 
void Start (){
    // Make the rigid body not change rotation
    if (rigidbody)
    {
        rigidbody.freezeRotation = true;
    }
}
}