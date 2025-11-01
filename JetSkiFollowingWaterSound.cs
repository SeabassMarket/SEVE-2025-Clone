using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetSkiFollowingWaterSound : MonoBehaviour
{
    [Header("Variables")]
    public GameObject jetSki;
    public AudioSource splashSound;
    public AudioClip splashClip;
    public float fallSpeedForSplashSound;
    public float minSplashVolume;
    public float maxSplashVolume;
    public float splashVolumeCoefficient;
    private Rigidbody rb;
    private float waterLevel;
    private bool canPlay = true;

    // Start is called before the first frame update
    void Start()
    {
        rb = jetSki.GetComponent<Rigidbody>();
        waterLevel = jetSki.GetComponent<JetSkiBuoyancy>().waterLevel;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if(rb.position.y < waterLevel)
        {
            transform.position = rb.position;

            if(rb.velocity.y < fallSpeedForSplashSound && canPlay)
            {
                float splashVolume = minSplashVolume + rb.velocity.y * -1 * splashVolumeCoefficient;
                if(splashVolume > maxSplashVolume)
                {
                    splashVolume = maxSplashVolume;
                }
                splashSound.PlayOneShot(splashClip, splashVolume);
                canPlay = false;
            }
            else if (rb.velocity.y > 0)
            {
                canPlay = true;
            }
        }
    }
}
