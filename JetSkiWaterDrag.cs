using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class JetSkiWaterDrag : MonoBehaviour
{

    [Header("Water Constants")]
    [Header("Y Drag")]
    public float dragCoefficientWaterY;
    public float maxDragForceY;

    [Header("X-Z Plane Drag")]
    public float dragCoefficientXZPlane;
    public float maxXZPlaneDrag;
    public float differenceInAngleExponent;
    public float differenceInAngleUpToConstant;

    //Private variables that will be taken from another script
    private float waterLevel;
    private float waterDensity;

    //Get the orientation control script so we can read angles
    private JetSkiOrientationControl jetSkiOrientationControlScript;

    //Rigidbody
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        waterLevel = GetComponent<JetSkiBuoyancy>().waterLevel;
        waterDensity = GetComponent<JetSkiBuoyancy>().waterDensity;
        jetSkiOrientationControlScript = GetComponent<JetSkiOrientationControl>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Get the depth in the water
        float depthInWater = waterLevel - transform.position.y;

        //Only continue adding force if it is underwater
        if (depthInWater > 0)
        {
            //Calculate the magnitude of the speed in the X-Z plane
            float speedXZPlane = new Vector2(rb.velocity.x, rb.velocity.z).magnitude;

            if (speedXZPlane > 0)
            {
                //Apply drag for the X-Z plane. This is directly related to the difference in angle, square of velocity, 
                //Get velocity direction and make sure it is a positive angle
                float velocityDirection = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
                if (velocityDirection < 0)
                {
                    velocityDirection += 360;
                }

                //Calculate the difference in angle and switch forms if the absolute value is more than 180
                float differenceInAngle = velocityDirection - jetSkiOrientationControlScript.getModifiedYRotation();

                //Change it to negative if it's more than 180 degrees
                if (Mathf.Abs(differenceInAngle) > 180)
                {
                    differenceInAngle -= 360 * Mathf.Abs(differenceInAngle) / differenceInAngle;
                }
                differenceInAngle = Mathf.Abs(differenceInAngle); //Just switch the absolute value here, magnitude only matters from here on out

                //Change the angle to a value from 0 to 90 (essentially, how far it is from either 0 or 180 degrees)
                if (differenceInAngle > 90)
                {
                    differenceInAngle = 180 - differenceInAngle;
                }

                //Change difference in angle so that it has a value of 1 - whatever the variable is set to
                differenceInAngle = 1 + (differenceInAngle / 90) * (differenceInAngleUpToConstant - 1);

                //Get the drag force that is going to be applied (magnitude)
                float dragForceXZPlane = (Mathf.Pow(differenceInAngle, differenceInAngleExponent) *
                    speedXZPlane * speedXZPlane *
                    dragCoefficientXZPlane
                    );

                if (dragForceXZPlane > maxXZPlaneDrag) //Limit it if the drag force is too much
                {
                    //Debug.Log("Drag force XZ plane limited to max: " + dragForceXZPlane + " to " + maxXZPlaneDrag);
                    dragForceXZPlane = maxXZPlaneDrag;
                }

                //Calculate and direct the force in the direction opposite of the movement in the X-Z plane
                Vector3 dragForceXZPlaneVector = new Vector3(rb.velocity.x, 0, rb.velocity.z) * -1 / speedXZPlane * dragForceXZPlane;

                //Apply the calculated force
                rb.AddForce(dragForceXZPlaneVector, ForceMode.Acceleration);
            }

            //Apply drag for y
            if (rb.velocity.y < 0)
            {
                //Drag force in the y direction
                float dragForceY = rb.velocity.y * rb.velocity.y * waterDensity * dragCoefficientWaterY;

                //Limit drag force if it is too big
                if(Mathf.Abs(dragForceY) > maxDragForceY)
                {
                    //Debug.Log("Drag force Y limited to max: " + dragForceY + " to " + maxDragForceY);
                    dragForceY = maxDragForceY * Mathf.Abs(dragForceY) / dragForceY;
                }

                //Compile the different drag forces
                Vector3 force = new Vector3(0, dragForceY, 0);

                //Add the force
                rb.AddForce(force, ForceMode.Force);
            }
        }
    }
}
