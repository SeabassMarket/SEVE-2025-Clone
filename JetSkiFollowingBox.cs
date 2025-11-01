using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetSkiFollowingBox : MonoBehaviour
{
    public GameObject jetSki;
    public GameObject fakeWater;
    public GameObject VRcamera;
    public float offset;
    private float waterLevel;

    // Start is called before the first frame update
    void Start()
    {
        waterLevel = jetSki.GetComponent<JetSkiBuoyancy>().waterLevel;
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if(VRcamera.transform.position.y < waterLevel)
        {
            transform.position = new Vector3(
                jetSki.transform.position.x,
                waterLevel + offset,
                jetSki.transform.position.z
                );
            fakeWater.SetActive(true);
        }
        else
        {
            fakeWater.SetActive(false);
        }
    }
}
