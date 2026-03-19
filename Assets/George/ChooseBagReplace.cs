using UnityEngine;
using UnityEngine.UI;

public class ChooseBagReplace : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GlobalPlayerInfo gS;
    private Image image;
    public bool isFirstBag;
    public bool isSecondBag;
    public bool isThirdBag;
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
    public void ReplaceBag(GameObject BloodBag) {
        if (isFirstBag)
            gS.BloodBag1 = BloodBag;
        else if (isSecondBag)
            gS.BloodBag2 = BloodBag;
        else if (isThirdBag)
            gS.BloodBag3 = BloodBag;
    } 
}
