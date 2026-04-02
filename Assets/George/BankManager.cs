using UnityEngine;
using TMPro;
public class BankManager : MonoBehaviour
{
    public TextMeshProUGUI AccBalance;
    public TextMeshProUGUI Cash;
    public GlobalPlayerInfo gS;
    public NEMFade NotEnoughCash;
    public GameObject BagStorage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        NotEnoughCash = FindAnyObjectByType<NEMFade>();
    }

    // Update is called once per frame
    void Update()
    {
        AccBalance.text = gS.Money.bankMoney.ToString();    
        Cash.text = gS.Money.money.ToString();
    }
    public void AddMoney()
    {
        if (gS.Money.money < 500){
            NotEnoughCash.fadeDuration = 1f;
        }
        else{
            gS.Money.money -= 500;
            gS.Money.bankMoney += 500;
        }
    }
    public void TakeMoney()
    {
        if (gS.Money.bankMoney < 500){
            NotEnoughCash.fadeDuration = 1f;
        }
        else{
            gS.Money.money += 500;
            gS.Money.bankMoney -= 500;
        }
    }
    public void StoreBags()
    {
        BagStorage.SetActive(true);
    }
}
