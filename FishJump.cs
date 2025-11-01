using UnityEngine;

public class DolphinJumpMotion : MonoBehaviour
{
    public float swimSpeed = 3f;
    public float jumpHeight = 2f;
    public float jumpDuration = 1f;
    public float minWait = 2f;
    public float maxWait = 5f;

    public AudioSource splashAudio;

    private Vector3 startPos;
    private float jumpTimer = 0f;
    private bool isJumping = false;
    private float jumpStartTime;

    void Start()
    {
        startPos = transform.position;
        StartCoroutine(ScheduleJump());
    }

    void Update()
    {
        transform.position += Vector3.forward * swimSpeed * Time.deltaTime;

        if (isJumping)
        {
            float elapsed = Time.time - jumpStartTime;
            float progress = elapsed / jumpDuration;

            if (progress >= 1f)
            {
                isJumping = false;
                transform.position = new Vector3(transform.position.x, startPos.y, transform.position.z);

                
                if (splashAudio != null)
                {
                    splashAudio.Play();
                }

                StartCoroutine(ScheduleJump());
            }
            else
            {
                float yOffset = Mathf.Sin(progress * Mathf.PI) * jumpHeight;
                transform.position = new Vector3(transform.position.x, startPos.y + yOffset, transform.position.z);
            }
        }
    }

    System.Collections.IEnumerator ScheduleJump()
    {
        float wait = Random.Range(minWait, maxWait);
        yield return new WaitForSeconds(wait);
        jumpStartTime = Time.time;
        isJumping = true;
    }
}
