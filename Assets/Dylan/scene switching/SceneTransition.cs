using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private bool fromTransition;
    [SerializeField] private float transitionDuration = 1f;

    private GameObject leftSide;
    private GameObject rightSide;
    private GameObject progress;
    private GameObject text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // find the child objects for the left and right sides of the transition, the progress bar, and the text
        leftSide = transform.GetChild(0).gameObject;
        rightSide = transform.GetChild(1).gameObject;
        progress = transform.GetChild(2).gameObject;
        text = transform.GetChild(3).gameObject;

        // if we're transitioning from a previous scene, start the transition coroutine immediately to fade in from black
        if (fromTransition)
        {
            StartCoroutine(CSidesExit());
            StartCoroutine(CTextExit());
        }
        else
        { 
            TransitionSetActive(false);
        }
    }

    private void TransitionSetActive(bool enabled)
    {
        leftSide.SetActive(enabled);
        rightSide.SetActive(enabled);
        progress.SetActive(enabled);
        text.SetActive(enabled);
    }

    public void SwitchScene(string sceneName)
    {
        TransitionSetActive(true);

        // start the transition to the next scene
        StartCoroutine(CSidesEnter());
        StartCoroutine(CTextEnter());
        StartCoroutine(CStart(sceneName));
    }

    // start transitioning to the next scene
    IEnumerator CStart(string sceneName)
    {
        // idk something about delay
        yield return null;

        yield return new WaitForSeconds(2*transitionDuration); // Wait for the initial transition to complete

        UnityEngine.UI.Image progressBar = progress.GetComponent<UnityEngine.UI.Image>();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            Debug.Log(operation.progress);

            // Here you would typically update a UI element's progress bar based on operation.progress.
            progress.GetComponent<UnityEngine.UI.Image>().fillAmount = operation.progress;
            yield return null;
        }
    }

    IEnumerator CSidesEnter()
    {
        // idk something about delay
        yield return null;

        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / transitionDuration;
            float sinout_t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out sine function for smooth transition

            leftSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(-Screen.width, 0, 0), Vector3.zero, sinout_t);
            rightSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(Screen.width, 0, 0), Vector3.zero, sinout_t);
            progress.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(0, -30, 0), Vector3.zero, sinout_t);

            yield return null; // Wait until the next frame
        }
    }

    IEnumerator CTextEnter()
    {
        // idk something about delay
        yield return null;

        text.GetComponent<TextLoop>().enabled = true; // Start the text loop effect

        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + 2 * transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / (2 * transitionDuration);
            float sinout_t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out sine function for smooth transition

            text.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(-370, 30, 0), new Vector3(30, 30, 0), sinout_t);

            yield return null; // Wait until the next frame
        }
    }

    // end the transition to the next scene
    IEnumerator CSidesExit()
    {
        // idk something about delay
        yield return null;

        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / transitionDuration;
            float sinin_t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease-in sine function for smooth transition

            leftSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(Vector3.zero, new Vector3(-Screen.width*1.5f, 0, 0), sinin_t);
            rightSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(Vector3.zero, new Vector3(Screen.width*1.5f, 0, 0), sinin_t);
            progress.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(Vector3.zero, new Vector3(0, -30, 0), sinin_t);

            yield return null; // Wait until the next frame
        }
    }

    IEnumerator CTextExit()
    {
        // idk something about delay
        yield return null;

        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + 2 * transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / (2 * transitionDuration);
            float sinin_t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease-in sine function for smooth transition

            text.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(30, 30, 0), new Vector3(-370, 30, 0), sinin_t);

            yield return null; // Wait until the next frame
        }

        // disable everything after the transition is complete to prevent it from interfering with the new scene
        TransitionSetActive(false);
    }
}
