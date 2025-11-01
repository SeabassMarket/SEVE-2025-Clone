using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class VelocityReading : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text velocityReading;

    [Header("XRRig")]
    public GameObject XRRig;
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = XRRig.GetComponent<Rigidbody>();
        velocityReading.text = rb.velocity.magnitude.ToString("F2") + " m/s";
    }

    // Update is called once per frame
    void Update()
    {
        velocityReading.text = rb.velocity.magnitude.ToString("F2") + " m/s";
    }
}
