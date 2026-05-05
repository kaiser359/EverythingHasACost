using System.Collections;
using TMPro;
using UnityEngine;

public class LayerTransition : MonoBehaviour
{
    public float transitionDuration = 1f;

    private GameObject elevatorUI;
    private GameObject floorText;
    private GameObject leftSide;
    private GameObject rightSide;
    public AudioClip elevatorDing;
    public AudioClip elevatorDoor;
    private AudioSource audioSource;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // find the child objects for the left and right sides of the transition, the progress bar, and the text
        elevatorUI = transform.GetChild(0).gameObject;
        floorText = transform.GetChild(1).gameObject;
        leftSide = transform.GetChild(2).gameObject;
        rightSide = transform.GetChild(3).gameObject;

        TransitionSetActive(false);
        ElevatorSetActive(false);
    }

    private void TransitionSetActive(bool enabled)
    {
        leftSide.SetActive(enabled);
        rightSide.SetActive(enabled);
    }

    public void ElevatorSetActive(bool enabled, int floor = 0)
    {
        elevatorUI.SetActive(enabled);
        floorText.SetActive(enabled);

        floorText.GetComponent<TMP_Text>().text = floor.ToString().PadLeft(3, '0');
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
        StartCoroutine(CElevatorSwitchFloors(floor));
    }

    IEnumerator CElevatorSwitchFloors(int floor)
    {
        // sm abt delay :broken_heart:
        yield return null;

        float startTime = Time.unscaledTime;

        audioSource.PlayOneShot(elevatorDing);
        //// initial
        //floorText.GetComponent<UnityEngine.UI.Text>().text = floor.ToString().PadLeft(3, '0');

        while (Time.unscaledTime < startTime + transitionDuration)
        {
            float t = (Time.unscaledTime - startTime) / transitionDuration;

            if (0 < t && t < 0.1f) { floorText.SetActive(false); }
            else if (0.1f <= t && t < 0.25f) { floorText.SetActive(true); }
            else if (0.25f <= t && t < 0.4f) { floorText.SetActive(false); }
            else if (0.4f <= t)
            {
                floorText.SetActive(true);

                // final
                floorText.GetComponent<TMP_Text>().text = (floor + 1).ToString().PadLeft(3, '0');
            }

            yield return null; // Wait until the next frame
        }
    }

    IEnumerator CSidesEnter()
    {
        // idk something about delay
        yield return null;

        float startTime = Time.unscaledTime;
        audioSource.PlayOneShot(elevatorDoor);

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
