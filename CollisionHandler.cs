using UnityEngine;

public class CollisionDetector : MonoBehaviour
{
    [SerializeField] private GameObject loseUI;
    [SerializeField] private AudioSource waveAudio;

    private bool hasLost = false;

    private void Start()
    {
        if (loseUI != null)
        {
            loseUI.SetActive(false);
        }
        else
        {
            Debug.LogWarning("LoseUI is not assigned in the inspector!");
        }

        if (waveAudio == null)
        {
            Debug.LogWarning("WaveAudio is not assigned in the inspector!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasLost) return;

        Debug.Log("Trigger detected with: " + other.gameObject.name);

        if (other.CompareTag("Obstacle"))
        {
            hasLost = true;
            Debug.Log("Hit an obstacle! Showing Lose UI.");

            if (loseUI != null)
            {
                loseUI.SetActive(true);
            }

            if (waveAudio != null)
            {
                waveAudio.Stop();
            }

            Time.timeScale = 0f;
        }
    }
}
