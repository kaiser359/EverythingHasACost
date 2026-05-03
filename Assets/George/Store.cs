using System.Collections;
using UnityEngine;

public class Store : MonoBehaviour
{
    public GameObject storePanel; // Reference to the store panel UI

    [SerializeField] private bool tutorialFirstInteract = false;
    private Dialogue dialogue; // reference to dialogue if needed

    public bool justLeftStore = false; //reference for pause menu to know whether to show or not, should be set to false by pause menu once it checks this

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (tutorialFirstInteract)
        {
            dialogue = gameObject.GetComponent<Dialogue>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator WaitToStartStore()
    {
        Debug.Log("Waiting to start store...");

        yield return new WaitForSecondsRealtime(1f);

        while (dialogue.dialogueActive)
        {
            yield return null;
        }

        // delay until panel deactivates
        //yield return new WaitForSecondsRealtime(0.5f);

        Time.timeScale = 0;
        storePanel.SetActive(true);
    }

    public void StartStore() {
        // just dont do anything if dialogue is active, should only be the case for the tutorial store
        if ((tutorialFirstInteract && dialogue.dialogueActive) || storePanel.activeSelf)
        {
            return;
        }

        if (tutorialFirstInteract)
        {
            dialogue.StartDialogue();
            StartCoroutine(WaitToStartStore());
        }
        else
        {
            Time.timeScale = 0f; // Pause the game while the store is open
            storePanel.SetActive(true); // Show the store panel when the player interacts with the store
        }

    }
    public void ExitStore() { 
        Debug.Log("Exiting store...");

        storePanel.SetActive(false); // Hide the store panel when the player exits the store
        Time.timeScale = 1f; // Resume the game when the store is closed
        justLeftStore = true;

        if (tutorialFirstInteract)
        {
            dialogue.StartDialogue();
            tutorialFirstInteract = false;
        }
    }
}
