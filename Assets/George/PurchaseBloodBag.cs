using UnityEngine;
using TMPro;

public class PurchaseBloodBag : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private GameObject thisBag;
    private SpriteRenderer sR;
    private TextMeshProUGUI priceText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        thisBag = gS.BloodBagOptions[Random.Range(0, gS.BloodBagOptions.Length)];
        priceText = GetComponentInChildren<TextMeshProUGUI>();
        var bagData = thisBag.GetComponent<BloodBagData>();
        priceText.text = "$" + bagData.BloodBagPrice;
    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnMouseDown()
    {
        var bagData = thisBag.GetComponent<BloodBagData>();
        if (gS.Money.money >= bagData.BloodBagPrice)
        {
            gS.Money.money -= bagData.BloodBagPrice;
            Destroy(gameObject);
        }
    }
}
