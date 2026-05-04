using UnityEngine;

public class Etointeract : MonoBehaviour
{
    public GameObject interactableObject; // Reference to the interactable object
    public GameObject Einteract;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (interactableObject != null)
        {
            Einteract.SetActive(true); // Show the "E to interact" prompt
        }
        else { 
         Einteract.SetActive(false); // Hide the "E to interact" prompt when not near an interactable object
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        interactableObject = collision.gameObject; // Store the reference to the interactable object
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject == interactableObject)
        {
            interactableObject = null; // Clear the reference when exiting the trigger
        }
    }
}
