using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleBarCollision : MonoBehaviour
{
    [Header("Other Collider Info")]
    public string otherColliderName;

    //Private variables
    private bool isWithinRange = false;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //Sense when it interacts with another collider
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == otherColliderName)
        {
            isWithinRange = true;
        }
    }

    //Sense when it leaves another collider
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == otherColliderName)
        {
            isWithinRange = false;
        }
    }

    //Returns whether the hand is within range of the handlebar
    public bool getIsWithinRange()
    {
        return isWithinRange;
    }
}
