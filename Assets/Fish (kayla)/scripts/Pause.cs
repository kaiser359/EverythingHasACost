using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
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
        store = GameObject.Find("Store");
        bank = GameObject.Find("Bank");
        Debug.Log("store:" + store + " bank:" + bank);

    }
    public void PauseGame(InputAction.CallbackContext ctx)
    {
        if (ctx.ReadValue<float>() == 1) return;

        if (store.GetComponent<Store>().justLeftStore || bank.GetComponent<Store>().justLeftStore)
        {
            Debug.Log("just left store, not pausing");
            store.GetComponent<Store>().justLeftStore = false; 
            bank.GetComponent<Store>().justLeftStore = false;
            return;
        }
        else if (pauseMenu != null && !pauseMenu.activeSelf && !store.GetComponent<Store>().justLeftStore && !bank.GetComponent<Store>().justLeftStore) 
        { 
            Debug.Log("paused"); 
            pauseMenu.SetActive(true); 
            Time.timeScale = 0; 
        }
    }
}
