using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour
{
    public GameObject pauseMenu;
    private GameObject store;
    private GameObject bank;

    private void Start()
    {
        pauseMenu.SetActive(false);
    }
    private void Update()
    {
        store = GameObject.Find("StorePanel");
        bank = GameObject.Find("BankPanel");
        
    }
    public void PauseGame()
    {
        if (pauseMenu != null && !pauseMenu.activeSelf && !store.activeSelf && !bank.activeSelf) { 
            Debug.Log("paused"); 
            pauseMenu.SetActive(true); 
            Time.timeScale = 0; }
    }
}
