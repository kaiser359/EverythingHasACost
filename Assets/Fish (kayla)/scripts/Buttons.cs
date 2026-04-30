using UnityEngine;

public class Buttons : MonoBehaviour
{

    public void SwitchScene(string scene)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.ExitPlaymode();
#endif
    }

    public void PlayClick(AudioClip clip)
    {
        FindAnyObjectByType<AudioSource>().PlayOneShot(clip);
    }

    public void Resume(GameObject pauseMenu)
    {
        Time.timeScale = 1.0f;
        pauseMenu.SetActive(false);
    }
}
