using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class JetSkiOrientationControl : MonoBehaviour
{
    [Header("Helper Objects")]
    public GameObject directionHelperXAxis;
    public GameObject directionHelperYAxis;
    public GameObject directionHelperZAxis;

    [Header("Physical Constants")]

    [Header("X Correction Constants")]
    public float optimalCorrectionAngleX;
    public float changeToOptimalCorrectionAngleXCoefficient;
    public float xCorrectionCoefficient;
    public float xCorrectionAngleCoefficient;
    public float maxXCorrectionValue;

    [Header("Y Correction Constants")]
    public float yCorrectionSpeedExponent;
    public float yCorrectionCoefficient;
    public float yCorrectionAngleExponent;
    public float maxYCorrectionValue;

    [Header("Z Correction Constants")]
    public float optimalCorrectionAngleZ;
    public float ZCorrectionCoefficient;
    public float ZCorrectionAngleExponent;
    public float maxZCorrectionValue;
    private float originalOptimalCorrectionAngleZ;

    //Rigidbody
    private Rigidbody rb;

    //Script for buoyancy (has water information)
    private float waterLevel;

    // Start is called before the first frame update
    void Start()
    {
        //Get the rigidbody
        rb = GetComponent<Rigidbody>();

        //Get the jetSkiBuoyancy
        waterLevel = GetComponent<JetSkiBuoyancy>().waterLevel;

        //Set the original optimal Z correction angle
        originalOptimalCorrectionAngleZ = optimalCorrectionAngleZ;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Change the anguler velocity of the jet ski so it levels out from front to back if it's in the water
        correctXAxis();

        //Change the angular velocity in the direction of the jet ski's movement depending on the jetski's if it's in the water
        correctYAxis();

        //Change the anguler velocity of the jet ski so it levels out from side to side (the tilt of the jet ski) if it's in the water
        correctZAxis();

    }

    //Change the anguler velocity of the jet ski so it levels out from front to back if it's in the water
    private void correctXAxis()
    {
        if(rb.position.y < waterLevel)
        {
            //Calculate optimum angle
            float optimalAngle = optimalCorrectionAngleX + (changeToOptimalCorrectionAngleXCoefficient * getSpeedXZPlane());

            //Get the difference in angle
            float differenceInAngle = optimalAngle - getModifiedXRotation();
            if (Mathf.Abs(differenceInAngle) > 180)
            {
                differenceInAngle += (360 * Mathf.Abs(differenceInAngle) / differenceInAngle * -1);
            }

            //Only continue if the difference in angle is not 0
            if(differenceInAngle != 0)
            {
                //Create the correction magnitude
                float correctionMagnitude = (
                    Mathf.Pow(Mathf.Abs(differenceInAngle), xCorrectionAngleCoefficient) * (Mathf.Abs(differenceInAngle) / differenceInAngle) *
                    xCorrectionCoefficient);
                if (Mathf.Abs(correctionMagnitude) > maxXCorrectionValue)
                {
                    //Debug.Log("X Correction limited from " + correctionMagnitude + " to " + maxXCorrectionValue);
                    correctionMagnitude = maxXCorrectionValue * Mathf.Abs(correctionMagnitude) / correctionMagnitude;
                }

                //Create the x correction based on the correction magnitude and the modified y angle orientation
                float xCorrection = correctionMagnitude * Mathf.Cos(getModifiedYRotation() * Mathf.Deg2Rad) * -1;
                if (directionHelperYAxis.transform.position.y < rb.position.y)
                {
                    xCorrection *= -1;
                }

                //Create the z correction based on the correction magnitude and the modified y angle orientation
                float zCorrection = correctionMagnitude * Mathf.Sin(getModifiedYRotation() * Mathf.Deg2Rad);
                if (directionHelperYAxis.transform.position.y < rb.position.y)
                {
                    zCorrection *= -1;
                };

                //Change the angular velocity
                rb.angularVelocity = new Vector3(
                    rb.angularVelocity.x + xCorrection,
                    rb.angularVelocity.y,
                    rb.angularVelocity.z + zCorrection
                    );
            }
        }
    }

    //Change the angular velocity in the direction of the jet ski's movement depending on the jetski's if it's in the water
    private void correctYAxis()
    {
        if (rb.position.y < waterLevel)
        {
            //Get velocity direction and make sure it is a positive angle
            float velocityDirection = getDirectionTraveling();

            //Calculate the difference in angle and switch forms if the absolute value is more than 180
            float differenceInAngle = velocityDirection - getModifiedYRotation();

            //Only continue if difference in angle is not 0 (you wouldn't need to change anything anyway!)
            if (differenceInAngle != 0)
            {
                if (Mathf.Abs(differenceInAngle) > 180)
                {
                    differenceInAngle -= 360 * Mathf.Abs(differenceInAngle) / differenceInAngle;
                }

                //Get the velocity in the 2D plane of x and z (aka flat ground, no height/y)
                float magnitudeIn2DPlane = Mathf.Sqrt(rb.velocity.x * rb.velocity.x + rb.velocity.z * rb.velocity.z);

                //Calculate the y correction and cap if it is too big
                float yCorrection = (
                    Mathf.Pow(Mathf.Abs(differenceInAngle), yCorrectionAngleExponent) * (Mathf.Abs(differenceInAngle) / differenceInAngle) *
                    Mathf.Pow(magnitudeIn2DPlane, yCorrectionSpeedExponent) *
                    yCorrectionCoefficient
                    );
                if (Mathf.Abs(yCorrection) > maxYCorrectionValue)
                {
                    //Debug.Log("Max Y Correction Value used: " + yCorrection + " to " + (maxYCorrectionValue * (Mathf.Abs(yCorrection) / yCorrection)));
                    yCorrection = maxYCorrectionValue * (Mathf.Abs(yCorrection) / yCorrection);
                }

                //Change the angular velocity
                rb.angularVelocity += Vector3.up * yCorrection;
            }
        }
    }

    //Change the anguler velocity of the jet ski so it levels out from side to side (the tilt of the jet ski) if it's in the water
    private void correctZAxis()
    {
        if(rb.position.y < waterLevel)
        {
            //Get the difference in angle
            float differenceInAngle = optimalCorrectionAngleZ - getModifiedZRotation();
            if (Mathf.Abs(differenceInAngle) > 180)
            {
                differenceInAngle += (360 * Mathf.Abs(differenceInAngle) / differenceInAngle * -1);
            }

            //Only continue if the difference in angle is not 0
            if (differenceInAngle != 0)
            {
                //Create the correction magnitude
                float correctionMagnitude = (
                        Mathf.Pow(Mathf.Abs(differenceInAngle), ZCorrectionAngleExponent) * (Mathf.Abs(differenceInAngle) / differenceInAngle) *
                        ZCorrectionCoefficient);
                if (Mathf.Abs(correctionMagnitude) > maxZCorrectionValue)
                {
                    Debug.Log("Z Correction limited from " + correctionMagnitude + " to " + maxZCorrectionValue);
                    correctionMagnitude = maxZCorrectionValue * Mathf.Abs(correctionMagnitude) / correctionMagnitude;
                }

                //Create the x correction based on the correction magnitude and the modified y angle orientation
                float xCorrection = correctionMagnitude * Mathf.Sin(getModifiedYRotation() * Mathf.Deg2Rad);

                //Create the z correction based on the correction magnitude and the modified y angle orientation
                float zCorrection = correctionMagnitude * Mathf.Cos(getModifiedYRotation() * Mathf.Deg2Rad);

                //Change the angular velocity
                rb.angularVelocity = new Vector3(
                    rb.angularVelocity.x + xCorrection,
                    rb.angularVelocity.y,
                    rb.angularVelocity.z + zCorrection
                    );
            }
        }
    }

    //Returns the direction in the X-Z plane the jet ski is traveling
    public float getDirectionTraveling()
    {
        //Get velocity direction and make sure it is a positive angle
        float velocityDirection = Mathf.Atan2(rb.velocity.x, rb.velocity.z) * Mathf.Rad2Deg;
        if (velocityDirection < 0)
        {
            velocityDirection += 360;
        }
        return velocityDirection;
    }

    //Returns the modified Y euler angle by using a helper object
    public float getModifiedYRotation()
    {
        float yRotation = Mathf.Atan2(directionHelperZAxis.transform.position.x - rb.position.x, 
            directionHelperZAxis.transform.position.z - rb.position.z) * Mathf.Rad2Deg;
        if (yRotation < 0)
        {
            yRotation = (yRotation + 360) % 360;
        }
        return yRotation;
    }

    //Returns the modified X euler angle by using two helper objects
    public float getModifiedXRotation()
    {
        float xRotation = Mathf.Asin((directionHelperZAxis.transform.position.y - rb.position.y) / 
            Vector3.Distance(rb.position, directionHelperZAxis.transform.position)) * Mathf.Rad2Deg;
        if (directionHelperYAxis.transform.position.y < rb.position.y)
        {
            xRotation = ((180 * Mathf.Abs(xRotation) / xRotation) - xRotation);
        }
        return xRotation;
    }

    //Returns the modified Z euler angle by using two helper objects. This is the "tilt" of the jet ski side-to-side
    public float getModifiedZRotation()
    {
        float xDisplacement = directionHelperXAxis.transform.position.x - rb.position.x;
        float zDisplacement = directionHelperXAxis.transform.position.z - rb.position.z;
        float zRotation = Mathf.Atan2(directionHelperXAxis.transform.position.y - rb.position.y,
            Mathf.Sqrt(xDisplacement * xDisplacement + zDisplacement * zDisplacement)) * Mathf.Rad2Deg;
        if (directionHelperYAxis.transform.position.y < rb.position.y)
        {
            if(zRotation != 0)
            {
                zRotation = (180 * Mathf.Abs(zRotation) / zRotation) - zRotation;
            }
            else
            {
                zRotation = 180;
            }
        }
        return zRotation;
    }

    //Return modified euler angle orientation by using helper objects
    public Vector3 getModifiedEulerAngles()
    {
        return new Vector3(getModifiedXRotation(), getModifiedYRotation(), getModifiedZRotation());
    }

    //Returns whether the jet ski is flipped over
    public bool isFlippedOver()
    {
        return directionHelperYAxis.transform.position.y < rb.position.y;
    }

    //Returns the speed of the jet ski in the X-Z plane (magnitude)
    public float getSpeedXZPlane()
    {
        return new Vector2(rb.velocity.x, rb.velocity.z).magnitude;
    }

    //Returns the original optimal Z correction angle
    public float getOriginalOptimalZCorrectionAngle()
    {
        return originalOptimalCorrectionAngleZ;
    }
}
