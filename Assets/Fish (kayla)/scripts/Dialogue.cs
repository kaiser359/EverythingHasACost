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
    public float speed = 0.04f;
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
        GameObject charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[0];
        FindAnyObjectByType<InteractDialogue>().animators[(int)lines[0].name].SetBool("talking", false);
        /*(charCloseUp.GetComponent<RectTransform>().position = new Vector3(charCloseUp.GetComponent<Transform>().position.x, charCloseUp.GetComponent<RectTransform>().position.y + 30, 0);
        FindAnyObjectByType<InteractDialogue>().characterClose[(int)lines[0].name].GetComponent<Image>().color = Color.white;*/
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
            GameObject charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[currentChar];
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].SetBool("talking", false);
            /*charCloseUp.GetComponent<Image>().color = Color.red;
            charCloseUp.GetComponent<RectTransform>().localScale = new Vector3(0.9f, 0.9f, 0.9f);
            charCloseUp.GetComponent<RectTransform>().position = new Vector3(charCloseUp.GetComponent<Transform>().position.x, charCloseUp.GetComponent<RectTransform>().position.y - 30,0);
            */
            index++;
            currentChar = (int)lines[index].name;
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].SetBool("talking", true);
            /*charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[currentChar];
            charCloseUp.GetComponent<Image>().color = Color.white;
            charCloseUp.GetComponent<RectTransform>().position = new Vector3(charCloseUp.GetComponent<Transform>().position.x, charCloseUp.GetComponent<RectTransform>().position.y + 30, 0);
            charCloseUp.GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);*/
            charCloseUp.SetActive(true);
            StartCoroutine(Type());
        }
        else
        {
            StopAllCoroutines();
            FindAnyObjectByType<InteractDialogue>().LeaveDialogue();
        }
        
    }
}
