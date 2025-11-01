using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DragTest : MonoBehaviour
{
    [Header("Velocity Variables")]
    public float velocityMagnitudeToAdd;

    [Header("Object")]
    public GameObject dragObject;
    private Rigidbody rb;

    [Header("Drifting Variables")]
    public float maxTimeAllowedDrifting;
    private Vector3 previousPosition;
    private float timeDrifting;
    private float lastDriftTime;
    private float lastVelocityTime;

    [Header("Drag Variables")]
    public float dragForceCoefficient;
    public float minDragForce;
    private float dragForce;

    // Start is called before the first frame update
    void Start()
    {
        //Get rigidbody
        rb = dragObject.GetComponent<Rigidbody>();
        //Initialize the position
        previousPosition = dragObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        //Set the velocity
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            //Unfreeze
            rb.constraints = RigidbodyConstraints.None;
            //Create the vector with the base ratios of velocities in the 3 directions
            Vector3 vectorVelocity = vectorDirectionToBaseVectorVelocity(dragObject.transform.rotation.eulerAngles);
            //Add velocity to the object by taking the base ratios and multiplying it by a magnitude
            if (velocityMagnitudeToAdd < 0) { velocityMagnitudeToAdd = velocityMagnitudeToAdd * -1; }
            rb.AddForce(vectorVelocity * velocityMagnitudeToAdd, ForceMode.VelocityChange);
        }
    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Run drag force program only if the velocity is greater than 0
        if(rb.velocity.magnitude > 0.0f)
        {
            dragForce = rb.velocity.magnitude * rb.velocity.magnitude * dragForceCoefficient;
            if (dragForce < minDragForce)
            {
                dragForce = minDragForce;
            }
            //Change the magnitude of the velocity
            float newVelocityMagnitude = rb.velocity.magnitude - dragForce * Time.fixedDeltaTime;
            if (newVelocityMagnitude < 0.0f)
            {
                newVelocityMagnitude = 0.0f;
            }
            //Create the vector with the base ratios of velocities in the 3 directions
            Vector3 vectorVelocity = rb.velocity / rb.velocity.magnitude;
            rb.velocity = vectorVelocity * newVelocityMagnitude;
        }

        //Really kill velocity if for some reason it is still moving (normal happens after collisions)
        if (rb.velocity.magnitude > 0)
        {
            lastVelocityTime = Time.time;
        }
        if (Vector3.Distance(previousPosition, dragObject.transform.position) > Mathf.Pow(10, -6) && rb.velocity.magnitude == 0)
        {
            if (Time.time - lastDriftTime > 1.0f)
            {
                timeDrifting = timeDrifting + Time.fixedDeltaTime;
            }
            else
            {
                timeDrifting = timeDrifting + Time.time - lastDriftTime;
            }
            //Let it drift a little bit before stopping
            if (timeDrifting > maxTimeAllowedDrifting && Time.time - lastVelocityTime > maxTimeAllowedDrifting)
            {
                rb.constraints = RigidbodyConstraints.FreezeAll;
                Debug.Log("Freezed");
            }
            lastDriftTime = Time.time;
        }
        else if (Time.time - lastDriftTime > 1.0f || rb.velocity.magnitude > 0) //Reset the time drifting if drifting hasn't been detected for over a second
        {
            timeDrifting = 0;
            rb.constraints = RigidbodyConstraints.None;
        }

        //Reset previous position for next frame
        previousPosition = dragObject.transform.position;
    }

    public static Vector3 vectorDirectionToBaseVectorVelocity(Vector3 velocityDirection)
    {
        //Take the originial velocity direction and change it into values in the x, y, and z direction
        //This one is modified for the hands because the way they are implemented and rotated the z becomes the x
        float xVelocity = Mathf.Cos(velocityDirection.x * Mathf.Deg2Rad * -1) * Mathf.Sin(velocityDirection.y * Mathf.Deg2Rad);
        float yVelocity = Mathf.Sin(velocityDirection.x * Mathf.Deg2Rad * -1);
        float zVelocity = Mathf.Cos(velocityDirection.x * Mathf.Deg2Rad * -1) * Mathf.Cos(velocityDirection.y * Mathf.Deg2Rad);
        return new Vector3(xVelocity, yVelocity, zVelocity);
    }

}
