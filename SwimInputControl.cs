using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

//This will allow us to get InputDevice
using UnityEngine.XR;

public class SwimInputControl : MonoBehaviour
{
    //Game Objects to help with 
    [Header("Objects to Assist with Orientation Calculations")]
    public GameObject rightController;
    public GameObject rightGameObjectForward;
    public GameObject rightGameObjectPalm;
    public GameObject leftController;
    public GameObject leftGameObjectForward;
    public GameObject leftGameObjectPalm;

    //Will store whether input devices are connected or not
    private static bool initialized = false;

    //Create variables for right controller input
    private static Vector3 rightControllerPosition;
    private static Quaternion quaternionRightControllerRotation;
    private static float rightTriggerValue;
    private static float rightGripValue;
    private static bool rightPrimary;
    private static bool rightSecondary;

    //Variables for right controller switch algorithm or press algorithm
    private static bool previousRightPrimary;
    private static bool rightPrimarySwitch = false;
    private static bool rightPrimaryPress = false;
    private static bool previousRightSecondary;
    private static bool rightSecondarySwitch = false;
    private static bool rightSecondaryPress = false;

    //Create variables for left controller input
    private static Vector3 leftControllerPosition;
    private static Quaternion quaternionLeftControllerRotation;
    private static float leftTriggerValue;
    private static float leftGripValue;
    private static bool leftPrimary;
    private static bool leftSecondary;
    private static bool menuButton;

    //Variables for left controller switch algorithm or press algorithm
    private static bool previousLeftPrimary;
    private static bool leftPrimarySwitch = false;
    private static bool previousLeftSecondary;
    private static bool leftSecondarySwitch = false;
    private static bool previousMenuButton;
    private static bool menuButtonSwitch = false;

    //Create variables for head input
    private static Vector3 headPosition;
    private static Quaternion quaternionHeadRotation;

    //Creating a List of Input Devices to store our Input Devices in
    List<InputDevice> inputDevices = new List<InputDevice>();

    //Stores time since that input devices have been unitialized
    private float timeUninitialized = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        //We will try to Initialize the InputReader here, but all components may not be loaded
        InitializeInputReader();
    }

    //This will try to initialize the InputReader by getting all the devices and printing them to the debugger.
    void InitializeInputReader()
    {

        InputDevices.GetDevices(inputDevices);

        /*foreach (var inputDevice in inputDevices)
        {
            Debug.Log(inputDevice.name + " " + inputDevice.characteristics);
        }*/
    }

    // Update is called once per frame
    void Update()
    {
        //We should have a total of 3 Input Devices. If it's less, then we try to initialize them again.
        if (inputDevices.Count < 2)
        {
            InitializeInputReader();
            timeUninitialized = timeUninitialized + Time.deltaTime;
            if (timeUninitialized > 1.0f)
            {
                initialized = false;
            }
            
        } else
        {
            //Record characteristics for right controller
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller, inputDevices);
            foreach (var inputDevice in inputDevices)
            {
                inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float temp);
                rightTriggerValue = temp;
                inputDevice.TryGetFeatureValue(CommonUsages.grip, out float temp0);
                rightGripValue = temp0;
                inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 temp1);
                rightControllerPosition = temp1;
                inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion temp2);
                quaternionRightControllerRotation = temp2;
                inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool temp3);
                rightPrimary = temp3;
                inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool temp4);
                rightSecondary = temp4;
            }
            if (previousRightPrimary != rightPrimary && rightPrimary)
            {
                rightPrimaryPress = true;
                if (rightPrimarySwitch)
                {
                    rightPrimarySwitch = false;
                }
                else
                {
                    rightPrimarySwitch = true;
                }
            }
            else
            {
                rightPrimaryPress = false;
            }
            if (previousRightSecondary != rightSecondary && rightSecondary)
            {
                rightSecondaryPress = true;
                if (rightSecondarySwitch)
                {
                    rightSecondarySwitch = false;
                } else
                {
                    rightSecondarySwitch = true;
                }
            }
            else
            {
                rightSecondaryPress = false;
            }

            //Record characteristics for left controller
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller, inputDevices);
            foreach (var inputDevice in inputDevices)
            {
                inputDevice.TryGetFeatureValue(CommonUsages.trigger, out float temp);
                leftTriggerValue = temp;
                inputDevice.TryGetFeatureValue(CommonUsages.grip, out float temp0);
                leftGripValue = temp0;
                inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 temp1);
                leftControllerPosition = temp1;
                inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion temp2);
                quaternionLeftControllerRotation = temp2;
                inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool temp3);
                leftPrimary = temp3;
                inputDevice.TryGetFeatureValue(CommonUsages.secondaryButton, out bool temp4);
                leftSecondary = temp4;
                inputDevice.TryGetFeatureValue(CommonUsages.menuButton, out bool temp5);
                menuButton = temp5;
            }
            if (previousLeftPrimary != leftPrimary && leftPrimary)
            {
                if (leftPrimarySwitch)
                {
                    leftPrimarySwitch = false;
                } else
                {
                    leftPrimarySwitch = true;
                }
            }
            if (previousLeftSecondary != leftSecondary && leftSecondary)
            {
                if (leftSecondarySwitch)
                {
                    leftSecondarySwitch = false;
                } else
                {
                    leftSecondarySwitch = true;
                }
            }
            if (previousMenuButton != menuButton && menuButton)
            {
                if (menuButtonSwitch)
                {
                    menuButtonSwitch = false;
                } else
                {
                    menuButtonSwitch = true;
                }
            }

            //Record characteristics for head
            InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, inputDevices);
            foreach (var inputDevice in inputDevices)
            {
                inputDevice.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 temp);
                headPosition = temp;
                inputDevice.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion temp0);
                quaternionHeadRotation = temp0;
            }

            //Store values again to be used as reference to previous frame
            previousRightPrimary = rightPrimary;
            previousRightSecondary = rightSecondary;
            previousLeftPrimary = leftPrimary;
            previousLeftSecondary = leftSecondary;
            previousMenuButton = menuButton;
            //show that it is initialized and reset unitialized timer
            initialized = true;
            timeUninitialized = 0;
        }
    }

    //Create get functions to get VR Input info with the risk of changing the values

    //General
    //Return whether it the VR is initialized
    public bool getInitialized() { return initialized; }

    //Right Controller
    //Return the position of the right controller in space
    public Vector3 getRightControllerPosition() { return rightControllerPosition; }
    //Return the rotation of the right controller in space in quaternions
    public Quaternion getQuaternionRightControllerRotation() { return quaternionRightControllerRotation; }
    //Return the rotation of the right controller in space in euler angles
    public Vector3 getEulerRightControllerRotation() { return quaternionRightControllerRotation.eulerAngles; }
    //Return the rotation of the right controller in space in euler angle but plam down fingers facing z is (0, 0, 0)
    //WARNING - Doesn't work great
    public Vector3 getPalmDownEulerRightControllerRotation() 
    {
        return new Vector3
            (360 - (getEulerRightControllerRotation().z + 260) % 360,
            (getEulerRightControllerRotation().y + 75) % 360,
            (getEulerRightControllerRotation().x + 15) % 360);
    }
    //Return the rotation of the right controller simplified into only X and Y angles (Z is 0)
    //NOTE: In Euler Angles and is in the direction of "forward" (The fingers)
    public Vector3 getRightControllerAssistedRotationForward()
    {
        Vector3 rightGameObjectOffset = rightGameObjectForward.transform.position - rightController.transform.position;
        return new Vector3
            (Mathf.Atan2(rightGameObjectOffset.y,
                        Mathf.Sqrt(rightGameObjectOffset.x * rightGameObjectOffset.x + rightGameObjectOffset.z * rightGameObjectOffset.z))
                        * Mathf.Rad2Deg * -1,
                        Mathf.Atan2(rightGameObjectOffset.x, rightGameObjectOffset.z) * Mathf.Rad2Deg,
                        0);
    }
    //Return the rotation of the right controller simplified into only X and Y angles (Z is 0)
    //NOTE: In Euler Angles and is in the direction of of the palms
    public Vector3 getRightControllerAssistedRotationPalm()
    {
        Vector3 rightGameObjectOffset = rightGameObjectPalm.transform.position - rightController.transform.position;
        return new Vector3
            (Mathf.Atan2(rightGameObjectOffset.y,
                        Mathf.Sqrt(rightGameObjectOffset.x * rightGameObjectOffset.x + rightGameObjectOffset.z * rightGameObjectOffset.z))
                        * Mathf.Rad2Deg * -1,
                        Mathf.Atan2(rightGameObjectOffset.x, rightGameObjectOffset.z) * Mathf.Rad2Deg,
                        0);
    }
    //Return the value of the right trigger (0.0-1.0)
    public float getRightTriggerValue() { return rightTriggerValue; }
    //Return the value of the right grip (0.0-1.0)
    public float getRightGripValue() { return rightGripValue; }
    //Return whether the right primary button (A) is currently being pressed
    public bool getRightPrimary() { return rightPrimary; }
    //Return whether the right secondary button (B) is currently being pressed
    public bool getRightSecondary() { return rightSecondary; }
    //Return whether the right primary button (A) switch is currently on or off
    public bool getRightPrimarySwitch() { return rightPrimarySwitch; }
    //Return whether the right secondary button (B) switch is currently on or off
    public bool getRightSecondarySwitch() { return rightSecondarySwitch; }
    //Return whether the right primary button (A) has been pressed
    public bool GetRightPrimaryPress()
    {
        return rightPrimaryPress;
    }
    //Return wthere the right secondary button (B) has been pressed
    public bool GetRightSecondaryPress()
    {
        return rightSecondaryPress;
    }

    //Left Controller
    //Return the position of the left controller in space
    public Vector3 getLeftControllerPosition() { return leftControllerPosition; }
    //Return the rotation of the left controller in space in quaternions
    public Quaternion getQuaternionLeftControllerRotation() { return quaternionLeftControllerRotation; }
    //Return the rotation of the left controller in space in euler angles
    public Vector3 getEulerLeftControllerRotation() { return quaternionLeftControllerRotation.eulerAngles; }
    //Return the rotation of the left controller simplified into only X and Y angles (Z is 0)
    //NOTE: In Euler Angles and is in the direction of "forward" (The fingers)
    public Vector3 getLeftControllerAssistedRotationForward()
    {
        Vector3 leftGameObjectOffset = leftGameObjectForward.transform.position - leftController.transform.position;
        return new Vector3
            (Mathf.Atan2(leftGameObjectOffset.y,
                        Mathf.Sqrt(leftGameObjectOffset.x * leftGameObjectOffset.x + leftGameObjectOffset.z * leftGameObjectOffset.z))
                        * Mathf.Rad2Deg * -1,
                        Mathf.Atan2(leftGameObjectOffset.x, leftGameObjectOffset.z) * Mathf.Rad2Deg,
                        0);
    }
    //Return the rotation of the left controller simplified into only X and Y angles (Z is 0)
    //NOTE: In Euler Angles and is in the direction of of the palms
    public Vector3 getLeftControllerAssistedRotationPalm()
    {
        Vector3 leftGameObjectOffset = leftGameObjectPalm.transform.position - leftController.transform.position;
        return new Vector3
            (Mathf.Atan2(leftGameObjectOffset.y,
                        Mathf.Sqrt(leftGameObjectOffset.x * leftGameObjectOffset.x + leftGameObjectOffset.z * leftGameObjectOffset.z))
                        * Mathf.Rad2Deg * -1,
                        Mathf.Atan2(leftGameObjectOffset.x, leftGameObjectOffset.z) * Mathf.Rad2Deg,
                        0);
    }
    //Return the value of the left trigger (0.0-1.0)
    public float getLeftTriggerValue() { return leftTriggerValue; }
    //Return the value of the left grip (0.0-1.0)
    public float getLeftGripValue() { return leftGripValue; }
    //Return whether the left primary button (X) is currently being pressed
    public bool getLeftPrimary() { return leftPrimary; }
    //Return whether the left secondary button (Y) is currently being pressed
    public bool getLeftSecondary() { return leftSecondary; }
    //Return whether the menu button (Three Lines) is currently being pressed
    public bool getMenuButton() { return menuButton; }
    //Return whether the left primary button (X) switch is currently on or off
    public bool getLeftPrimarySwitch() { return leftPrimarySwitch; }
    //Return whether the left secondary button (Y) switch is currently on or off
    public bool getLeftSecondarySwitch() { return leftSecondarySwitch; }
    //Return whether the menu button (Three Lines) switch is currently on or off
    public bool getMenuButtonSwitch() {  return menuButtonSwitch; }

    //Head
    //Return the position of the head in space
    public Vector3 getHeadPosition() { return headPosition; }
    //Return the rotation of the head in space in quaternions
    public Quaternion getQuaternionHeadRotation() { return quaternionHeadRotation; }
    //Return the rotation of the head in space in euler angles
    public Vector3 getEulerHeadRotation() { return quaternionHeadRotation.eulerAngles; }

}
