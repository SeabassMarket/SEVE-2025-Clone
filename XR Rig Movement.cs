using System;
using UnityEngine;
using UnityEngine.Audio;

public class XRRigMovement : MonoBehaviour
{
    [Header("XRRig")]
    public GameObject dragObject;
    private Rigidbody rb;

    [Header("Swim Input Control")]
    public SwimInputControl VRSwimInput;

    [Header("Max Speed")]
    public float maxSpeed;

    [Header("Boost")]
    public float velocityMagnitudeToAdd;
    public float boostRecharge;
    public GameObject boostBar;
    public Color boostBarFullColor;
    public Color boostBarNotFullColor;

    [Header("Drag")]
    public float dragForceCoefficient;
    public float minDragForce;

    [Header("Thrust")]
    public float maxThrustForce;
    public float maxSpeedAchievedByThrustForce;
    public float fuelConsumptionAtMax;
    public float constantFuelRecharge;
    public float minimumFuel;
    public GameObject fuelBar;
    public Color fullTankColor;
    public Color emptyTankColor;

    [Header("Swimming")]
    public float swimStrength;
    public float maxSwimStrength;
    public float backHandStrength;
    public float swimVelocityRemover;
    public float swimStrengthDistanceBoosterCoefficient;
    public float minSwimStrengthDistanceBooster;
    public float maxSwimStrengthDistanceBooster;

    [Header("Gravity")]
    public float gravityStrength;

    [Header("Ray and Fish Selection")]
    public GameObject rightHandRayVisualizerParent;
    public GameObject rightRayOrigin;
    public float fishRayDistance;
    public LayerMask fishLayer;
    public Color rayVisualizerTriggerColor;
    public Color rayVisualizerDefaultColor;
    public CenterMessageScript centerMessage;
    private Ray rightFishRay;

    [Header("Audio")]
    public AudioMixer XRRigAudioMixer;
    public float boostAudioStartTime;
    public float boostAudioDuration;
    public AudioSource boostSound;
    public float audioAmpliferCoefficient;
    public float maxAmplifer;

    //Private variables for calculations
    //Previous buttons values
    private bool previousRightPrimary;
    private bool previousLeftPrimary;

    //Previous right orientation and position data
    private Vector3 previousRightPosition;
    private Vector3 previousRightRotationPalm;

    //Previous left orientation and position data
    private Vector3 previousLeftPosition;
    private Vector3 previousLeftRotationPalm;

    //Previous stuff for pressing the secondary buttons
    private bool previousRightSecondary;
    private bool previousLeftSecondary;

    //Record whether it is currently the first frame
    private bool firstFrame;

    //Record the last time a boost was used
    private float lastBoost;

    //The amount of fuel in the thrust tank
    private float fuelPercentage;
    private bool forcedRecharge = true;

    //Private wrist jet bubble control script
    private WristJetBubbleControl bubbleControl;

    // Start is called before the first frame update
    void Start()
    {
        rb = dragObject.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        bubbleControl = GetComponent<WristJetBubbleControl>();
        firstFrame = true;
    }

    // Update is called once per frame
    void Update()
    {

        //Add velocity to the user if one of the velocity buttons is pressed (doesn't run if cooldown hasn't passed)
        AddVelocity();

        //Control the ray that is used to select fish
        RayMethod();

        //Set the drag
        rb.drag = minDragForce + dragForceCoefficient * Vector3.Distance(VRSwimInput.getLeftControllerPosition(), VRSwimInput.getRightControllerPosition());
        
        //Cap the velocity if it's too high
        if(rb.velocity.magnitude > maxSpeed && maxSpeed > 0)
        {
            rb.velocity = (rb.velocity / rb.velocity.magnitude) * maxSpeed;
        }

        //Change the volume of ambient noise  based on the velocity
        float audioAmplifier = 0;
        if(rb.velocity.magnitude != 0)
        {
            audioAmplifier = audioAmpliferCoefficient * Mathf.Log(rb.velocity.magnitude);
        }
        if(audioAmplifier > maxAmplifer)
        {
            audioAmplifier = maxAmplifer;
        }
        XRRigAudioMixer.SetFloat("masterVol", -80 + audioAmplifier);
    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Accelerate the user when the triggers are pressed
        AddAcceleration();

        //Only run if it isn't the first frame (prevents crazy stuff while loading up)
        if (!(firstFrame))
        {
            Swim();
        }
        else
        {
            firstFrame = false;
        }

        //Add gravity
        rb.AddForce(new Vector3(0, gravityStrength, 0), ForceMode.Acceleration);

        //Cap the velocity if it's too high
        if (rb.velocity.magnitude > maxSpeed && maxSpeed > 0)
        {
            rb.velocity = (rb.velocity / rb.velocity.magnitude) * maxSpeed;
        }

        //NOTE: The reason why the following 4 values aren't in swim is so that they run in the first frame as well
        //NOTE CONTINUED: Otherwise it will be glitchy as the position teleports from 0 to the started position
        //Record the previous information about the right hand to use for swim calculations
        previousRightRotationPalm = VRSwimInput.getRightControllerAssistedRotationPalm();
        previousRightPosition = VRSwimInput.getRightControllerPosition();

        //Record the previous information about the left hand to use for swim calculations
        previousLeftRotationPalm = VRSwimInput.getLeftControllerAssistedRotationPalm();
        previousLeftPosition = VRSwimInput.getLeftControllerPosition();
    }

    private void Swim()
    {
        //Right hand swim forces
        //Get the swim velocity (velocity in direction of the palm)
        float rightSwimVelocity = CalculateRightPalmVelocity();
        //Get the base vector velocity using the inverse of the direction of the palm
        Vector3 rightSwimVectorVelocity = VectorDirectionToBaseVectorValues(new Vector3(previousRightRotationPalm.x, previousRightRotationPalm.y, 0));
        //Add force
        AddPalmOrBackForce(rightSwimVelocity, rightSwimVectorVelocity);

        //Left hand swim forces
        //Get the swim velocity (velocity in direction of the palm)
        float leftSwimVelocity = CalculateLeftPalmVelocity();
        //Get the base vector velocity using the inverse of the direction of the palm
        Vector3 leftSwimVectorVelocity = VectorDirectionToBaseVectorValues(new Vector3(previousLeftRotationPalm.x, previousLeftRotationPalm.y, 0));
        //Apply the swim velocity
        AddPalmOrBackForce(leftSwimVelocity, leftSwimVectorVelocity);
    }

    //Adds a force in the opposite direction of hand movement in direction of palm
    //Magnitude of force depends on mainly the velocity and distance between hands (plus other little variables)
    private void AddPalmOrBackForce(float swimVelocity, Vector3 swimVectorVelocity)
    {
        //This value is used to store the swim strength magnitude in all possibilities
        float swimStrengthMagnitude;

        //This value is used to boost strength of swimming depending on the distance between the controllers
        //This 1: makes the player swim breastroke more (how it should be) and 2: prevents the weird swimming of shuffling hands back in forth very close together
        float swimStrengthDistanceBooster;

        //Apply the swim velocity if the velocity is positive
        if (swimVelocity > 0)
        {
            //Remove a little bit of the velocity (this is to prevent small movements from moving the player)
            swimVelocity -= swimVelocityRemover;

            //If the swim velocity is still positve after removal
            if (swimVelocity > 0)
            {
                //Calculate our base swim strength magnitude by taking the square of the velocity times the swim strength
                swimStrengthMagnitude = swimVelocity * swimVelocity * swimStrength;

                //Determine our swim strength booster using the distance between controllers and our coefficient
                swimStrengthDistanceBooster = minSwimStrengthDistanceBooster + swimStrengthDistanceBoosterCoefficient * Vector3.Distance(VRSwimInput.getLeftControllerPosition(), VRSwimInput.getRightControllerPosition());

                //Set to max if it's too big
                if (swimStrengthDistanceBooster > maxSwimStrengthDistanceBooster)
                {
                    swimStrengthDistanceBooster = maxSwimStrengthDistanceBooster;
                }

                //Multiple swim strength magnitude by the booster
                swimStrengthMagnitude *= swimStrengthDistanceBooster;

                //Tame the swim strength magnitude if it is too strong
                if (swimStrengthMagnitude > maxSwimStrength)
                {
                    swimStrengthMagnitude = maxSwimStrength;
                }

                //Add force (we multiply by -1 to add in the opposite direction of movement)
                rb.AddForce(swimVectorVelocity * -1 * swimStrengthMagnitude * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
        //else if the swim velocity was in the negative direction
        else if (swimVelocity < 0)
        {
            //Add the velocity (reduces magnitude/absolute value of the velocity as long as it is > than the swim strength remover)
            swimVelocity += swimVelocityRemover;

            //If the value is still negative (same concept as right above, prevents small movements from moving the player)
            if (swimVelocity < 0)
            {

                //Calculate our base swim strength magnitude by taking the square of the velocity times the swim strength
                swimStrengthMagnitude = swimVelocity * swimVelocity * swimStrength;

                //Determine our swim strength booster using the distance between controllers and our coefficient
                swimStrengthDistanceBooster = minSwimStrengthDistanceBooster + swimStrengthDistanceBoosterCoefficient * Vector3.Distance(VRSwimInput.getLeftControllerPosition(), VRSwimInput.getRightControllerPosition());

                //Set to max if it's too big
                if (swimStrengthDistanceBooster > maxSwimStrengthDistanceBooster)
                {
                    swimStrengthDistanceBooster = maxSwimStrengthDistanceBooster;
                }

                //Multiple swim strength magnitude by the booster
                swimStrengthMagnitude *= swimStrengthDistanceBooster;

                //Tame the swim strength magnitude if it is too strong
                if (swimStrengthMagnitude > maxSwimStrength)
                {
                    swimStrengthMagnitude = maxSwimStrength;
                }

                //Multiply the swim magnitude by the back hand strength (should be a percentage; ex. 0.8 or 80%)
                swimStrengthMagnitude = backHandStrength;

                //Add force (no -1 needed because the swim strength magnitude is always positve due to it being the square of velocity, so it's already opposite)
                rb.AddForce(swimVectorVelocity * swimStrengthMagnitude * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    //Converts a rotation vector3 into a base direction value (between 0 and 1 for each)
    //Only will use the x and y values of the vector3 entered (simplified because for application z is irrelevant)
    public static Vector3 VectorDirectionToBaseVectorValues(Vector3 velocityDirection)
    {
        //Take the originial velocity direction and change it into values in the x, y, and z direction
        //This one is modified for the hands because the way they are implemented and rotated the z becomes the x
        float xVelocity = Mathf.Cos(velocityDirection.x * Mathf.Deg2Rad * -1) * Mathf.Sin(velocityDirection.y * Mathf.Deg2Rad);
        float yVelocity = Mathf.Sin(velocityDirection.x * Mathf.Deg2Rad * -1);
        float zVelocity = Mathf.Cos(velocityDirection.x * Mathf.Deg2Rad * -1) * Mathf.Cos(velocityDirection.y * Mathf.Deg2Rad);
        return new Vector3(xVelocity, yVelocity, zVelocity);
    }

    //Calculate the velocity of the right hand in the palm direction
    private float CalculateRightPalmVelocity()
    {
        /*
           This algorithm works by taking the orientation of the hand in the direction of the palm and creating two
           imaginary parralel planes for each of the two points. The orientation of the parralel plane is the orientation
           of the hand. Then, the program finds the shortest distance between these two planes. It does this by rotating
           the objects about the y-axis until we can view the problem as 2D. Then we create two lines with the slope
           of the x value of the rotation of the palm. We can do this because the planes we just had have now been converted into 2D.
           We find the closest point from the original point to the other point's parralel plane using some trig tricks and that's
           the distance!
        */

        //Rotate the location of the current hand location by the previous hand rotation so that we can look at the problem in 2D
        //The x value here is essentially the new z value after rotation. The y value is the old one
        Vector3 rightControllerOffset = VRSwimInput.getRightControllerPosition() - previousRightPosition;
        float xValue2D = Mathf.Sqrt(rightControllerOffset.x * rightControllerOffset.x + rightControllerOffset.z * rightControllerOffset.z);
        xValue2D *= Mathf.Cos((previousRightRotationPalm.y * -1 - (Mathf.Atan2(rightControllerOffset.z, rightControllerOffset.x) * Mathf.Rad2Deg - 90)) * Mathf.Deg2Rad);
        float yValue2D = rightControllerOffset.y;
        Vector2 rightControllerOffset2D = new Vector2(xValue2D, yValue2D);
        //Take the vector2 just made and use the point to calculate the distance between the parralel lines of the different planes using some trig
        float distanceBetweenRightParallelLines = (rightControllerOffset2D.x + rightControllerOffset2D.y * Mathf.Tan(-1 * previousRightRotationPalm.x * Mathf.Deg2Rad));
        distanceBetweenRightParallelLines *= Mathf.Cos(-1 * previousRightRotationPalm.x * Mathf.Deg2Rad);

        //Calculate velocity in direction of the plane
        return distanceBetweenRightParallelLines / Time.fixedDeltaTime;
    }

    //Calculate the velocity of the left hand in the palm direction
    private float CalculateLeftPalmVelocity()
    {
        //Works the same as the left algorithm

        //Rotate the location of the current hand location by the previous hand rotation so that we can look at the problem in 2D
        //The x value here is essentially the new z value after rotation. The y value is the old one Right
        Vector3 leftControllerOffset = VRSwimInput.getLeftControllerPosition() - previousLeftPosition;
        float xValue2D = Mathf.Sqrt(leftControllerOffset.x * leftControllerOffset.x + leftControllerOffset.z * leftControllerOffset.z);
        xValue2D *= Mathf.Cos((previousLeftRotationPalm.y * -1 - (Mathf.Atan2(leftControllerOffset.z, leftControllerOffset.x) * Mathf.Rad2Deg - 90)) * Mathf.Deg2Rad);
        float yValue2D = leftControllerOffset.y;
        Vector2 leftControllerOffset2D = new Vector2(xValue2D, yValue2D);
        //Take the vector2 just made and use the point to calculate the distance between the parralel lines of the different planes using some trig
        float distanceBetweenLeftParallelLines = (leftControllerOffset2D.x + leftControllerOffset2D.y * Mathf.Tan(-1 * previousLeftRotationPalm.x * Mathf.Deg2Rad));
        distanceBetweenLeftParallelLines *= Mathf.Cos(-1 * previousLeftRotationPalm.x * Mathf.Deg2Rad);

        //Calculate velocity in direction of the plane
        return distanceBetweenLeftParallelLines / Time.fixedDeltaTime;
    }

    //Add velocity to the user if one of the velocity buttons is pressed
    private void AddVelocity()
    {
        //Update the velocity bar appearance and calculate the boost bar percentage
        float boostBarPercentage = 0;
        if(boostRecharge > 0)
        {
            boostBarPercentage = ((Time.time - lastBoost) / boostRecharge) * 100;
        }
        if (boostBarPercentage > 100)
        {
            boostBarPercentage = 100;
        }
        UpdateVelocityBoostBar(boostBarPercentage);

        //Can only add velocity if the recharge is finished
        if(boostBarPercentage >= 100)
        {
            //Add a velocity boost if the right primary button is pressed
            if (!(previousRightPrimary) && VRSwimInput.getRightPrimary())
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                Vector3 velocityDirection = VRSwimInput.getRightControllerAssistedRotationForward();
                //Create the vector with the base ratios of velocities in the 3 directions
                Vector3 vectorVelocity = VectorDirectionToBaseVectorValues(velocityDirection);
                //Add velocity to the object by taking the base ratios and multiplying it by a magnitude
                if (velocityMagnitudeToAdd < 0) { velocityMagnitudeToAdd = velocityMagnitudeToAdd * -1; }
                rb.AddForce(vectorVelocity * velocityMagnitudeToAdd, ForceMode.VelocityChange);
                lastBoost = Time.time;

                //Play boost audio
                boostSound.time = boostAudioStartTime;
                boostSound.Play();
                Invoke("StopBoostAudio", boostAudioDuration);

                //Communicate to wrist jet
                bubbleControl.SetLastBoost(true);
            }

            //Add a velocity boost if the left primary button is pressed
            if (!(previousLeftPrimary) && VRSwimInput.getLeftPrimary())
            {
                rb.constraints = RigidbodyConstraints.FreezeRotation;
                Vector3 velocityDirection = VRSwimInput.getLeftControllerAssistedRotationForward();
                //Create the vector with the base ratios of velocities in the 3 directions
                Vector3 vectorVelocity = VectorDirectionToBaseVectorValues(velocityDirection);
                //Add velocity to the object by taking the base ratios and multiplying it by a magnitude
                if (velocityMagnitudeToAdd < 0) { velocityMagnitudeToAdd = velocityMagnitudeToAdd * -1; }
                rb.AddForce(vectorVelocity * velocityMagnitudeToAdd, ForceMode.VelocityChange);
                lastBoost = Time.time;

                //Play boost audio
                boostSound.time = boostAudioStartTime;
                boostSound.Play();
                Invoke("StopBoostAudio", boostAudioDuration);


                //Communicate to wrist jet
                bubbleControl.SetLastBoost(false);
            }
        }

        //Set previous conditions for future calculations
        previousRightPrimary = VRSwimInput.getRightPrimary();
        previousLeftPrimary = VRSwimInput.getLeftPrimary();
    }

    //Stops the boosts audio after given time
    private void StopBoostAudio()
    {
        boostSound.Stop();
    }

    //Accelerate the user when the triggers are pressed
    private void AddAcceleration()
    {

        //Open the door to accelerating if there's more fuel than the hard minimum
        if(fuelPercentage > 0)
        {
            //Turns off forced recharge if the fuel is above the minimum
            if(fuelPercentage > minimumFuel)
            {
                forcedRecharge = false;
            }
            //Turn on forced recharge as long as forced recharge is not already on and both controllers have been let go of
            //If the user continues holding the triggers they will go until they hit 0
            else if(!forcedRecharge && VRSwimInput.getRightTriggerValue() == 0 && VRSwimInput.getLeftTriggerValue() == 0)
            {
                forcedRecharge = true;
                //Change turbine rotation
                bubbleControl.SetRotationSpeedWristJet(true, 0);
                bubbleControl.SetRotationSpeedWristJet(false, 0);
            }

            if(!forcedRecharge)
            {
                //Accelerate the player in the direction of their right hand
                if (VRSwimInput.getRightTriggerValue() > 0 && VRSwimInput.getLeftTriggerValue() == 0 && rb.velocity.magnitude < maxSpeedAchievedByThrustForce)
                {
                    rb.drag = 0;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    Vector3 accelerationDirection = VRSwimInput.getRightControllerAssistedRotationForward();
                    //Create the vector with the base ratios of velocities in the 3 directions
                    Vector3 vectorAcceleration = VectorDirectionToBaseVectorValues(accelerationDirection);
                    //Add the force the the player
                    rb.AddForce(vectorAcceleration * maxThrustForce * VRSwimInput.getRightTriggerValue(), ForceMode.Acceleration);
                    fuelPercentage -= (Time.fixedDeltaTime * fuelConsumptionAtMax * VRSwimInput.getRightTriggerValue());

                    //Change turbine rotation
                    bubbleControl.SetRotationSpeedWristJet(true, VRSwimInput.getRightTriggerValue());
                }

                //Accelerate the player in the direction of their left hand
                if (VRSwimInput.getLeftTriggerValue() > 0 && VRSwimInput.getRightTriggerValue() == 0 && rb.velocity.magnitude < maxSpeedAchievedByThrustForce)
                {
                    rb.drag = 0;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;
                    Vector3 accelerationDirection = VRSwimInput.getLeftControllerAssistedRotationForward();
                    //Create the vector with the base ratios of velocities in the 3 directions
                    Vector3 vectorAcceleration = VectorDirectionToBaseVectorValues(accelerationDirection);
                    //Add the force the the player
                    rb.AddForce(vectorAcceleration * maxThrustForce * VRSwimInput.getLeftTriggerValue(), ForceMode.Acceleration);
                    fuelPercentage -= (Time.fixedDeltaTime * fuelConsumptionAtMax * VRSwimInput.getLeftTriggerValue());

                    //Change turbine rotation
                    bubbleControl.SetRotationSpeedWristJet(false, VRSwimInput.getLeftTriggerValue());
                }

                //Accelerate the player in the direction of both their hands combined
                if (VRSwimInput.getRightTriggerValue() > 0 && VRSwimInput.getLeftTriggerValue() > 0 && rb.velocity.magnitude < maxSpeedAchievedByThrustForce)
                {
                    rb.drag = 0;
                    rb.constraints = RigidbodyConstraints.FreezeRotation;

                    //Add acceleration for the right hand
                    Vector3 rightAccelerationDirection = VRSwimInput.getRightControllerAssistedRotationForward();
                    //Create the vector with the base ratios of velocities in the 3 directions
                    Vector3 rightVectorAcceleration = VectorDirectionToBaseVectorValues(rightAccelerationDirection);
                    //Add the force the the player
                    rb.AddForce(rightVectorAcceleration * maxThrustForce * 0.5f * VRSwimInput.getRightTriggerValue(), ForceMode.Acceleration);
                    fuelPercentage -= (Time.fixedDeltaTime * fuelConsumptionAtMax * VRSwimInput.getRightTriggerValue() * 0.5f);

                    //Add acceleration for the left hand
                    Vector3 leftAccelerationDirection = VRSwimInput.getLeftControllerAssistedRotationForward();
                    //Create the vector with the base ratios of velocities in the 3 directions
                    Vector3 leftVectorAcceleration = VectorDirectionToBaseVectorValues(leftAccelerationDirection);
                    //Add the force the the player
                    rb.AddForce(leftVectorAcceleration * maxThrustForce * 0.5f * VRSwimInput.getLeftTriggerValue(), ForceMode.Acceleration);
                    fuelPercentage -= (Time.fixedDeltaTime * fuelConsumptionAtMax * VRSwimInput.getLeftTriggerValue() * 0.5f);

                    //Change turbine rotation
                    bubbleControl.SetRotationSpeedWristJet(true, VRSwimInput.getRightTriggerValue());
                    bubbleControl.SetRotationSpeedWristJet(false, VRSwimInput.getLeftTriggerValue());
                }
            }
            //Force recharge if the fuel tank is empty
            else
            {
                forcedRecharge = true;

                //Change turbine rotation
                bubbleControl.SetRotationSpeedWristJet(true, 0);
                bubbleControl.SetRotationSpeedWristJet(false, 0);
            }
        }
        else
        {
            //Change turbine rotation
            bubbleControl.SetRotationSpeedWristJet(true, 0);
            bubbleControl.SetRotationSpeedWristJet(false, 0);
        }

        //Turn on forced recharge if the fuel has run out
        if(fuelPercentage < 0)
        {
            forcedRecharge = true;
            //Change turbine rotation
            bubbleControl.SetRotationSpeedWristJet(true, 0);
            bubbleControl.SetRotationSpeedWristJet(false, 0);
        }

        if(VRSwimInput.getRightTriggerValue() == 0)
        {
            //Change turbine rotation
            bubbleControl.SetRotationSpeedWristJet(true, 0);
        }
        if(VRSwimInput.getLeftTriggerValue() == 0)
        {
            //Change turbine rotation
            bubbleControl.SetRotationSpeedWristJet(false, 0);
        }

        //Add fuel to the tank
        if (constantFuelRecharge > 0)
        {
            fuelPercentage += (Time.fixedDeltaTime * constantFuelRecharge);
        }
        if (fuelPercentage > 100)
        {
            fuelPercentage = 100;
        }

        //Update the bar in the UI
        UpdateAccelerationFuelBar(fuelPercentage);
    }

    //Control the ray and how it is used to select fish (this is called every frame)
    private void RayMethod()
    {
        rightHandRayVisualizerParent.transform.rotation = Quaternion.Euler(VRSwimInput.getRightControllerAssistedRotationForward());
        if(previousRightSecondary != VRSwimInput.getRightSecondary() && VRSwimInput.getRightSecondary())
        {
            rightHandRayVisualizerParent.SetActive(!rightHandRayVisualizerParent.activeSelf);
        }

        if (rightHandRayVisualizerParent.activeSelf)
        {
            rightFishRay = new Ray(rightRayOrigin.transform.position, VectorDirectionToBaseVectorValues(VRSwimInput.getRightControllerAssistedRotationForward()));
            CheckForColliders();
        }

        previousRightSecondary = VRSwimInput.getRightSecondary();
        previousLeftSecondary = VRSwimInput.getLeftSecondary();
    }

    private void CheckForColliders()
    {
        if (Physics.Raycast(rightFishRay, out RaycastHit hit, fishRayDistance))
        {
            rightHandRayVisualizerParent.transform.localScale = new Vector3(rightHandRayVisualizerParent.transform.localScale.x, rightHandRayVisualizerParent.transform.localScale.y, hit.distance);

            if (Physics.Raycast(rightFishRay, out RaycastHit hit2, fishRayDistance, fishLayer))
            {
                rightHandRayVisualizerParent.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = rayVisualizerTriggerColor;
                //Call the function to change the center text
                string boidTag = hit.collider.gameObject.tag;
                if(boidTag.Equals("Blue Tang"))
                {
                    centerMessage.Fish(" blue tang");
                }
                else if(boidTag.Equals("Clownfish"))
                {
                    centerMessage.Fish(" clownfish");
                }
                else if (boidTag.Equals("Angelfish"))
                {
                    centerMessage.Fish("n angelfish");
                }
                else if (boidTag.Equals("Dolphin"))
                {
                    centerMessage.Fish(" dolphin");
                }
                else if (boidTag.Equals("Humu"))
                {
                    centerMessage.Fish(" humu");
                }
                else if (boidTag.Equals("Idolfish"))
                {
                    centerMessage.Fish("n idol fish");
                }
                else if (boidTag.Equals("Ornate"))
                {
                    centerMessage.Fish("n ornate");
                }
                else if (boidTag.Equals("Purple Tang"))
                {
                    centerMessage.Fish(" purple tang");
                }
                else if (boidTag.Equals("Raccoon"))
                {
                    centerMessage.Fish(" raccoon fish");
                }
                else if (boidTag.Equals("Yellow Tang"))
                {
                    centerMessage.Fish(" yellow tang");
                }
                else if (boidTag.Equals("Turtle"))
                {
                    centerMessage.Fish(" turtle");
                }

            }
            else
            {
                rightHandRayVisualizerParent.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = rayVisualizerDefaultColor;
            }
        }
        else
        {
            rightHandRayVisualizerParent.transform.localScale = new Vector3(rightHandRayVisualizerParent.transform.localScale.x, rightHandRayVisualizerParent.transform.localScale.y, fishRayDistance);
            rightHandRayVisualizerParent.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = rayVisualizerDefaultColor;
        }
    }

    //Update the appearance of the velocity boost bar
    private void UpdateVelocityBoostBar(float barPercentage)
    {
        if(barPercentage == 100)
        {
            boostBar.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = boostBarFullColor;
        }
        else
        {
            boostBar.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = boostBarNotFullColor;
        }
        boostBar.transform.localScale = new Vector3(boostBar.transform.localScale.x, barPercentage / 100, boostBar.transform.localScale.z);
    }

    //Update the appearance of the acceleration fuel bar
    private void UpdateAccelerationFuelBar(float barPercentage)
    {
        fuelBar.transform.localScale = new Vector3(boostBar.transform.localScale.x, barPercentage / 100, boostBar.transform.localScale.z);
        fuelBar.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = new Color(fullTankColor.r * barPercentage / 100 + emptyTankColor.r * (100 - barPercentage) / 100,
            fullTankColor.g * barPercentage / 100 + emptyTankColor.g * (100 - barPercentage) / 100,
            fullTankColor.b * barPercentage / 100 + emptyTankColor.b * (100 - barPercentage) / 100,
            fullTankColor.a * barPercentage / 100 + emptyTankColor.a * (100 - barPercentage) / 100);
    }
}
