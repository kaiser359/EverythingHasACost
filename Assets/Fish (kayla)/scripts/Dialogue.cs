using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;

public enum Names
{
    Dimmy = 0,
    Vinny = 1,
    Slim_Shadow = 2,
    Ivanna = 3,
    Casino_Guy = 4,
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void startDialogue()
    {
        index = 0;
        FindAnyObjectByType<InteractDialogue>().characterClose[(int)lines[0].name].GetComponent<Image>().color = Color.white;
        FindAnyObjectByType<InteractDialogue>().characterClose[(int)lines[0].name].SetActive(true);
        StartCoroutine(Type());
    }
    IEnumerator Type()
    {
        dialogueText.text = "";
        Names currentChar = lines[index].name;
        dialogueText.text = currentChar + ": ";
        foreach (char letter in lines[index].text.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(speed);
        }
    }
    public void nextLine()
    {
        //if current dialogue text < length of line, finish line
        if (dialogueText.text.Length < lines[index].text.Length + lines[index].name.ToString().Length + 2)
        {
            StopAllCoroutines();
            dialogueText.text = lines[index].name + ": " + lines[index].text;
            return;
        }

        if (index < lines.Count - 1)
        {
            int currentChar = (int)lines[index].name;
            FindAnyObjectByType<InteractDialogue>().characterClose[currentChar].GetComponent<Image>().color = Color.red;
            index++;
            currentChar = (int)lines[index].name;

            FindAnyObjectByType<InteractDialogue>().characterClose[currentChar].GetComponent<Image>().color = Color.white;
            FindAnyObjectByType<InteractDialogue>().characterClose[currentChar].SetActive(true);
            StartCoroutine(Type());
        }
        else
        {
            StopAllCoroutines();
            FindAnyObjectByType<InteractDialogue>().LeaveDialogue();
        }
        
    }
}
