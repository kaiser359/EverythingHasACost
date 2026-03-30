using UnityEngine;
using UnityEngine.UI;

public class ChooseBagReplace : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GlobalPlayerInfo gS;
    private Image image;
    public GameObject BloodBag;
    public bool isFirstBag;
    public bool isSecondBag;
    public bool isThirdBag;
    public int bagNum;
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        GameObject chosenBag = null;
        if (isFirstBag)
            chosenBag = gS.BloodBag1;
        else if (isSecondBag)
            chosenBag = gS.BloodBag2;
        else if (isThirdBag)
            chosenBag = gS.BloodBag3;
        image.sprite = chosenBag.GetComponent<BloodBagData>().BloodBagSprite;

    }
    public void ReplaceBag() {
        if (isFirstBag)
            gS.BloodBag1 = BloodBag;
        else if (isSecondBag)
            gS.BloodBag2 = BloodBag;
        else if (isThirdBag)
            gS.BloodBag3 = BloodBag;
        gameObject.transform.parent.gameObject.SetActive(false);
    }
    public void Storage() {
        switch(bagNum) {
            case 0:
                if (isFirstBag)
                    gS.BankBag1 = gS.BloodBag1;
                else if (isSecondBag)
                    gS.BankBag1 = gS.BloodBag2;
                else if (isThirdBag)
                    gS.BankBag1 = gS.BloodBag3;
                break;
            case 1:
                if (isFirstBag)
                    gS.BankBag2 = gS.BloodBag1;
                else if (isSecondBag)
                    gS.BankBag2 = gS.BloodBag2;
                else if (isThirdBag)
                    gS.BankBag2 = gS.BloodBag3;
                break;
            case 2:
                if (isFirstBag)
                    gS.BankBag3 = gS.BloodBag1;
                else if (isSecondBag)
                    gS.BankBag3 = gS.BloodBag2;
                else if (isThirdBag)
                    gS.BankBag3 = gS.BloodBag3;
                break;
        }
        if (isFirstBag)
            gS.BloodBag1 = BloodBag;
        else if (isSecondBag)
            gS.BloodBag2 = BloodBag;
        else if (isThirdBag)
            gS.BloodBag3 = BloodBag;
        gameObject.transform.parent.gameObject.SetActive(false);
        
    }
}
