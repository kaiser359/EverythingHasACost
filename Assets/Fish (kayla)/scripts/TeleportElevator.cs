using UnityEngine;

public class TeleportElevator : MonoBehaviour
{
    private GameObject elevator;
    public GameObject player;
    public void Tele()
    {
        elevator = GameObject.FindWithTag("Elevator");
        player.GetComponent<Transform>().transform.position = elevator.GetComponent<Transform>().transform.position;

    }
}
