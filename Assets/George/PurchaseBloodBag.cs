using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PurchaseBloodBag : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private GameObject thisBag;
    private Image image;
    private TextMeshProUGUI priceText;
    public GameObject RP;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        thisBag = gS.BloodBagOptions[Random.Range(0, gS.BloodBagOptions.Length)];
        priceText = GetComponentInChildren<TextMeshProUGUI>();
        var bagData = thisBag.GetComponent<BloodBagData>();
        priceText.text = "$" + bagData.BloodBagPrice;
        image.sprite = bagData.BloodBagSprite;
    }
    // Update is called once per frame after the MonoBehaviour is created
    void Update()
    {

    }
    void OnMouseDown()
    {
        var bagData = thisBag.GetComponent<BloodBagData>();
        if (gS.Money.money >= bagData.BloodBagPrice)
        {
            gS.Money.money -= bagData.BloodBagPrice;
            RP.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
