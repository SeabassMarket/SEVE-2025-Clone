using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointColliderScript : MonoBehaviour
{
    [Header("Checkpoint Parent")]
    public GameObject checkpointParent;

    [Header("Tags")]
    public string jetSkiTag;

    //Script of all the checkpoints
    private CheckpointsJetSkiRacetrack checkpointsParentScript;

    //Private variable for id number
    private int idNumber = -1;

    // Start is called before the first frame update
    void Start()
    {
        //Get the script of all the checkpoints
        checkpointsParentScript = checkpointParent.GetComponent<CheckpointsJetSkiRacetrack>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Trigger when another collider enters
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == jetSkiTag && idNumber >= 0)
        {
            checkpointsParentScript.jetSkiEnterCheckpoint(idNumber);
        }
    }

    //Sets the id numbers
    public void setIdNumber(int idNumber)
    {
        this.idNumber = idNumber;
    }
}
