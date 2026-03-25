using UnityEngine;
using UnityEngine.InputSystem;

public class InteractDialogue : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject NPC;
    public GameObject dialoguePanel;
    void Start()
    {
        dialoguePanel.SetActive(false);
    }
    public void InteractWith()
    {
        Debug.Log("Interacting");
        if (NPC != null)
        {
            Debug.Log("Interacting with NPC");
            dialoguePanel.SetActive(true);
            Time.timeScale = 0f;
            NPC.GetComponent<Dialogue>().startDialogue();
        }
        else { return; }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("NPC"))
        {
            Debug.Log("Collided with NPC");
            NPC = collision.gameObject;
        }
    }
    /*private void OnTriggerExit2D(Collider2D collision)
    {
        LeaveDialogue();
    }*/
    public void LeaveDialogue()
    {
        Debug.Log("Leaving dialogue");
        NPC = null;
        dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    public void NextLine()
    {
        if (NPC != null)
        {
            NPC.GetComponent<Dialogue>().nextLine();
        }
    }
}
