using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum Names
{
    Dimmy = 0,
    Vinny = 1,
    Slim = 2,
    Ivanna = 3,
    Casino_Guy = 4,
    Duchess_Malveina = 5,
}

[Serializable]
public class Line
{
    public Names name;
    [TextArea] public string text;
}

[Serializable]
public class DialogueSet
{
    // A serializable wrapper so Unity's inspector can display each dialogue set as an editable element.
    public List<Line> lines;
}

public class Dialogue : MonoBehaviour
{
    public bool IntroDi;
    public TextMeshProUGUI dialogueText;
    //public List<Line> lines;
    public List<DialogueSet> dialogueSets;
    public float speed = 0.04f;
    private int index;

    public bool dialogueActive = false;
    private int dialogSet = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IntroDi)
        {
            // Subscribe so future scene loads trigger dialogue
            SceneManager.sceneLoaded += OnSceneLoaded;

            // If this object is created as part of an already-loaded active scene,
            // the sceneLoaded event has already fired. Manually invoke the handler
            // to ensure the intro dialogue runs.
            if (SceneManager.GetActiveScene().isLoaded)
            {
                Debug.Log("Dialogue.Start: Active scene already loaded; invoking OnSceneLoaded.");
                CStartDialogue();
            }

            //// Also attempt to start immediately once InteractDialogue is available.
            //StartCoroutine(StartWhenInteractDialogueReady());
        }
    }

    public void CStartDialogue()
    {
        StartCoroutine(StartDialogue());
    }

    IEnumerator StartDialogue()
    {
        // delay cuz apparently doing it on startup is Bad
        yield return new WaitForSecondsRealtime(2f);

        GameObject.FindGameObjectWithTag("Player").GetComponent<InteractDialogue>().StartDialogue(gameObject);
        startDialogue();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from the event to prevent memory leaks (keeps original behaviour)
        SceneManager.sceneLoaded -= OnSceneLoaded;

        Debug.Log("Dialogue.OnSceneLoaded: Scene loaded, attempting to start dialogue.");

        CStartDialogue();

        //GameObject.FindGameObjectWithTag("Player").GetComponent<InteractDialogue>().StartDialogue(gameObject);
        //startDialogue();
    }

    void OnDestroy()
    {
        // Ensure we are unsubscribed in case OnSceneLoaded never fired
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void startDialogue()
    {
        if (dialogueActive) {
            nextLine();
            return;
        }

        index = 0;
        List<Line> lines = dialogueSets[dialogSet].lines;

        GameObject charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[0];
        //FindAnyObjectByType<InteractDialogue>().ActivatePanel();
        FindAnyObjectByType<InteractDialogue>().characterClose[(int)lines[0].name].SetActive(true);
        //FindAnyObjectByType<InteractDialogue>().animators[(int)lines[0].name].SetBool("talking", true);
        FindAnyObjectByType<InteractDialogue>().animators[(int)lines[0].name].gameObject.GetComponent<AnimateDialogue>().StartTalking();
        StartCoroutine(Type());

        dialogueActive = true;
    }

    private string FilterLine(Line line)
    {
        string filteredText = "";

        int letterIndex = line.name.ToString().Length + 2;
        bool isReadingTag = false;
        Dictionary<int, char> effectIndicatorLookup = new();
        foreach (char letter in line.text.ToCharArray())
        {
            // filtering tags from the dialogue text so they don't affect the typing effect
            if (isReadingTag)
            {
                switch (letter)
                {
                    case '}':
                        isReadingTag = false;
                        break;
                    default:
                        effectIndicatorLookup[letterIndex] = letter;
                        break;
                }

                continue;
            }
            if (letter == '{')
            {
                Debug.Log("i exist");
                isReadingTag = true;
                continue;
            }

            letterIndex++;
            filteredText += letter;
            dialogueText.gameObject.GetComponent<AnimateDialogueText>().effectIndicatorLookup = effectIndicatorLookup; // constantly updating the text component's reference to the tag indicators so that it can trigger effects at the right time
        }

        //foreach (KeyValuePair<int, char> entry in effectIndicatorLookup)
        //{
        //    Debug.Log("effect at " + entry.Key + ": " + entry.Value);
        //}

        return filteredText;
    }

    IEnumerator Type()
    {
        List<Line> lines = dialogueSets[dialogSet].lines;
        string filteredText = FilterLine(lines[index]);

        dialogueText.text = "";
        Names currentChar = lines[index].name;
        dialogueText.text = currentChar.ToString().Replace('_', ' ') + ": ";

        Debug.Log(filteredText);

        foreach (char letter in filteredText.ToCharArray())
        {
            dialogueText.text += letter;

            // update the text before it renders to the screen
            dialogueText.gameObject.GetComponent<AnimateDialogueText>().UpdateText();

            float delay = speed;
            if (letter == ',')
            {
                delay *= 4; // longer pause for commas
            }
            if (letter == '.' || letter == '!' || letter == '?')
            {
                delay *= 8; // even longer pause for sentence endings
            }


            yield return new WaitForSecondsRealtime(delay);
        }
    }

    public void nextLine()
    {
        List<Line> lines = dialogueSets[dialogSet].lines;
        string filteredText = FilterLine(lines[index]);

        // if current dialogue text < length of line, finish line
        if (dialogueText.text.Length < filteredText.Length + lines[index].name.ToString().Length + 2)
        {
            Debug.Log("finish line");

            StopAllCoroutines();
            dialogueText.text = lines[index].name.ToString().Replace('_', ' ') + ": " + filteredText;
            //if ((int)lines[index].name == 5) { dialogueText.text = "Dutchess Malveina: " + lines[index].text; }
            //else { dialogueText.text = lines[index].name + ": " + lines[index].text; }
            return;
        }

        if (index < lines.Count - 1)
        {
            int currentChar = (int)lines[index].name;
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].gameObject.GetComponent<AnimateDialogue>().StartListening();
            //FindAnyObjectByType<InteractDialogue>().animators[currentChar].SetBool("talking", false);
            Debug.Log(currentChar + "now listening" + FindAnyObjectByType<InteractDialogue>().animators[currentChar].GetBool("talking"));

            index++;
            currentChar = (int)lines[index].name;
            GameObject charCloseUp = FindAnyObjectByType<InteractDialogue>().characterClose[currentChar];
            charCloseUp.SetActive(true);
            //FindAnyObjectByType<InteractDialogue>().animators[currentChar].SetBool("talking", true);
            Debug.Log(currentChar + "now talking" + FindAnyObjectByType<InteractDialogue>().animators[currentChar].GetBool("talking"));
            StartCoroutine(Type());
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].gameObject.GetComponent<AnimateDialogue>().StartTalking();
            FindAnyObjectByType<InteractDialogue>().animators[currentChar].gameObject.GetComponent<AnimateDialogue>().Jump();
        }
        else
        {
            Debug.Log("leave dialogue");

            StopAllCoroutines();
            FindAnyObjectByType<InteractDialogue>().LeaveDialogue();

            dialogSet += 1;
            dialogueActive = false;
            dialogSet = Mathf.Clamp(dialogSet, 0, dialogueSets.Count - 1);
        }
        
    }

}
