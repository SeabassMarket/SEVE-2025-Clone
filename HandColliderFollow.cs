using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandColliderFollow : MonoBehaviour
{
    [Header("VR")]
    public InputControl VRInput;
    public GameObject CameraOffset;

    [Header("Head Parent")]
    public GameObject headParent;

    [Header("Right Or Left")]
    public bool right; //True is right, false is left

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        //Set the position to the camera
        transform.position = CameraOffset.transform.position;

        //Add offset
        if (right)
        {
            transform.localPosition += VRInput.getRightControllerPosition();
        }
        else
        {
            transform.localPosition += VRInput.getLeftControllerPosition();
        }
    }
}
