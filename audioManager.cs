using UnityEngine;

public class UIAudioManager : MonoBehaviour
{
    public AudioSource buttonAudioSource;

    public void PlayButtonClick()
    {
        if (buttonAudioSource != null)
        {
            buttonAudioSource.Play();
        }
    }
}
