using System.Collections;
using System.Collections.Generic;
using Unity.Serialization.Json;
using Unity.Transforms;
using UnityEngine;

public class JetSkiAudioControl : MonoBehaviour
{
    [Header("Rev Audio")]
    public AudioSource revAudio;
    public float rateOfChangeNeededToTrigger;
    public float valueNeededToRefresh;
    private bool canPlay = true;
    private float previousAverageTriggerValue;

    [Header("Water Audio")]
    public AudioSource waterAudio;
    public float waterAudioCoefficient;
    public float waterExponent;
    public float maxWaterVolume;
    public float timeToSettleWater;
    private float lastTimeInWater;
    private float lastWaterVolume;

    [Header("Engine Hum Audio")]
    public AudioSource engineHumAudio;
    public float maxEngineVolume;
    public float fastestPossibleRateOfDecrease;
    private float previousEngineVolume;

    //VRInput
    private InputControl VRInput;

    //Private scripts
    private JetSkiOrientationControl jSOC;
    private JetSkiThrustControl jSTC;

    //Private water level
    private float waterLevel;

    //Rigidbody
    private Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        VRInput = GetComponent<InputControl>();
        jSOC = GetComponent<JetSkiOrientationControl>();
        jSTC = GetComponent<JetSkiThrustControl>();
        waterLevel = GetComponent<JetSkiBuoyancy>().waterLevel;
        rb = GetComponent<Rigidbody>();
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        //Revving
        RevAudio();

        //Water moving
        WaterAudio();

        //Engine humming
        EngineHummingAudio();
    }

    //For rev audio
    private void RevAudio()
    {
        //Get the average trigger value
        float averageTriggerValue = jSTC.getAverageTriggerValue();

        //Allow the jet ski to rev again if it is the average trigger value is low enough
        if (averageTriggerValue <= valueNeededToRefresh)
        {
            canPlay = true;
        }

        //Check to see if the average trigger value's rate of change is greater than that needed to trigger the sound
        if (canPlay && (averageTriggerValue - previousAverageTriggerValue) >= rateOfChangeNeededToTrigger * Time.deltaTime)
        {
            revAudio.Play();
            canPlay = false;
        }

        //Store previous average trigger value
        previousAverageTriggerValue = averageTriggerValue;
    }

    //For water audio
    private void WaterAudio()
    {
        if(rb.position.y < waterLevel)
        {
            float waterVolume = waterAudioCoefficient * Mathf.Pow(jSOC.getSpeedXZPlane(), waterExponent);
            if(waterVolume > maxWaterVolume)
            {
                waterVolume = maxWaterVolume;
            }
            waterAudio.volume = waterVolume;

            lastTimeInWater = Time.time;
            lastWaterVolume = waterVolume;
        }
        else if(Time.time < lastTimeInWater + timeToSettleWater && timeToSettleWater > 0)
        {
            float percentageVolume = 1 - ((Time.time - lastTimeInWater) / timeToSettleWater);
            waterAudio.volume = percentageVolume * lastWaterVolume;
        }
        else
        {
            waterAudio.volume = 0;
        }
    }

    //For the engine humming
    private void EngineHummingAudio()
    {
        //Calculate the average trigger value
        float averageTriggerValue = jSTC.getAverageTriggerValue();

        //Check to see if the rate of change in percentage of max engine volume is too fast(if so limit it)
        //Calculate the new engine volume and turn it into a percentage
        float newEngineVolume = averageTriggerValue * maxEngineVolume;
        float newEngineVolumePercentage = newEngineVolume / maxEngineVolume * 100f;
        //Get the old engine volume as a percentage
        float oldEngineVolumePercentage = previousEngineVolume / maxEngineVolume * 100f;
        //If the rate of change is too fast down, limit it
        if(newEngineVolumePercentage - oldEngineVolumePercentage < fastestPossibleRateOfDecrease * Time.deltaTime)
        {
            newEngineVolumePercentage = oldEngineVolumePercentage + fastestPossibleRateOfDecrease * Time.deltaTime;
        }
        //Apply new engine volume
        newEngineVolume = (newEngineVolumePercentage / 100f) * maxEngineVolume;
        engineHumAudio.volume = newEngineVolume;
        //Record previous engine volume for next time
        previousEngineVolume = newEngineVolume;
    }
}
