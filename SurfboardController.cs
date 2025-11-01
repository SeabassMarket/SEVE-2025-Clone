using UnityEngine;
using UnityEngine.XR;

public class SurfboardMovement : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float strafeSpeed = 3f;
    public float tiltSensitivity = 1f;
    public float horizontalLimit = 5f;

    private Transform headTransform;

    void Start()
    {
        headTransform = Camera.main.transform;
    }

    void Update()
    {
        
        float joystickInput = Input.GetAxis("Horizontal"); // Works with XR input system too

        
        float roll = headTransform.eulerAngles.z;
        if (roll > 180f) roll -= 360f;
        float headTiltInput = Mathf.Clamp(roll / 30f, -1f, 1f) * tiltSensitivity;

        
        float combinedInput = Mathf.Clamp(joystickInput + headTiltInput, -1f, 1f);

        
        Vector3 movement = Vector3.forward * forwardSpeed + Vector3.right * combinedInput * strafeSpeed;
        transform.Translate(movement * Time.deltaTime, Space.World);

        
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, -horizontalLimit, horizontalLimit);
        transform.position = clampedPos;
    }
}
