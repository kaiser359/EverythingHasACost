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
            if (gS.Money.money > 50)
            {
                gS.Money.money = Mathf.FloorToInt(gS.Money.money * (1 - gS.levelTax));
                if (gS.Money.money < 50)
                    gS.Money.money = 50;
            }
            gS.Money.bankMoney = Mathf.FloorToInt(gS.Money.bankMoney * (1 + gS.bankRate));
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
