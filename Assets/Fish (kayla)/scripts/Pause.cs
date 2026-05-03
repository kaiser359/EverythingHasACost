using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    private GameObject store;
    private GameObject bank;

    void Start()
    {
        pauseMenu.SetActive(false);
    }
    void Update()
    {
        /*store = GameObject.FindWithTag("shopui");
        Debug.Log(store);
        bank = GameObject.FindWithTag("bankui");
        Debug.Log(bank);*/

    }
    public void PauseGame()
    {
        if (pauseMenu != null && !pauseMenu.activeSelf && !GameObject.FindWithTag("shopui").activeSelf && !GameObject.FindWithTag("bankui").activeSelf) { 
            Debug.Log("paused"); 
            pauseMenu.SetActive(true); 
            Time.timeScale = 0; }
    }
}
