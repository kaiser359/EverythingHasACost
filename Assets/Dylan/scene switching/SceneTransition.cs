using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph.Internal;
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

    private void Awake()
    {
        // find the child objects for the left and right sides of the transition, the progress bar, and the text
        leftSide = transform.Find("left").gameObject;
        rightSide = transform.Find("right").gameObject;
        progress = transform.Find("progress").gameObject;
        text = transform.Find("text").gameObject;

        // if we're transitioning from a previous scene, start the transition coroutine immediately to fade in from black
        if (fromTransition)
        {
            StartCoroutine(CSidesExit());
            StartCoroutine(CTextExit());
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void SwitchScene(string sceneName)
    {
        // start the transition to the next scene
        StartCoroutine(CStart(sceneName));
        StartCoroutine(CSidesEnter());
        StartCoroutine(CTextEnter());
    }

    // start transitioning to the next scene
    IEnumerator CStart(string sceneName)
    {
        UnityEngine.UI.Image progressBar = progress.GetComponent<UnityEngine.UI.Image>();
        progressBar.enabled = true; // Show the progress bar

        text.GetComponent<TextLoop>().enabled = true; // Start the text loop effect

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            // Here you would typically update a UI element's progress bar based on operation.progress.
            progress.GetComponent<UnityEngine.UI.Image>().fillAmount = operation.progress;
            yield return null;
        }
    }

    IEnumerator CSidesEnter()
    {
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / transitionDuration;
            float sinout_t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out sine function for smooth transition

            leftSide.transform.localPosition = Vector3.Lerp(new Vector3(-Screen.width, 0, 0), Vector3.zero, sinout_t);
            rightSide.transform.localPosition = Vector3.Lerp(new Vector3(Screen.width, 0, 0), Vector3.zero, sinout_t);
            progress.transform.localPosition = Vector3.Lerp(new Vector3(0, -20, 0), Vector3.zero, sinout_t);

            yield return null; // Wait until the next frame
        }
    }

    IEnumerator CTextEnter()
    {
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + 2 * transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / (2 * transitionDuration);
            float sinout_t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out sine function for smooth transition

            text.transform.localPosition = Vector3.Lerp(new Vector3(-370, 30, 0), new Vector3(30, 30, 0), sinout_t);

            yield return null; // Wait until the next frame
        }
    }

    // end the transition to the next scene
    IEnumerator CSidesExit()
    {
        // Wait a frame to ensure the scene is fully loaded before starting the fade-in.
        yield return null;

        float startTime = Time.unscaledTime;

        while (Time.unscaledTime < startTime + transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / transitionDuration;
            float sinin_t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease-in sine function for smooth transition

            leftSide.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(-Screen.width, 0, 0), sinin_t);
            rightSide.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(Screen.width, 0, 0), sinin_t);
            progress.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(0, -20, 0), sinin_t);

            yield return null; // Wait until the next frame
        }
    }

    IEnumerator CTextExit()
    {
        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + 2 * transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / (2 * transitionDuration);
            float sinin_t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f); // Ease-in sine function for smooth transition

            text.transform.localPosition = Vector3.Lerp(new Vector3(30, 30, 0), new Vector3(-370, 30, 0), sinin_t);

            yield return null; // Wait until the next frame
        }
    }
}
