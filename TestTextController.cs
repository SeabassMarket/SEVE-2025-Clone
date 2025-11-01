using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestTextController : MonoBehaviour
{
    //Create public labels and variables to show up in Unity Inspector
    [Header("Text Objects")]
    public TMP_Text testText;
    public GameObject leftController;
    public GameObject rightController;
    public GameObject head;
    public GameObject rightGameObject;

    [Header("Swim Input Control")]
    public GameObject XRRig;

    //Private variables to use internally
    private SwimInputControl VRSwimInput;
    private Vector3 previousRightPosition;
    private Vector3 previousRightRotationPalm;
    //private float maxVelocity;
    private float timeSinceLastRefresh;

    // Start is called before the first frame update
    void Start()
    {
        VRSwimInput = XRRig.GetComponent<SwimInputControl>();
        if (VRSwimInput.getInitialized())
        {
            previousRightPosition = VRSwimInput.getRightControllerPosition();
            previousRightRotationPalm = VRSwimInput.getRightControllerAssistedRotationPalm();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        if (VRSwimInput.getInitialized())
        {
            testText.text = "";
            /*float currentVelocity = Vector3.Distance(previousRightPosition, VRSwimInput.getRightControllerPosition()) / Time.fixedDeltaTime;
            if (currentVelocity > maxVelocity)
            {
                maxVelocity = currentVelocity;
            }
            testText.text = maxVelocity.ToString() + " units per second\n" + currentVelocity.ToString() + " units per second\n";*/

            //Display text
            testText.text += (rightGameObject.transform.position - rightController.transform.position).ToString() + "\n";
            testText.text += VRSwimInput.getRightControllerAssistedRotationForward().ToString() + "\n";
            testText.text += VRSwimInput.getRightControllerAssistedRotationPalm().ToString() + "\n";

            //Test calculations
            
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
            float distanceBetweenRightParralelLines = (rightControllerOffset2D.x + rightControllerOffset2D.y * Mathf.Tan(-1 * previousRightRotationPalm.x * Mathf.Deg2Rad));
            distanceBetweenRightParralelLines *= Mathf.Cos(-1 * previousRightRotationPalm.x * Mathf.Deg2Rad);

            //Calculate velocity in direction of the plane
            testText.text += (distanceBetweenRightParralelLines / Time.fixedDeltaTime).ToString();

            //Reset last known position of right hand
            previousRightRotationPalm = VRSwimInput.getRightControllerAssistedRotationPalm();
            previousRightPosition = VRSwimInput.getRightControllerPosition();
        }
        else { testText.text = "NOT INITIALIZED"; }
        timeSinceLastRefresh = timeSinceLastRefresh + Time.fixedDeltaTime;
        if (timeSinceLastRefresh > 5)
        {
            timeSinceLastRefresh = 0;
            //maxVelocity = 0;
        }
    }
}
