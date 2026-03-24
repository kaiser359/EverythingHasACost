using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AssignBloodBags : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private Image image; 
    private TextMeshProUGUI text;
    public GameObject bloodBag;
    private BloodBagData bagData;
    public GameObject zoomedBag;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        bloodBag = gS.BloodBagOptions[Random.Range(0, gS.BloodBagOptions.Length)];
        bagData = bloodBag.GetComponent<BloodBagData>();
        image = GetComponentInChildren<Image>();
        text.text = "$" + bagData.BloodBagPrice;
        image.sprite = bagData.BloodBagSprite;
    }

    public void Interact()
    {
        zoomedBag.SetActive(true);
        zoomedBag.GetComponent<PurchaseBloodBag>().thisBag = bloodBag;
    }
}
