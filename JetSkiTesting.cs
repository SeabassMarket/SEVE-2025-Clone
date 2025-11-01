using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class JetSkiTesting : MonoBehaviour
{

    [Header("Timing Control")]
    public float timeBetweenRespawn;

    [Header("Height")]
    public float height;
    public bool shouldSetHeight = false;

    [Header("Launch")]
    public Vector3 launchVelocityVector;
    public bool shouldLaunch = false;

    [Header("Inertia")]
    public Vector3 inertialCoefficient;
    public bool shouldSetInertialCoefficient;

    [Header("Torque")]
    public float timeUntilTorque;
    public Vector3 torque;

    [Header("Angle Testing")]
    public Vector3 startingAngle;
    public bool shouldUseCustomStartAngle;

    //Controls the continuation
    private float lastTime = 0;

    //Rigidbody
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if(timeBetweenRespawn > 0)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.rotation = Quaternion.Euler(-90, 0, 0);
            if (shouldSetHeight)
            {
                rb.position = new Vector3(0, height, 0);
            }
            if (shouldLaunch)
            {
                rb.AddForce(launchVelocityVector, ForceMode.VelocityChange);
            }
            if(shouldSetInertialCoefficient)
            {
                rb.inertiaTensor = inertialCoefficient;
            }
            if(shouldUseCustomStartAngle)
            {
                rb.rotation = Quaternion.Euler(startingAngle);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        if (timeBetweenRespawn > 0)
        {
            if (Time.time >= lastTime + timeBetweenRespawn)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.rotation = Quaternion.Euler(-90, 0, 0);
                if (shouldSetHeight)
                {
                    rb.position = new Vector3(0, height, 0);
                }
                if (shouldLaunch)
                {
                    rb.AddForce(launchVelocityVector, ForceMode.VelocityChange);
                }
                if (shouldSetInertialCoefficient)
                {
                    rb.inertiaTensor = inertialCoefficient;
                }
                if (shouldUseCustomStartAngle)
                {
                    rb.rotation = Quaternion.Euler(startingAngle);
                }
                lastTime = Time.time;
            }
            if (timeUntilTorque >= 0 && Time.time >= lastTime + timeUntilTorque)
            {
                Debug.Log("APPLYING TORQUE: " + torque);
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                rb.AddTorque(torque, ForceMode.Force);
            }
        }
    }
}
