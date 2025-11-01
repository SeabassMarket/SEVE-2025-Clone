using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class JetSkiReset : MonoBehaviour
{
    [Header("Variables")]
    public Vector3 respawnLocation;
    public bool automaticRespawnLocation;
    public Vector3 respawnOrientation;
    public bool automaticRespawnOrientation;
    public float respawnDelay;

    //Private variavles for time and controller management
    private float lastTime;
    private InputControl VRInput;
    private Rigidbody rb;
    private bool lastX;

    // Start is called before the first frame update
    void Start()
    {
        //Initialize
        lastTime = respawnDelay * -1;
        VRInput = GetComponent<InputControl>();
        rb = GetComponent<Rigidbody>();

        //Initialize automatics if checked
        if(automaticRespawnLocation)
        {
            respawnLocation = rb.position;
        }
        if(automaticRespawnOrientation)
        {
            respawnOrientation = rb.rotation.eulerAngles;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.time > lastTime + respawnDelay && (VRInput.getLeftPrimary() && !lastX))
        {
            //Reset jetski
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.position = respawnLocation;
            rb.rotation = Quaternion.Euler(respawnOrientation);
            //Reinitialize last time
            lastTime = Time.time;
        }
        lastX = VRInput.getLeftPrimary();
    }
}
