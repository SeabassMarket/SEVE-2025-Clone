using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Runtime.CompilerServices;

public class CenterMessageScript : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text centerMessageText;
    public float startingMessageTime;
    public float fishTime;

    private float lastFishTime;
    private string message;

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < startingMessageTime)
        {
            message = "Welcome! Click on fish to learn their names!";
        }
        else if (Time.time >= lastFishTime + fishTime)
        {
            message = "";
        }
        centerMessageText.text = message;
    }

    //Set the message based on the type of fish
    public void Fish(string fishType)
    {
        message = "That's a" + fishType + "!";
        lastFishTime = Time.time;
    }
}
