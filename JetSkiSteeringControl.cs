using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetSkiSteeringControl : MonoBehaviour
{
    [Header("VR Related")]
    public GameObject rightHand;
    public GameObject leftHand;
    public GameObject rightHandCollider;
    public GameObject leftHandCollider;
    private InputControl VRInput;

    [Header("Manual Offsets")]
    public float angleOffset;

    [Header("Steering")]
    public GameObject handlebarParent;
    public GameObject handlebar;
    public GameObject steeringRightHand;
    public GameObject steeringLeftHand;
    public GameObject rightHandProxyLocation;
    public GameObject leftHandProxyLocation;
    public float gripStrengthNeededToSteer;

    [Header("Turning Constants")]
    public float maxTurningDirectionMagnitude;
    public float minTurningDirectionMagnitude;
    public float turningForceCoefficient;
    public float maxTurningForce;
    public float turnDirectionExponent;
    public float speedExponent;
    public float tiltCoefficient;

    [Header("Handlebar")]
    public float slowBackCoefficient;
    public float slowBackExponent;
    public float snapToZero;

    //Rigidbody
    private Rigidbody rb;

    //Orientation control script (to get modified angle values)
    private JetSkiOrientationControl jSOC;

    //Private variable (will be taken from another script)
    private float waterLevel;

    //Collider scripts for each hand
    private HandleBarCollision rightHandleBarCollision;
    private HandleBarCollision leftHandleBarCollision;

    //Boolean values for if the right hand or left hand is gripped on to the handlebar
    private bool rightGripped = false;
    private bool leftGripped = false;

    // Start is called before the first frame update
    void Start()
    {
        //Instantiate instance variables
        VRInput = GetComponent<InputControl>();
        rb = GetComponent<Rigidbody>();
        jSOC = GetComponent<JetSkiOrientationControl>();
        waterLevel = GetComponent<JetSkiBuoyancy>().waterLevel;
        rightHandleBarCollision = rightHandCollider.GetComponent<HandleBarCollision>();
        leftHandleBarCollision = leftHandCollider.GetComponent<HandleBarCollision>();

        //Call update
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        //Determine whether the right hand is gripped on or not
        if(rightHandleBarCollision.getIsWithinRange() && VRInput.getRightGripValue() > gripStrengthNeededToSteer)
        {
            //Turn on gripped stuff
            rightGripped = true;
            steeringRightHand.SetActive(true);
            rightHand.SetActive(false);
        }
        else
        {
            //Turn off gripped stuff
            rightGripped = false;
            steeringRightHand.SetActive(false);
            rightHand.SetActive(true);
        }

        //Determine whether the left hand is gripped on or not
        if(leftHandleBarCollision.getIsWithinRange() && VRInput.getLeftGripValue() > gripStrengthNeededToSteer)
        {
            //Turn on gripped stuff
            leftGripped = true;
            steeringLeftHand.SetActive(true);
            leftHand.SetActive(false);
        }
        else
        {
            //Turn off gripped stuff
            leftGripped = false;
            steeringLeftHand.SetActive(false);
            leftHand.SetActive(true);
        }

        //Get the steering angle and determine if we need to do the slow back animation
        float steeringAngle = 0;
        bool slowBack = false;
        if (rightGripped && leftGripped)
        {
            steeringAngle = getAverageSteeringAngle();
        }
        else if (rightGripped)
        {
            steeringAngle = getRightSteeringAngle();
        }
        else if (leftGripped)
        {
            steeringAngle = getLeftSteeringAngle();
        }
        else
        {
            slowBack = true;
        }

        //Do the slowback animation if needed
        if (slowBack)
        {
            float handlebarAngle = convertTo180Range(handlebarParent.transform.localEulerAngles.z);
            float turnBack = Mathf.Sign(handlebarAngle) * Mathf.Pow(Mathf.Abs(handlebarAngle), slowBackExponent) * slowBackCoefficient * Time.deltaTime;
            float newAngle = handlebarAngle - turnBack;
            if (Mathf.Abs(newAngle) < snapToZero)
            {
                newAngle = 0;
            }
            handlebarParent.transform.localEulerAngles = new Vector3(
                handlebarParent.transform.localEulerAngles.x,
                handlebarParent.transform.localEulerAngles.y,
                newAngle
                );
        }
        else if (steeringAngle != 0)
        {
            //First, convert the angle to a range from -180 to 180
            steeringAngle = convertTo180Range(steeringAngle);

            //Then, limit the angle if it is too big
            if(Mathf.Abs(steeringAngle) > maxTurningDirectionMagnitude)
            {
                steeringAngle = maxTurningDirectionMagnitude * Mathf.Sign(steeringAngle);
            }
            
            //Finally, turn the steering wheel into place
            handlebarParent.transform.localEulerAngles = new Vector3(handlebarParent.transform.localEulerAngles.x,
                handlebarParent.transform.localEulerAngles.y,
                steeringAngle * -1);
        }
    }

    private void applySteeringForce()
    {
        //First, get the steering angle from the handlebar
        float steeringAngle = convertTo180Range(handlebarParent.transform.localEulerAngles.z) * -1;

        //Apply a centripetal impulse if the steering wheel is turned and the jet ski is in the water
        if (Mathf.Abs(steeringAngle) > minTurningDirectionMagnitude && rb.position.y < waterLevel)
        {
            //Calculate new steering angle
            steeringAngle = (Mathf.Abs(steeringAngle) - minTurningDirectionMagnitude) * Mathf.Sign(steeringAngle);

            //Calculate the magnitude of centripetal impulse, and limit it if it is too big
            float centripetalImpulseMagnitude = (Mathf.Pow(jSOC.getSpeedXZPlane(), speedExponent) *
                Mathf.Pow(Mathf.Abs(steeringAngle), turnDirectionExponent) *
                turningForceCoefficient);
            if (centripetalImpulseMagnitude > maxTurningForce)
            {
                Debug.Log("Centripetal impulse limited from " + centripetalImpulseMagnitude + " to " + maxTurningForce);
                centripetalImpulseMagnitude = maxTurningForce;
            }

            //Calculate the direction the centripetal impulse will be applied
            float directionFacing = jSOC.getModifiedYRotation();
            int multiplier = 1;
            if (steeringAngle < 0)
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

            //Update the optimal Z angle in jSOC
            float newOptimalZAngle = centripetalImpulseMagnitude * tiltCoefficient * Mathf.Sign(steeringAngle);
            jSOC.optimalCorrectionAngleZ = jSOC.getOriginalOptimalZCorrectionAngle() + 
                centripetalImpulseMagnitude * tiltCoefficient * Mathf.Sign(steeringAngle) * -1;
        }
        else
        {
            jSOC.optimalCorrectionAngleZ = jSOC.getOriginalOptimalZCorrectionAngle();
        }
    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Apply the force (will only apply if in water plus is moving)
        applySteeringForce();
    }

    //Returns the steering angle for the right hand
    private float getRightSteeringAngle()
    {
        //DO CALCULATIONS FOR RIGHT HAND
        //X coordinate is pretty basic
        float xCoordinate = rightHandProxyLocation.transform.localPosition.x - handlebarParent.transform.localPosition.x;
        //Calculations for the new y coordinate projection
        float newX = (rightHandProxyLocation.transform.localPosition.z - handlebarParent.transform.localPosition.z) * -1;
        float newY = rightHandProxyLocation.transform.localPosition.y - handlebarParent.transform.localPosition.y;
        float yCoordinate = (-90 - handlebar.transform.localEulerAngles.x) * Mathf.Deg2Rad - Mathf.Atan2(newY, newX);
        yCoordinate = Mathf.Cos(yCoordinate) * Mathf.Sqrt(newX * newX + newY * newY) * -1;
        //Calculate the direction this would be pointing towards by taking the perpendicular direction
        float rightDirectionOfHand = Mathf.Atan2(yCoordinate, xCoordinate) * Mathf.Rad2Deg;
        rightDirectionOfHand = rightDirectionOfHand * -1 + angleOffset;
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
        float xCoordinate = leftHandProxyLocation.transform.localPosition.x - handlebarParent.transform.localPosition.x;
        //Calculations for the new y coordinate projection
        float newX = (leftHandProxyLocation.transform.localPosition.z - handlebarParent.transform.localPosition.z) * -1;
        float newY = leftHandProxyLocation.transform.localPosition.y - handlebarParent.transform.localPosition.y;
        float yCoordinate = (-90 - handlebar.transform.localEulerAngles.x) * Mathf.Deg2Rad - Mathf.Atan2(newY, newX);
        yCoordinate = Mathf.Cos(yCoordinate) * Mathf.Sqrt(newX * newX + newY * newY) * -1;
        //Calculate the direction this would be pointing towards by taking the perpendicular direction
        float leftDirectionOfHand = Mathf.Atan2(yCoordinate, xCoordinate) * Mathf.Rad2Deg;
        leftDirectionOfHand = leftDirectionOfHand * -1 - angleOffset + 180;
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

    //Converts an angle to a distance from 0, aka 0 to 180 inclusive, 0 to -180 exclusive
    private float convertTo180Range(float angle)
    {
        float sign = Mathf.Sign(angle);
        angle = Mathf.Abs(angle);
        angle -= Mathf.Floor(angle / 360) * 360;
        if(angle > 180)
        {
            angle = angle - 360;
        }
        return angle * sign;
    }

    //Return whether the right hand is gripped
    public bool isRightGripped()
    {
        return rightGripped;
    }

    //Return whether the left hand is gripped
    public bool isLeftGripped()
    {
        return leftGripped;
    }
}
