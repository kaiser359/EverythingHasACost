using UnityEngine;

public class Store : MonoBehaviour
{
    public GameObject storePanel; // Reference to the store panel UI
    [SerializeField] private bool tutorialFirstInteract = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void StartStore() { 



        Time.timeScale = 0f; // Pause the game while the store is open
        storePanel.SetActive(true); // Show the store panel when the player interacts with the store

    }
    public void ExitStore() { 
        storePanel.SetActive(false); // Hide the store panel when the player exits the store
        Time.timeScale = 1f; // Resume the game when the store is closed

    }
}
