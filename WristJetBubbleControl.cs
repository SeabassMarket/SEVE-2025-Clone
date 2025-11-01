using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WristJetBubbleControl : MonoBehaviour
{
    [Header("Wrist Jet Variables")]
    public ParticleSystem rightWristJetBubbles;
    public ParticleSystem leftWristJetBubbles;
    public AudioSource rightBubbleSounds;
    public AudioSource leftBubbleSounds;
    public Animator rightWristJetAnimator;
    public Animator leftWristJetAnimator;

    [Header("Constants")]
    public float boostBubbleTime;
    public float boostBubbleRate;
    public float thrusterBubbleMax;
    public float maxRotatingSpeed;
    public float maxRateOfDecreasingSpeedPercentage;
    public float maxAudioLevel;
    public float maxRateOfDecreasingAudioPercentage;
    private float lastBoostRight;
    private float lastBoostLeft;
    private float rightRotationSpeed;
    private float leftRotationSpeed;
    private float lastRightRotationSpeed;
    private float lastLeftRotationSpeed;
    private float lastRightBubbleSound;
    private float lastLeftBubbleSound;

    // Start is called before the first frame update
    void Start()
    {
        var rightEmmision = rightWristJetBubbles.emission;
        rightEmmision.rateOverTime = 0;
        var leftEmmision = leftWristJetBubbles.emission;
        leftEmmision.rateOverTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        RightWristJet();
        LeftWristJet();
    }

    //Sets the rotation of either the left or right wrist animator
    public void SetRotationSpeedWristJet(bool right, float speed)
    {
        if(right)
        {
            rightRotationSpeed = speed;
        }
        else
        {
            leftRotationSpeed = speed;
        }
    }

    //Sets the time of the last boost
    public void SetLastBoost(bool right)
    {
        if(right)
        {
            lastBoostRight = Time.time;
        }
        else
        {
            lastBoostLeft = Time.time;
        }
    }

    //Fix rotation right
    private void RightWristJet()
    {
        //Rotation
        float newRotationSpeed = rightRotationSpeed * maxRotatingSpeed;
        float newRotationSpeedPercentage = newRotationSpeed / maxRotatingSpeed * 100f;
        float oldRotationSpeedPercentage = lastRightRotationSpeed / maxRotatingSpeed * 100f;
        if (newRotationSpeedPercentage - oldRotationSpeedPercentage < maxRateOfDecreasingSpeedPercentage * Time.deltaTime)
        {
            newRotationSpeedPercentage = oldRotationSpeedPercentage + maxRateOfDecreasingSpeedPercentage * Time.deltaTime;
        }
        newRotationSpeed = (newRotationSpeedPercentage / 100f) * maxRotatingSpeed;
        rightWristJetAnimator.speed = newRotationSpeed;
        lastRightRotationSpeed = newRotationSpeed;

        //Bubbles
        var emmision = rightWristJetBubbles.emission;
        if (Time.time < lastBoostRight + boostBubbleTime)
        {
            emmision.rateOverTime = boostBubbleRate;
        }
        else
        {
            emmision.rateOverTime = rightRotationSpeed * thrusterBubbleMax;
        }

        //Audio
        float newAudioLevel = rightRotationSpeed * maxAudioLevel;
        float newAudioLevelPercentage = newAudioLevel / maxAudioLevel * 100f;
        float oldAudioLevelPercentage = lastRightBubbleSound / maxAudioLevel * 100f;
        if (newAudioLevelPercentage - oldAudioLevelPercentage < maxRateOfDecreasingAudioPercentage * Time.deltaTime)
        {
            newAudioLevelPercentage = oldAudioLevelPercentage + maxRateOfDecreasingAudioPercentage * Time.deltaTime;
        }
        newAudioLevel = (newAudioLevelPercentage / 100f) * maxAudioLevel;
        rightBubbleSounds.volume = newAudioLevel;
        lastRightBubbleSound = newAudioLevel;
    }

    //Fix rotation left
    private void LeftWristJet()
    {
        //Rotation
        float newRotationSpeed = leftRotationSpeed * maxRotatingSpeed;
        float newRotationSpeedPercentage = newRotationSpeed / maxRotatingSpeed * 100f;
        float oldRotationSpeedPercentage = lastLeftRotationSpeed / maxRotatingSpeed * 100f;
        if (newRotationSpeedPercentage - oldRotationSpeedPercentage < maxRateOfDecreasingSpeedPercentage * Time.deltaTime)
        {
            newRotationSpeedPercentage = oldRotationSpeedPercentage + maxRateOfDecreasingSpeedPercentage * Time.deltaTime;
        }
        newRotationSpeed = (newRotationSpeedPercentage / 100f) * maxRotatingSpeed;
        leftWristJetAnimator.speed = newRotationSpeed;
        lastLeftRotationSpeed = newRotationSpeed;

        //Bubbles
        var emmision = leftWristJetBubbles.emission;
        if (Time.time < lastBoostLeft + boostBubbleTime)
        {
            emmision.rateOverTime = boostBubbleRate;
        }
        else
        {
            emmision.rateOverTime = leftRotationSpeed * thrusterBubbleMax;
        }

        //Audio
        float newAudioLevel = leftRotationSpeed * maxAudioLevel;
        float newAudioLevelPercentage = newAudioLevel / maxAudioLevel * 100f;
        float oldAudioLevelPercentage = lastLeftBubbleSound / maxAudioLevel * 100f;
        if (newAudioLevelPercentage - oldAudioLevelPercentage < maxRateOfDecreasingAudioPercentage * Time.deltaTime)
        {
            newAudioLevelPercentage = oldAudioLevelPercentage + maxRateOfDecreasingAudioPercentage * Time.deltaTime;
        }
        newAudioLevel = (newAudioLevelPercentage / 100f) * maxAudioLevel;
        leftBubbleSounds.volume = newAudioLevel;
        lastLeftBubbleSound = newAudioLevel;
    }
}
