using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingRotationAlgorithm : MonoBehaviour
{
    public GameObject point;
    public GameObject plane;
    public GameObject parralelPlane;
    public GameObject planePoint;

    // Start is called before the first frame update
    void Start()
    {
        //Calculations
        //Find the current offset
        Vector3 offset = point.transform.position - plane.transform.position;
        //Rotate the object by the y axis of the parent object (makes the problem 2D)
        Vector3 newOffset = new Vector3(Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z) * Mathf.Sin((plane.transform.rotation.eulerAngles.y * -1 - (Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg - 90)) * Mathf.Deg2Rad),
            offset.y,
            Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z) * Mathf.Cos((plane.transform.rotation.eulerAngles.y * -1 - (Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg - 90)) * Mathf.Deg2Rad));
        //Now that we have it in 2D, find the distance betweent the parrelel lines
        Vector2 betterOffset = new Vector2(newOffset.z, newOffset.y);
        float distanceBetweenParralelLines = (betterOffset.x + betterOffset.y * Mathf.Tan(-1 * plane.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) * Mathf.Cos(-1 * plane.transform.rotation.eulerAngles.x * Mathf.Deg2Rad);

        //All this is just for demonstration, not calculations
        point.transform.position = plane.transform.position + newOffset;
        plane.transform.rotation = Quaternion.Euler(new Vector3(plane.transform.rotation.eulerAngles.x, 0, 0));
        parralelPlane.transform.position = point.transform.position;
        parralelPlane.transform.rotation = Quaternion.Euler(new Vector3(plane.transform.rotation.eulerAngles.x, 0, 0));
        planePoint.transform.position = plane.transform.position;
        //Debug.Log(distanceBetweenParralelLines);
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Calculations
        //Find the current offset
        Vector3 offset = point.transform.position - plane.transform.position;
        //Rotate the object by the y axis of the parent object (makes the problem 2D)
        Vector3 newOffset = new Vector3(Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z) * Mathf.Sin((plane.transform.rotation.eulerAngles.y * -1 - (Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg - 90)) * Mathf.Deg2Rad),
            offset.y,
            Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z) * Mathf.Cos((plane.transform.rotation.eulerAngles.y * -1 - (Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg - 90)) * Mathf.Deg2Rad));
        //Now that we have it in 2D, find the distance betweent the parrelel lines
        Vector2 betterOffset = new Vector2(newOffset.z, newOffset.y);
        float distanceBetweenParralelLines = (betterOffset.x + betterOffset.y * Mathf.Tan(-1 * plane.transform.rotation.eulerAngles.x * Mathf.Deg2Rad)) * Mathf.Cos(-1 * plane.transform.rotation.eulerAngles.x * Mathf.Deg2Rad);
        parralelPlane.transform.position = point.transform.position;
        //Debug.Log(distanceBetweenParralelLines);
    }
}