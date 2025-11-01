using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetSkiBouncer : MonoBehaviour
{
    [Header("Variables")]
    public string jetSkiTag;
    public float bounceStrength;

    [Header("Audio")]
    public float bounceSoundStart;
    private AudioSource bounceSound;

    // Start is called before the first frame update
    void Start()
    {
        //Get bounce sound
        bounceSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Bounce the jet ski when it enters the collider
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == jetSkiTag)
        {
            //Control physics
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(new Vector3(0, bounceStrength, 0), ForceMode.VelocityChange);

            //Play audio
            bounceSound.time = bounceSoundStart;
            bounceSound.Play();
        }
    }
}
