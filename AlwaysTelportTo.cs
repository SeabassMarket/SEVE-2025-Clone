using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Haha I spelled teleport to wrong
public class AlwaysTelportTo : MonoBehaviour
{
    [Header("Object")]
    public GameObject objectToTeleportTo;

    // Start is called before the first frame update
    void Start()
    {
        Update();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = objectToTeleportTo.transform.position;
    }
}
