using UnityEngine;

public class StartScreenManager : MonoBehaviour
{
    public GameObject startScreenUI;
    public GameObject gameplayElements;
    public SurvivalTimer survivalTimer;

    public void StartGame()
    {
        startScreenUI.SetActive(false);
        gameplayElements.SetActive(true);
        survivalTimer.StartTimer();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
