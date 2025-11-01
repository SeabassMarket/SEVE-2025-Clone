using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitToBeach : MonoBehaviour
{
    public void LoadBeachScene()
    {
        SceneManager.LoadScene("beach");
    }
}
