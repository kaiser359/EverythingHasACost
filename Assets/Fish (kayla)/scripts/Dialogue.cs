using System.Collections;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System;
using UnityEngine.UI;
using System.Reflection;

public enum Names
{
    Dimmy = 0,
    Vinny = 1,
    Slim = 2,
    Ivanna = 3,
    Casino_Guy = 4,
    Malveina = 5,
}

[Serializable]
public class Line
{
    public Names name;
    public string text;
}


public class Dialogue : MonoBehaviour
{
    public bool IntroDi;
    public TextMeshProUGUI dialogueText;
    public List<Line> lines;
    public float speed = 0.04f;
    private int index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IntroDi) { 
            startDialogue();
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void startDialogue()
    {
        index = 0;
        GameObject charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[0];
        FindAnyObjectByType<InteractDialogue>().characterClose[(int)lines[0].name].SetActive(true);
        FindAnyObjectByType<InteractDialogue>().animators[(int)lines[0].name].SetBool("talking", true);
        StartCoroutine(Type());
    }
    IEnumerator Type()
    {
        dialogueText.text = "";
        Names currentChar = lines[index].name;
        if ((int)currentChar == 5) {dialogueText.text = "Dutchess Malveina: ";}
        else  {dialogueText.text = currentChar + ": ";}
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
            if ((int)lines[index].name == 5) { dialogueText.text = "Dutchess Malveina: " + lines[index].text; }
            else { dialogueText.text = lines[index].name + ": " + lines[index].text; }
            return;
        }

        if (index < lines.Count - 1)
        {
            int currentChar = (int)lines[index].name;
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].SetBool("talking", false);
            Debug.Log(currentChar + "now listening" + FindAnyObjectByType<InteractDialogue>().animators[currentChar].GetBool("talking"));

            index++;
            currentChar = (int)lines[index].name;
            GameObject charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[currentChar];
            charCloseUp.SetActive(true);
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].SetBool("talking", true);
            Debug.Log(currentChar + "now talking" + FindAnyObjectByType<InteractDialogue>().animators[currentChar].GetBool("talking"));
            StartCoroutine(Type());
        }
        else
        {
            StopAllCoroutines();
            FindAnyObjectByType<InteractDialogue>().LeaveDialogue();
        }
        
    }
}
