using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public string[] lines;
    public float speed;
    private int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //dialogueText.text = "";
        //startDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        /*if(InputAction.)
        {
            if (dialogueText.text == lines[index])
            {
                nextLine();
            }
            else
            {
                StopAllCoroutines();
                dialogueText.text = lines[index];
            }
        }*/
    }

    public void startDialogue()
    {
        index = 0;
        StartCoroutine(Type());
    }
    IEnumerator Type()
    {
        foreach (char letter in lines[index].ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(speed);
        }
    }
    public void nextLine()
    {
        Debug.Log("next line");
        if (index < lines.Length - 1)
        {
            index++;
            Debug.Log("line # " + index);
            dialogueText.text = "";
            StartCoroutine(Type());
        }
        else
        {
            StopAllCoroutines();
            FindAnyObjectByType<InteractDialogue>().LeaveDialogue();
        }
    }
}
