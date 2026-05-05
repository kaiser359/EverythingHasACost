using UnityEngine;

public class Buttons : MonoBehaviour
{
    public void SwitchScene(string scene)
    {
        GameObject.FindGameObjectWithTag("Transition").GetComponent<SceneTransition>().SwitchScene(scene);
        //UnityEngine.SceneManagement.SceneManager.LoadScene(scene);
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

    public void CanvasOn(GameObject canvas)
    {
        canvas.SetActive(!canvas.activeSelf);
        Debug.Log("Canvas active: " + canvas.activeSelf);
    }
}
