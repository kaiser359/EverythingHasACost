using UnityEngine;

public class PurchaseBloodBag : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private GameObject thisBag;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        thisBag = gS.BloodBagOptions[Random.Range(0, gS.BloodBagOptions.Length)];
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
