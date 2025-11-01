using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CheckpointsJetSkiRacetrack : MonoBehaviour
{
    [Header("Tags")]
    public string checkpointTag;
    public string checkpointColliderTag;
    public string checkpointCoconutTag;
    public string finishLineTag;

    [Header("UI Control")]
    public TMP_Text messageText;
    public float initialMessageLength;
    public float wrongWayMessageLength;
    public float finishTimeMessageLength;

    //Private message
    private string message;

    //Private lists
    private List<GameObject> checkpointColliders = new List<GameObject>();
    private List<GameObject> checkpointCoconuts = new List<GameObject>();

    //Private checkpoint number
    private int checkpointNum = 0;

    //Keeps track of when the player started the race and other times
    private float startTime = 0;
    private float finishTime = 0;
    private float wrongWayMessageTime;

    // Start is called before the first frame update
    void Start()
    {
        //Complete the lists
        int count = 0;
        foreach (Transform child in transform)
        {
            //Check to see if it is a checkpoint or the finishline
            if(child.gameObject.tag == checkpointTag || child.gameObject.tag == finishLineTag)
            {
                //Get the transform so that we can iterate through to find the colliders and coconuts
                Transform checkpointOrFinishLineTransform = child.gameObject.GetComponent<Transform>();
                foreach(Transform child2 in checkpointOrFinishLineTransform)
                {
                    //Add to list if it is a checkpoint collider
                    if(child2.gameObject.tag == checkpointColliderTag)
                    {
                        checkpointColliders.Add(child2.gameObject);
                    }
                    //Otherwise add it if it is a coconut
                    else if(child2.gameObject.tag == checkpointCoconutTag)
                    {
                        checkpointCoconuts.Add(child2.gameObject);
                    }
                    //Otherwise there's an issue with organization
                    else
                    {
                        Debug.Log("Issue - Unknown tag in child2 objects");
                    }
                }
            }
            //Otherwise there is an issue with organization
            else
            {
                Debug.Log("Issue - Unknown tag in child objects");
            }

            //Add to count
            count++;
        }

        //Check to make sure there are equivalent colliders and coconuts as there are children
        if(checkpointColliders.Count != count)
        {
            Debug.Log("Issue with number of colliders: Expected " + count + " but got " + checkpointColliders.Count);
        }
        //If it's all good assign each checkpoint collider an id number in script
        else
        {
            for (int i = 0; i < checkpointColliders.Count; i++)
            {
                checkpointColliders[i].gameObject.GetComponent<CheckpointColliderScript>().setIdNumber(i);
            }
        }
        if(checkpointCoconuts.Count != count)
        {
            Debug.Log("Issue with number of coconuts: Expected " + count + " but got " + checkpointCoconuts.Count);
        }

        //Initialize finish time to be the negative of the message length (so it doesn't appear at the start)
        finishTime = finishTimeMessageLength * -1;

        //Update
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        //Update the coconut indicating which is next
        for(int i = 0; i < checkpointCoconuts.Count; i++)
        {
            if(i == checkpointNum)
            {
                checkpointCoconuts[i].SetActive(true);
            }
            else
            {
                checkpointCoconuts[i].SetActive(false);
            }
        }

        //Control the text
        if (Time.time < initialMessageLength)
        {
            message = "Jet Ski Race!";
        }
        else if (Time.time < finishTime + finishTimeMessageLength && startTime < finishTime)
        {
            float playerTime = (Mathf.Round((finishTime - startTime) * 100f) / 100f);
            string displayedTime = playerTime.ToString("F2");
            message = "Your time was: " + displayedTime + " seconds!";
        }
        else if (Time.time < wrongWayMessageTime + wrongWayMessageLength)
        {
            message = "Wrong Way!";
        }
        else
        {
            message = "";
        }

        //Set the message text
        messageText.text = message;
    }

    //Handles when a jet ski enters a collider
    public void jetSkiEnterCheckpoint(int idNumber)
    {
        if (idNumber == 0)
        {
            startTime = Time.time;
            checkpointNum = 1;
        }
        else if (idNumber == checkpointNum)
        {
            if (checkpointNum >= checkpointColliders.Count - 1)
            {
                finishTime = Time.time;
                checkpointNum = 0;
            }
            else
            {
                checkpointNum++;
            }
        }
        else
        {
            wrongWayMessageTime = Time.time;
        }
    }
}
