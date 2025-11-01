using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class speedBoost : MonoBehaviour
{
    [Header("Variables")]
    public string jetSkiTag;
    public float speedMultiplier;

    [Header("Audio")]
    public float boostSoundStart;
    private AudioSource boostAudio;

    // Start is called before the first frame update
    void Start()
    {
        //Get adder sound
        boostAudio = GetComponent<AudioSource>();
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
            //Physics
            Rigidbody rb = other.gameObject.GetComponent<Rigidbody>();
            rb.velocity = rb.velocity * speedMultiplier;

            //Play audio
            boostAudio.time = boostSoundStart;
            boostAudio.Play();
        }
    }
}
