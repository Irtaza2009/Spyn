using UnityEngine;

public class GameManager : MonoBehaviour
{
    bool isPaused = false;
    void togglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            Time.timeScale = 0f;
        }
        else
        {
            Time.timeScale = 1f;
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
