using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ElevatorInteract : MonoBehaviour
{
    public GameObject Elevator;
    public GlobalPlayerInfo gS;

    public bool isTutorial = false;

    // Update is called once per frame

    void Start()
    {
        gS = FindFirstObjectByType<GlobalPlayerInfo>();
    }

    IEnumerator LayerSwitch()
    {
        yield return null;

        LayerTransition transition = GameObject.FindGameObjectWithTag("LayerTransition").GetComponent<LayerTransition>();

        transition.NormalTransitionIn();

        yield return new WaitForSecondsRealtime(transition.transitionDuration); // delay until transition is fully in
        yield return new WaitForSecondsRealtime(0.5f); // extra delay

        // switch ui here

        transition.NormalTransitionOut();

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

        yield return new WaitForSecondsRealtime(2*transition.transitionDuration); // delay for cutscene

        transition.NormalTransitionIn();

        yield return new WaitForSecondsRealtime(transition.transitionDuration); // delay until transition is fully in
        yield return new WaitForSecondsRealtime(0.5f); // extra delay

        // switch ui here

        transition.NormalTransitionOut();
    }

    public void Interact()
    {
        if (Elevator != null)
        {
            // tutorial level just loads the next scene
            if (isTutorial)
            {
                FindAnyObjectByType<SceneTransition>().SwitchScene("Level 1");
                return;
            }

            StartCoroutine(LayerSwitch());

            //FindFirstObjectByType<SceneTransition>().NormalTransitionIn();

            //FindFirstObjectByType<DungeonController>().RegenerateDungeon();
            //transform.position = Vector3.zero;
            //if (gS.Money.money > 50)
            //{
            //    gS.Money.money = Mathf.FloorToInt(gS.Money.money * (1 - gS.levelTax));
            //    if (gS.Money.money < 50)
            //        gS.Money.money = 50;
            //}
            //gS.Money.bankMoney = Mathf.FloorToInt(gS.Money.bankMoney * (1 + gS.bankRate));
            //gS.floor++;
            //play elevator cutscene
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
