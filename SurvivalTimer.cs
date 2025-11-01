using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class SurvivalTimer : MonoBehaviour
{
    public float timeToSurvive = 30f;
    public TextMeshProUGUI timerText;
    public GameObject victoryUI;

    private float currentTime;
    private bool timerRunning = false;

    public void StartTimer()
    {
        currentTime = timeToSurvive;
        timerRunning = true;
    }

    void Update()
    {
        if (!timerRunning) return;

        currentTime -= Time.deltaTime;
        timerText.text = "Time Left: " + Mathf.Ceil(currentTime).ToString();

        if (currentTime <= 0)
        {
            timerRunning = false;
            ShowVictoryScreen();
        }
    }

    void ShowVictoryScreen()
    {
        if (victoryUI != null)
            victoryUI.SetActive(true);

        Time.timeScale = 0f; 
    }


    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("beach");
    }
}
