using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedAdder : MonoBehaviour
{
    [Header("Variables")]
    public string jetSkiTag;
    public float additonalSpeed;

    [Header("Audio")]
    public float adderSoundStart;
    private AudioSource adderAudio;

    // Start is called before the first frame update
    void Start()
    {
        //Get adder sound
        adderAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Add speed to the jet ski when it enters the collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == jetSkiTag)
        {
            //Handle physics
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            Vector2 vectorXZ = new Vector2(rb.velocity.x, rb.velocity.z);
            if(vectorXZ.magnitude > 0)
            {
                vectorXZ = vectorXZ / vectorXZ.magnitude * additonalSpeed;
            }
            else
            {
                float angle = other.gameObject.GetComponent<JetSkiOrientationControl>().getModifiedYRotation();
                vectorXZ = new Vector2(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * additonalSpeed,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * additonalSpeed);
            }
            rb.AddForce(new Vector3(vectorXZ.x, 0, vectorXZ.y), ForceMode.VelocityChange);

            //Play audio
            adderAudio.time = adderSoundStart;
            adderAudio.Play();
        }
    }
}
