using UnityEngine;

public class ElevatorInteract : MonoBehaviour
{
    public GameObject Elevator;
    public GlobalPlayerInfo gS;
    // Update is called once per frame
   
    void Start()
    {
        gS = FindFirstObjectByType<GlobalPlayerInfo>();
    }
    public void Interact()
    {
        if(Elevator != null)
        {
            FindFirstObjectByType<DungeonController>().RegenerateDungeon();
            transform.position = Vector3.zero;
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
