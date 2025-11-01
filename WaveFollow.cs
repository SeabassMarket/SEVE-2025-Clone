using UnityEngine;

public class WaveFollow : MonoBehaviour
{
    public Transform player;  // Assign your XR Rig or surfboard here
    private Vector3 offset;

    void Start()
    {
        offset = transform.position - player.position;
    }

    void LateUpdate()
    {
        Vector3 newPos = player.position + offset;
        newPos.y = transform.position.y; // Lock Y so wave stays flat
        transform.position = newPos;
    }
}
