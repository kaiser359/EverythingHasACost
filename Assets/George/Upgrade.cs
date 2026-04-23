using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Upgrade : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    public int BagNum;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        switch (BagNum)
        {
            case 0:
                GetComponent<Image>().sprite = gS.BloodBag1.GetComponent<BloodBagData>().BloodBagSprite;
                if (gS.BloodBag1.GetComponent<BloodBagData>().NextLevelBag != null)
                    gameObject.SetActive(false);
                GetComponentInChildren<TextMeshProUGUI>().text = "$" + gS.BloodBag1.GetComponent<BloodBagData>().BloodBagPrice / 2;
                break;
            case 1:
                GetComponent<Image>().sprite = gS.BloodBag2.GetComponent<BloodBagData>().BloodBagSprite;
                if (gS.BloodBag2.GetComponent<BloodBagData>().NextLevelBag != null)
                    gameObject.SetActive(false);    
                GetComponentInChildren<TextMeshProUGUI>().text = "$" + gS.BloodBag2.GetComponent<BloodBagData>().BloodBagPrice / 2;
                break;
            case 2:
                GetComponent<Image>().sprite = gS.BloodBag3.GetComponent<BloodBagData>().BloodBagSprite;
                if (gS.BloodBag3.GetComponent<BloodBagData>().NextLevelBag != null)
                    gameObject.SetActive(false);
                GetComponentInChildren<TextMeshProUGUI>().text = "$" + gS.BloodBag3.GetComponent<BloodBagData>().BloodBagPrice / 2;
                break;
        }
    }
    public void UpgradeBag() {
        switch (BagNum)
        {
            case 0:
                if (gS.Money.money > gS.BloodBag1.GetComponent<BloodBagData>().BloodBagPrice / 2)
                {
                    gS.Money.money -= gS.BloodBag1.GetComponent<BloodBagData>().BloodBagPrice / 2;
                    gS.BloodBag1 = gS.BloodBag1.GetComponent<BloodBagData>().NextLevelBag;
                }
                break;
            case 1:
                if (gS.Money.money > gS.BloodBag2.GetComponent<BloodBagData>().BloodBagPrice / 2)
                {
                    gS.Money.money -= gS.BloodBag2.GetComponent<BloodBagData>().BloodBagPrice / 2;
                    gS.BloodBag2 = gS.BloodBag2.GetComponent<BloodBagData>().NextLevelBag;
                }
                break;
            case 2:
                if (gS.Money.money > gS.BloodBag3.GetComponent<BloodBagData>().BloodBagPrice / 2)
                {
                    gS.Money.money -= gS.BloodBag3.GetComponent<BloodBagData>().BloodBagPrice / 2;
                    gS.BloodBag3 = gS.BloodBag3.GetComponent<BloodBagData>().NextLevelBag;
                }
                break;
        }
    }
}
