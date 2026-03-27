using UnityEngine;
using TMPro;
public class BankManager : MonoBehaviour
{
    public TextMeshProUGUI AccBalance;
    public TextMeshProUGUI Cash;
    public GlobalPlayerInfo gS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();        
    }

    // Update is called once per frame
    void Update()
    {
        AccBalance.text = "Account Balance: " + gS.Money.bankMoney;    
        Cash.text = "Cash Available: " + gS.Money.money;
    }
    public void AddMoney()
    {
        gS.Money.money -= 500;
        gS.Money.bankMoney += 500;        
    }
    public void TakeMoney()
    {
        gS.Money.money += 500;
        gS.Money.bankMoney -= 500;
    }
}
