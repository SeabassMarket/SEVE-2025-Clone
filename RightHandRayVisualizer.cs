using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightHandRayVisualizer : MonoBehaviour
{
    public GameObject RightHandRayVisualizerParent;
    public SwimInputControl VRSwimInput;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        RightHandRayVisualizerParent.transform.rotation = Quaternion.Euler(VRSwimInput.getRightControllerAssistedRotationForward());
    }
}
