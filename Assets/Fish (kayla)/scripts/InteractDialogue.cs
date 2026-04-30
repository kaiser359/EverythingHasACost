using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InteractDialogue : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private GameObject NPC;
    public GameObject background;
    public GameObject dialoguePanel;
    public GameObject[] characterClose;
    public Animator[] animators;
    void Start()
    {
        // make transparent
        Color bgColor = background.GetComponent<Image>().color;
        bgColor.a = 0;
        background.GetComponent<Image>().color = bgColor;

        dialoguePanel.SetActive(false);
        foreach (var character in characterClose)
        {
            character.SetActive(false);
            //character.GetComponent<Image>().color = Color.red;
        }
    }

    public void StartDialogue(GameObject startNPC = null)
    {
        // initiate dialog without being triggered by collision, used for intro dialogue and cutscenes
        if (startNPC != null)
        {
            NPC = startNPC;
        }

        Debug.Log("Interacting with NPC");
        if (!NPC.GetComponent<Dialogue>().dialogueActive)
        {
            Debug.Log("Starting dialogue");
            ActivatePanel();
        }
        //dialoguePanel.SetActive(true);
        Time.timeScale = 0f;
        NPC.GetComponent<Dialogue>().startDialogue();

    }

    // activate and deactivate panel wrappers
    public void ActivatePanel()
    {
        StartCoroutine(CActivatePanel());
    }

    public void DeactivatePanel()
    {
        StartCoroutine(CDeactivatePanel());
    }

    IEnumerator CActivatePanel()
    {
        // delay cuz apparently doing it on startup is Bad
        yield return new WaitForSecondsRealtime(0.1f);

        dialoguePanel.SetActive(true);
        float initialTime = Time.unscaledTime;
        float transitionTime = 0.5f;

        //float originalScale = currentChar.GetComponent<RectTransform>().localScale.x;

        while (Time.unscaledTime < initialTime + transitionTime)
        {
            Debug.Log("nazekara");
            float t = (Time.unscaledTime - initialTime) / transitionTime;

            // move panel up
            dialoguePanel.transform.localPosition = new Vector3(dialoguePanel.transform.localPosition.x, Mathf.Lerp(-1000f, 0, -t * (t - 2)), dialoguePanel.transform.localPosition.z);

            // fade color
            Color color = background.GetComponent<Image>().color;
            color.a = Mathf.Lerp(0, 0.3f, -t * (t - 2));
            background.GetComponent<Image>().color = color;
            yield return null;
        }
    }

    IEnumerator CDeactivatePanel()
    {
        float initialTime = Time.unscaledTime;
        float transitionTime = 0.5f;

        //float originalScale = currentChar.GetComponent<RectTransform>().localScale.x;

        while (Time.unscaledTime < initialTime + transitionTime)
        {
            float t = (Time.unscaledTime - initialTime) / transitionTime;

            // move panel down
            dialoguePanel.transform.localPosition = new Vector3(dialoguePanel.transform.localPosition.x, Mathf.Lerp(0, -1000f, -t * (t - 2)), dialoguePanel.transform.localPosition.z);

            // fade color
            Color color = background.GetComponent<Image>().color;
            color.a = Mathf.Lerp(0.3f, 0, -t * (t - 2));
            background.GetComponent<Image>().color = color;
            yield return null;
        }

        dialoguePanel.SetActive(false);
        foreach (var character in characterClose)
        {
            character.SetActive(false);
            //character.GetComponent<Image>().color = Color.red;
        }
    }

    public void InteractWith(InputAction.CallbackContext ctx)
    {
        if (!ctx.started)
        {
            return;
        }

        Debug.Log("Interacting");
        if (NPC != null)
        {
            StartDialogue();
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
        DeactivatePanel();
        //dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    public void NextLine(InputAction.CallbackContext ctx)
    {
        if (!ctx.started)
        {
            return;
        }

        if (NPC != null)
        {
            NPC.GetComponent<Dialogue>().nextLine();
        }
    }
    public void OnTriggerExit2D(Collider2D collision)
    {
        NPC = null;
    }
}
