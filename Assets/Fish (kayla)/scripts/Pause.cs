using UnityEngine;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;

    private void Start()
    {
        pauseMenu.SetActive(false);
    }
    public void PauseGame()
    {
        if (pauseMenu != null && !pauseMenu.activeSelf) { Debug.Log("paused"); pauseMenu.SetActive(true); Time.timeScale = 0; }
    }
}
