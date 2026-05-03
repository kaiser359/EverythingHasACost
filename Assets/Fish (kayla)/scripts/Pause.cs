using System;
using System.Linq;
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
        Debug.Log("justLeftStore: " + FindAnyObjectByType<Store>().justLeftStore);

    }
    public void PauseGame()
    {
        if (pauseMenu != null && !pauseMenu.activeSelf && !FindAnyObjectByType<Store>().justLeftStore) { 
            Debug.Log("paused"); 
            pauseMenu.SetActive(true); 
            Time.timeScale = 0; }
        else { FindAnyObjectByType<Store>().justLeftStore = false; }
    }
}
