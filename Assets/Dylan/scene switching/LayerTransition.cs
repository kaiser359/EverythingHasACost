using System.Collections;
using UnityEngine;

public class LayerTransition : MonoBehaviour
{
    public float transitionDuration = 1f;

    private GameObject elevatorUI;
    private GameObject floorText;
    private GameObject leftSide;
    private GameObject rightSide;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // find the child objects for the left and right sides of the transition, the progress bar, and the text
        elevatorUI = transform.GetChild(0).gameObject;
        floorText = transform.GetChild(1).gameObject;
        leftSide = transform.GetChild(2).gameObject;
        rightSide = transform.GetChild(3).gameObject;

        TransitionSetActive(false);
    }

    private void TransitionSetActive(bool enabled)
    {
        elevatorUI.SetActive(enabled);
        floorText.SetActive(enabled);
        leftSide.SetActive(enabled);
        rightSide.SetActive(enabled);
    }

    public void NormalTransitionIn()
    {
        // start the transition to the next scene
        StartCoroutine(CSidesEnter());

        TransitionSetActive(true);
    }

    public void NormalTransitionOut()
    {
        // start the transition to the next scene
        StartCoroutine(CSidesExit());
    }

    public void ElevatorSwitchFloors(int floor)
    {
        // ...
    }

    IEnumerator CElevatorSwitchFloors(int floor)
    {
        // sm abt delay :broken_heart:
        yield return null;

        float startTime = Time.unscaledTime;
        while (Time.unscaledTime < startTime + transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / transitionDuration;
            float sinout_t = Mathf.Sin(t * Mathf.PI * 0.5f); // Ease-out sine function for smooth transition

            leftSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(-Screen.width * 1.5f, 0, 0), Vector3.zero, sinout_t);
            rightSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(Screen.width * 1.5f, 0, 0), Vector3.zero, sinout_t);

            yield return null; // Wait until the next frame
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

            leftSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(-Screen.width*1.5f, 0, 0), Vector3.zero, sinout_t);
            rightSide.GetComponent<RectTransform>().anchoredPosition = Vector3.Lerp(new Vector3(Screen.width*1.5f, 0, 0), Vector3.zero, sinout_t);

            yield return null; // Wait until the next frame
        }

        // ensure the final position is set to prevent any floating point inaccuracies from leaving the sides slightly off-screen
        leftSide.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
        rightSide.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
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

            yield return null; // Wait until the next frame
        }

        // ensure the final position is set to prevent any floating point inaccuracies from leaving the sides slightly off-screen
        leftSide.GetComponent<RectTransform>().anchoredPosition = new Vector3(-Screen.width * 1.5f, 0, 0);
        rightSide.GetComponent<RectTransform>().anchoredPosition = new Vector3(Screen.width * 1.5f, 0, 0);

        // disable everything after the transition is complete to prevent it from interfering with the new scene
        TransitionSetActive(false);
    }
}
