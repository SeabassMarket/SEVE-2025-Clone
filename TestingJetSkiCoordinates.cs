using NUnit.Framework.Internal.Commands;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TestingJetSkiCoordinates : MonoBehaviour
{
    [Header("Test Objects")]
    public GameObject rightHand;
    public GameObject leftHand;
    public GameObject temporaryHandle;
    public GameObject rightDirectionArrowTester;
    public GameObject leftDirectionArrowTester;
    public GameObject centerDirectionArrowTester;
    public TMP_Text directionTestText;

    [Header("Manual Offsets")]
    public float offset;

    [Header("Turning Constants")]
    public float turningForceCoefficient;
    public float maxTurningForce;
    public float turnDirectionExponent;
    public float speedExponent;
    public float testingDirection;

    //Rigidbody
    private Rigidbody rb;

    //Orientation control script (to get modified angle values)
    private JetSkiOrientationControl jSOC;

    //Private variable (will be taken from another script)
    private float waterLevel;

    // Start is called before the first frame update
    void Start()
    {
        //Instantiate instance variables
        rb = GetComponent<Rigidbody>();
        jSOC = GetComponent<JetSkiOrientationControl>();
        waterLevel = GetComponent<JetSkiBuoyancy>().waterLevel;
    }

    // Update is called once per frame
    void Update()
    {   
        //Get the steering angle provided by the right hand
        float rightDirectionOfHand = getRightSteeringAngle();
        //Show the direction that this would be pointing toward
        rightDirectionArrowTester.transform.localEulerAngles = new Vector3(
            rightDirectionArrowTester.transform.localEulerAngles.x,
            rightDirectionOfHand,
            rightDirectionArrowTester.transform.localEulerAngles.z
            );

        //Get the steering angle provided by the left hand
        float leftDirectionOfHand = getLeftSteeringAngle();
        //Show the direction that this would be pointing toward
        leftDirectionArrowTester.transform.localEulerAngles = new Vector3(
            leftDirectionArrowTester.transform.localEulerAngles.x,
            leftDirectionOfHand,
            leftDirectionArrowTester.transform.localEulerAngles.z
            );

        //DO CALCULATIONS FOR CENTER
        //Take average
        float centerDirectionOfHand = getAverageSteeringAngle();
        //Show the direction that this would be pointing toward
        centerDirectionArrowTester.transform.localEulerAngles = new Vector3(
            centerDirectionArrowTester.transform.localEulerAngles.x,
            centerDirectionOfHand,
            centerDirectionArrowTester.transform.localEulerAngles.z
            );

        //Display the directions in text
        directionTestText.text = (
            "Right: " + rightDirectionOfHand + "\n" +
            "Left: " + leftDirectionOfHand + "\n" +
            "Center: " + centerDirectionOfHand
            );
    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Apply a centripetal impulse if the steering wheel is turned and the jet ski is in the water
        if (testingDirection != 0 && rb.position.y < waterLevel)
        {
            //Calculate the magnitude of centripetal impulse, and limit it if it is too big
            float centripetalImpulseMagnitude = (Mathf.Pow(jSOC.getSpeedXZPlane(), speedExponent) * 
                Mathf.Pow(Mathf.Abs(testingDirection), turnDirectionExponent) * 
                turningForceCoefficient);
            if(centripetalImpulseMagnitude > maxTurningForce)
            {
                Debug.Log("Centripetal impulse limited from " + centripetalImpulseMagnitude + " to " + maxTurningForce);
                centripetalImpulseMagnitude = maxTurningForce;
            }

            //Calculate the direction the centripetal impulse will be applied
            float directionFacing = jSOC.getModifiedYRotation();
            int multiplier = 1;
            if (testingDirection < 0)
            {
                multiplier *= -1;
            }
            if (jSOC.isFlippedOver())
            {
                multiplier *= -1;
            }
            float centripetalImpulseDirection = directionFacing + 90 * multiplier;

            //Create the vector direction of where it will be applied
            Vector3 centripetalImpulseVector = new Vector3(Mathf.Sin(centripetalImpulseDirection * Mathf.Deg2Rad),
                0,
                Mathf.Cos(centripetalImpulseDirection * Mathf.Deg2Rad)) * centripetalImpulseMagnitude;

            //Create the vector direction of where a counter impulse will be applied
            //This is to conserve energy. Essentially, this simulates the jet ski redirecting drag towards the center
            float directionTraveling = jSOC.getDirectionTraveling();
            Vector3 counterImpulseVector = new Vector3(Mathf.Sin(directionTraveling * Mathf.Deg2Rad),
                0,
                Mathf.Cos(directionTraveling * Mathf.Deg2Rad)) * centripetalImpulseMagnitude * -1;

            //Apply the two impulses (in the form of velocity, just in case we change mass in the future)
            rb.AddForce(centripetalImpulseVector, ForceMode.Acceleration);
            rb.AddForce(counterImpulseVector, ForceMode.Acceleration);
        }
    }

    //Returns the steering angle for the right hand
    private float getRightSteeringAngle()
    {
        //DO CALCULATIONS FOR RIGHT HAND
        //X coordinate is pretty basic
        float xCoordinate = rightHand.transform.localPosition.x - temporaryHandle.transform.localPosition.x;
        //Calculations for the new y coordinate projection
        float newX = (rightHand.transform.localPosition.z - temporaryHandle.transform.localPosition.z) * -1;
        float newY = rightHand.transform.localPosition.y - temporaryHandle.transform.localPosition.y;
        float yCoordinate = temporaryHandle.transform.localEulerAngles.x * Mathf.Deg2Rad - Mathf.Atan2(newY, newX);
        yCoordinate = Mathf.Cos(yCoordinate) * Mathf.Sqrt(newX * newX + newY * newY) * -1;
        //Calculate the direction this would be pointing towards by taking the perpendicular direction
        float rightDirectionOfHand = Mathf.Atan2(yCoordinate, xCoordinate) * Mathf.Rad2Deg;
        rightDirectionOfHand = rightDirectionOfHand * -1 + offset;
        //Change it to negative if it is above 180 degrees
        if (rightDirectionOfHand < 0)
        {
            rightDirectionOfHand = 360 + rightDirectionOfHand;
        }
        return rightDirectionOfHand;
    }

    //Returns the steering angle for the left hand
    private float getLeftSteeringAngle()
    {
        //DO CALCULATIONS FOR LEFT HAND
        //X coordinate is pretty basic
        float xCoordinate = leftHand.transform.localPosition.x - temporaryHandle.transform.localPosition.x;
        //Calculations for the new y coordinate projection
        float newX = (leftHand.transform.localPosition.z - temporaryHandle.transform.localPosition.z) * -1;
        float newY = leftHand.transform.localPosition.y - temporaryHandle.transform.localPosition.y;
        float  yCoordinate = temporaryHandle.transform.localEulerAngles.x * Mathf.Deg2Rad - Mathf.Atan2(newY, newX);
        yCoordinate = Mathf.Cos(yCoordinate) * Mathf.Sqrt(newX * newX + newY * newY) * -1;
        //Calculate the direction this would be pointing towards by taking the perpendicular direction
        float leftDirectionOfHand = Mathf.Atan2(yCoordinate, xCoordinate) * Mathf.Rad2Deg;
        leftDirectionOfHand = leftDirectionOfHand * -1 - offset + 180;
        //Change it to negative if it is above 180 degrees
        if (leftDirectionOfHand < 0)
        {
            leftDirectionOfHand = 360 + leftDirectionOfHand;
        }
        return leftDirectionOfHand;
    }

    //Returns the average/center steering angle of both hands
    private float getAverageSteeringAngle()
    {
        //DO CALCULATIONS FOR CENTER
        //Get both the right and left steering angles
        float rightDirectionOfHand = getRightSteeringAngle();
        float leftDirectionOfHand = getLeftSteeringAngle();
        //Take average
        float averageDirectionOfHand = (rightDirectionOfHand + leftDirectionOfHand) / 2;
        //Change values for "intuitive" directions if needed
        if (Mathf.Abs(rightDirectionOfHand - leftDirectionOfHand) > 180)
        {
            averageDirectionOfHand = (averageDirectionOfHand + 180) % 360;
        }
        return averageDirectionOfHand;
    }
}
