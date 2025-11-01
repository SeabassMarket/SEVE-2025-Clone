using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTesting : MonoBehaviour
{

    private Ray ray;
    public GameObject myObject;
    public GameObject rayVisualizer;
    public float maxRayDistance;
    public LayerMask layers;

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

    //Just the method so I don't have to write it twice
    private void MyRayMethod()
    {
        Vector3 offset = myObject.transform.position - transform.position;
        ray = new Ray(transform.position, offset / offset.magnitude);
        rayVisualizer.transform.position = transform.position;
        rayVisualizer.transform.rotation = Quaternion.Euler(RotationInDirectionOfTarget(myObject, this.gameObject));
        float rayVisualizerMagnitude = offset.magnitude;
        if (rayVisualizerMagnitude > maxRayDistance)
        {
            rayVisualizerMagnitude = maxRayDistance;
        }
        rayVisualizer.transform.localScale = new Vector3(rayVisualizer.transform.localScale.x, rayVisualizer.transform.localScale.y, rayVisualizerMagnitude);
        CheckForColliders();
    }
    private void CheckForColliders()
    {
        if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, layers))
        {
            Debug.Log(hit.collider.gameObject.name + " was hit!");
        }
    }

    //Return the rotation so that an origin object faces in the direction of a target object
    //NOTE: In Euler Angles and only uses X and Y (Z is set to 0, not necessary)
    public Vector3 RotationInDirectionOfTarget(GameObject target, GameObject origin)
    {
        Vector3 offset = target.transform.position - origin.transform.position;
        return new Vector3
            (Mathf.Atan2(offset.y,
                        Mathf.Sqrt(offset.x * offset.x + offset.z * offset.z))
                        * Mathf.Rad2Deg * -1,
                        Mathf.Atan2(offset.x, offset.z) * Mathf.Rad2Deg,
                        0);
    }
}
