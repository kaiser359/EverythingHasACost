using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class ElevatorInteract : MonoBehaviour
{
    public GameObject Elevator;
    public GlobalPlayerInfo gS;

    public bool isTutorial = false;

    [Header("Audio")]
    [SerializeField] private AudioClip elevatorUse;
    [SerializeField] private AudioClip elevatorDing;

    // Update is called once per frame

    void Start()
    {
        gS = FindFirstObjectByType<GlobalPlayerInfo>();
    }

    private void UseElevatorSFX()
    {
        if (elevatorUse == null) return;

        AudioSource audioSource = gameObject.GetComponent<AudioSource>();

        audioSource.PlayOneShot(elevatorUse);
    }

    private void ElevatorFinishSFX()
    {
        if (elevatorDing == null) return;

        AudioSource audioSource = gameObject.GetComponent<AudioSource>();

        audioSource.PlayOneShot(elevatorDing);
    }

    IEnumerator LayerSwitch()
    {
        yield return null;

        LayerTransition transition = GameObject.FindGameObjectWithTag("LayerTransition").GetComponent<LayerTransition>();

        transition.NormalTransitionIn();
        UseElevatorSFX();

        yield return new WaitForSecondsRealtime(transition.transitionDuration); // delay until transition is fully in
        yield return new WaitForSecondsRealtime(0.5f); // extra delay

        // switch ui here
        transition.ElevatorSetActive(true, gS.floor);

        transition.NormalTransitionOut();

        yield return new WaitForSecondsRealtime(transition.transitionDuration); // delay until transition is fully out

        // elevator cutscene here
        transition.ElevatorSwitchFloors(gS.floor);

        // game logic
        FindFirstObjectByType<DungeonController>().RegenerateDungeon();
        transform.position = Vector3.zero;
        if (gS.Money.money > 50)
        {
            gS.Money.money = Mathf.FloorToInt(gS.Money.money * (1 - gS.levelTax));
            if (gS.Money.money < 50)
                gS.Money.money = 50;
        }
        gS.Money.bankMoney = Mathf.FloorToInt(gS.Money.bankMoney * (1 + gS.bankRate));
        gS.floor++;

        Time.timeScale = 0f; // pause game for cutscene

        yield return new WaitForSecondsRealtime(transition.transitionDuration); // delay for cutscene

        transition.NormalTransitionIn();
        UseElevatorSFX();

        yield return new WaitForSecondsRealtime(transition.transitionDuration); // delay until transition is fully in
        yield return new WaitForSecondsRealtime(0.5f); // extra delay

        // switch ui here
        transition.ElevatorSetActive(false, gS.floor);

        transition.NormalTransitionOut();
        ElevatorFinishSFX();

        Time.timeScale = 1f; // unpause game
    }

    public void Interact(InputAction.CallbackContext ctx)
    {
        // only interact once
        if (!ctx.started) return;

        if (Elevator != null)
        {
            // tutorial level just loads the next scene
            if (isTutorial)
            {
                FindAnyObjectByType<SceneTransition>().SwitchScene("Level 1");
                return;
            }

            StartCoroutine(LayerSwitch());
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Elevator"))
        {
            Elevator = collision.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Elevator = null;
    }
}
