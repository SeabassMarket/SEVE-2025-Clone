using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PamphletColliders : MonoBehaviour
{
    [Header("Pamphlet Script")]
    public PamphletGamePicker gamePicker;

    [Header("Tags")]
    public string cursorTag;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == cursorTag)
        {
            if(gameObject.name == "Right Collider")
            {
                gamePicker.IncrementSelection();
            }
            else if(gameObject.name == "Left Collider")
            {
                gamePicker.ReduceSelection();
            }
            else if(gameObject.name == "Play")
            {
                gamePicker.PlaySelection();
            }
            else
            {
                Debug.Log("Name error in pamphlet colliders");
            }
        }
    }
}
