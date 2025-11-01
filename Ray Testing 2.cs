using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTesting2 : MonoBehaviour
{

    public GameObject rayVisualizer;
    public GameObject RayOrigin;
    public GameObject RayOriginParent;
    public LayerMask layers;
    public float maxRayDistance;
    public Color rayVisualizerDefaultColor;
    public Color rayVisualizerTriggerColor;

    private Ray ray;

    // Start is called before the first frame update
    void Start()
    {
        MyRayMethod();
    }

    // Update is called once per frame
    void Update()
    {
        MyRayMethod();
    }

    private void MyRayMethod()
    {
        ray = new Ray(RayOrigin.transform.position, XRRigMovement.VectorDirectionToBaseVectorValues(RayOriginParent.transform.rotation.eulerAngles));
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
        {
            rayVisualizer.transform.position = RayOrigin.transform.position;
            rayVisualizer.transform.localScale = new Vector3(rayVisualizer.transform.localScale.x, rayVisualizer.transform.localScale.y, hit.distance);

            if (Physics.Raycast(ray, out RaycastHit hit2, maxRayDistance, layers))
            {
                rayVisualizer.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = rayVisualizerTriggerColor;
                Debug.Log(hit.collider.gameObject.name + " was hit!");
            }
            else
            {
                rayVisualizer.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = rayVisualizerDefaultColor;
            }
        }
        else
        {
            rayVisualizer.transform.position = RayOrigin.transform.position;
            rayVisualizer.transform.localScale = new Vector3(rayVisualizer.transform.localScale.x, rayVisualizer.transform.localScale.y, maxRayDistance);
            rayVisualizer.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = rayVisualizerDefaultColor;
        }
    }
}
