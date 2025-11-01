using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JetSkiBuoyancy : MonoBehaviour
{

    [Header("Assistive Game Objects")]
    public GameObject topRight;
    public GameObject bottomLeft;

    [Header("Water")]
    public float waterLevel;
    public float waterDensity;
    public float maxBuoyancyForce;

    //Values of the length, width, and area
    private float length;
    private float width;
    private float baseArea;

    //Rigidbody
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {

        //Calculate the length, width, and area of the base of the jetski
        length = Mathf.Abs(topRight.transform.localPosition.z - bottomLeft.transform.localPosition.z);
        width = Mathf.Abs(topRight.transform.localPosition.x - bottomLeft.transform.localPosition.x);
        baseArea = length * width;

        //Get the mass of the jetski
        rb = GetComponent<Rigidbody>();

        //Set the top right and bottom left objects to inactive
        topRight.SetActive(false);
        bottomLeft.SetActive(false);

        //Print values
        /*
        Debug.Log(topRight.transform.localPosition);
        Debug.Log(bottomLeft.transform.localPosition);
        Debug.Log(length);
        Debug.Log(width);
        Debug.Log(baseArea);
        */
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Update is consistently fixed to ensure physics integrity
    void FixedUpdate()
    {
        //Get the depth in the water
        float depthInWater = waterLevel - transform.position.y;
        
        //Only continue adding force if it is underwater
        if(depthInWater > 0)
        {
            //Calculate and apply the upwards force based on the jetski's mass and volume in the water
            float volumeInWater = depthInWater * baseArea;
            Vector3 force = Vector3.up * volumeInWater * waterDensity;
            if(force.magnitude > maxBuoyancyForce)
            {
                //Debug.Log("Buoyancy force limited to max: " + force + " to " + maxBuoyancyForce);
                force = (force / force.magnitude) * maxBuoyancyForce;
            }
            rb.AddForce(force, ForceMode.Force);
            //Debug.Log(depthInWater.ToString() + " " + volumeInWater.ToString() + " " + force.ToString());
        }
    }
}
