using UnityEngine;

public class PlayButton : MonoBehaviour
{
    public void PlayGame()
    {
        int NextSceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex + 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene(NextSceneIndex);
    }
}
