using UnityEngine;
using UnityEngine.UI;

public class TakeBloodBag : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    public GameObject thisBag;
    public Image image;
    public GameObject SP;
    public GameObject SelectedBag;
    public int bagNum;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
    }
    // Update is called once per frame after the MonoBehaviour is created
    void Update()
    {
        if (thisBag != null)
        {
            var bagData = thisBag.GetComponent<BloodBagData>();
            image.sprite = bagData.BloodBagSprite;
        }
    }
    public void Interact()
    {
        SP.SetActive(true);
        var bagsToReplace = SP.GetComponentsInChildren<ChooseBagReplace>();
        for (int i = 0; i < bagsToReplace.Length; i++)
        {
            bagsToReplace[i].BloodBag = thisBag;
            bagsToReplace[i].bagNum = bagNum;
        }
        gameObject.SetActive(false);
    }
}
