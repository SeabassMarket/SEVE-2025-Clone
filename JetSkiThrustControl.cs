using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class JetSkiThrustControl : MonoBehaviour
{
    [Header("Thrust Properties")]
    public float maxThrustForce;

    //Water level from other script
    private float waterLevel;

    //Rigidbody
    private Rigidbody rb;

    //This is for getting angles and stuff
    private JetSkiOrientationControl jSOC;

    //This will be used to check if a hand is gripped
    private JetSkiSteeringControl jSSC;

    //VR input control
    private InputControl VRInput;

    //Just keep track of the trigger values it's reading
    private float averageTriggerValueForOtherScripts;

    // Start is called before the first frame update
    void Start()
    {
        //Instantiate instance variables
        rb = GetComponent<Rigidbody>();
        waterLevel = GetComponent<JetSkiBuoyancy>().waterLevel;
        jSOC = GetComponent<JetSkiOrientationControl>();
        jSSC = GetComponent<JetSkiSteeringControl>();
        VRInput = GetComponent<InputControl>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Determine the average trigger value
        int count = 0;
        float sum = 0;
        if (jSSC.isRightGripped())
        {
            sum += VRInput.getRightTriggerValue();
            count++;
        }
        if (jSSC.isLeftGripped())
        {
            sum += VRInput.getLeftTriggerValue();
            count++;
        }

        //Only continue if there is at least one valid trigger value
        if (count > 0)
        {
            //Calculate the thrust magnitude
            float averageTriggerValue = sum / count;
            setAverageTriggerValue(averageTriggerValue);

            //Continue if the jet ski is in the water
            if (rb.position.y < waterLevel)
            {
                //Add force if it is in water
                float thrustMagnitude = averageTriggerValue * maxThrustForce;

                //Get the direction the jet ski is facing
                float directionFacing = jSOC.getModifiedYRotation();

                //Calculate the force vector
                Vector3 thrustForceVector = new Vector3(
                    Mathf.Sin(directionFacing * Mathf.Deg2Rad),
                    0,
                    Mathf.Cos(directionFacing * Mathf.Deg2Rad)
                    );

                //Apply the force
                rb.AddForce(thrustForceVector * thrustMagnitude, ForceMode.Acceleration);
            }
        }
        else
        {
            setAverageTriggerValue(0);
        }
    }

    //Sets the average trigger value for other scripts
    private void setAverageTriggerValue(float value)
    {
        averageTriggerValueForOtherScripts = value;
    }

    //Returns the average trigger value for other scripts
    public float getAverageTriggerValue()
    {
        return averageTriggerValueForOtherScripts;
    }
}
