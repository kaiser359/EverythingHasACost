using UnityEngine;
using UnityEngine.UI;

public class PurchaseBloodBag : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    public GameObject thisBag;
    public Image image;
    public GameObject RP;
    public GameObject SelectedBag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
    }
    // Update is called once per frame after the MonoBehaviour is created
    void Update()
    {
        if (thisBag != null) {
            var bagData = thisBag.GetComponent<BloodBagData>();
            image.sprite = bagData.ZoomedBloodBagSprite;
        }
    }
    public void Interact()
    {
        var bagData = thisBag.GetComponent<BloodBagData>();
        if (gS.Money.money >= bagData.BloodBagPrice)
        {
            Destroy(SelectedBag);
            gS.Money.money -= bagData.BloodBagPrice;
            RP.SetActive(true);
            var bagsToReplace = RP.GetComponentsInChildren<ChooseBagReplace>();
            for (int i = 0; i < bagsToReplace.Length; i++)
            {
                bagsToReplace[i].BloodBag = thisBag;
            }
            gameObject.SetActive(false);

        }else{
            FindAnyObjectByType<NEMFade>().fadeDuration = 1f;
        }
    }
}
