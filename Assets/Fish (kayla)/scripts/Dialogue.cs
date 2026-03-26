using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;

public enum Names
{
    Dimmy,
    Vinny,
    Slim_Shadow,
    Ivanna,
}

[Serializable]
public class Line
{
    public Names name;
    public string text;
}

public class Dialogue : MonoBehaviour
{
    public TextMeshProUGUI dialogueText;
    public List<Line> lines;
    public float speed;
    private int index;
    private PlayerInput playerInput;
    //public int character;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInput = FindAnyObjectByType<PlayerInput>();
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
        Debug.Log(lines[index].text);

        foreach (char letter in lines[index].text.ToCharArray())
        {
            Debug.Log(speed);
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(speed);
        }
    }
    public void nextLine()
    {
        //if (playerInput.actions["Next"].WasPerformedThisFrame())
        //{
            //if current dialogue text < length of line, finish line
            Debug.Log("next line");
            if (index < lines.Count - 1)
            {
                index++;
                Debug.Log("line # " + index);
                Names currentChar = lines[index].name;
                dialogueText.text = currentChar + ": ";

                //turn on character close up based on name of current line, set color to white
                if (currentChar == Names.Dimmy)
                {
                    FindAnyObjectByType<InteractDialogue>().characterClose[0].GetComponent<Image>().color = Color.white;
                    FindAnyObjectByType<InteractDialogue>().characterClose[0].SetActive(true);
                }
                else if (currentChar == Names.Vinny)
                {
                    FindAnyObjectByType<InteractDialogue>().characterClose[1].GetComponent<Image>().color = Color.white;
                    FindAnyObjectByType<InteractDialogue>().characterClose[1].SetActive(true);
                }
                else if (currentChar == Names.Slim_Shadow)
                {
                    FindAnyObjectByType<InteractDialogue>().characterClose[2].GetComponent<Image>().color = Color.white;
                    FindAnyObjectByType<InteractDialogue>().characterClose[2].SetActive(true);
                }
                else if (currentChar == Names.Ivanna)
                {
                    FindAnyObjectByType<InteractDialogue>().characterClose[3].GetComponent<Image>().color = Color.white;
                    FindAnyObjectByType<InteractDialogue>().characterClose[3].SetActive(true);
                }
                StartCoroutine(Type());
            }
            else
            {
                StopAllCoroutines();
                FindAnyObjectByType<InteractDialogue>().LeaveDialogue();
            }
        //}
    }
}
