using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    bool isPaused = false;
    public GameObject pauseText;
    [SerializeField] private MatchManager matchManager;

    void togglePause()
    {
        if (matchManager.IsRoundOver)
        {
            return;
        }

        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseText.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            pauseText.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            togglePause();
        }
    }
}
