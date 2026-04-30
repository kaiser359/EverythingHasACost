using UnityEngine;
using UnityEngine.UI;
public class AbilityTrigger : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private GameObject BloodBag1; // Assign your ability effect prefab in the Inspector
    private GameObject BloodBag2; // Assign your ability effect prefab in the Inspector
    private GameObject BloodBag3; // Assign your ability effect prefab in the Inspector
    private GameObject BagToTrigger1;
    private GameObject BagToTrigger2;
    private GameObject BagToTrigger3;
    public GameObject BagCooldown1;
    public GameObject BagCooldown2;
    public GameObject BagCooldown3;
    private float bloodTimer1;
    private float bloodTimer2; 
    private float bloodTimer3;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
    }
    void Update()
    {

        if (BloodBag1 != gS.BloodBag1)
        {
            Destroy(BagToTrigger1);
            BloodBag1 = gS.BloodBag1;
            BagToTrigger1 = Instantiate(BloodBag1, FindFirstObjectByType<MainGun>().transform.position, Quaternion.identity, FindFirstObjectByType<MainGun>().transform);
        }
        if (BloodBag2 != gS.BloodBag2)
        {
            Destroy(BagToTrigger2);
            BloodBag2 = gS.BloodBag2;
            BagToTrigger2 = Instantiate(BloodBag2, FindFirstObjectByType<MainGun>().transform.position, Quaternion.identity, FindFirstObjectByType<MainGun>().transform);

        }
        if (BloodBag3 != gS.BloodBag3)
        {
            Destroy(BagToTrigger3);
            BloodBag3 = gS.BloodBag3;
            BagToTrigger3 = Instantiate(BloodBag3, FindFirstObjectByType<MainGun>().transform.position, Quaternion.identity, FindFirstObjectByType<MainGun>().transform);

        }
        bloodTimer1 -= Time.deltaTime;
        bloodTimer2 -= Time.deltaTime;
        bloodTimer3 -= Time.deltaTime;
        BagCooldown1.GetComponent<Image>().fillAmount = bloodTimer1 / BagToTrigger1.GetComponent<BloodBagData>().Cooldown;
        BagCooldown2.GetComponent<Image>().fillAmount = bloodTimer2 / BagToTrigger2.GetComponent<BloodBagData>().Cooldown;
        BagCooldown3.GetComponent<Image>().fillAmount = bloodTimer3 / BagToTrigger3.GetComponent<BloodBagData>().Cooldown;
    }
    // Update is called once per frame
    public void TriggerAbility1()
    {
        Debug.Log("Ability 1 Triggered!");
        if (BloodBag1 != null && bloodTimer1 <= 0)
        {
            
            BagToTrigger1.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
            bloodTimer1 = BagToTrigger1.GetComponent<BloodBagData>().Cooldown;
            
        }
    }
    public void TriggerAbility2()
    {
        Debug.Log("Ability 2 Triggered!");
        if (BloodBag2 != null && bloodTimer2 <= 0)
        {
            BagToTrigger2.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
            bloodTimer2 = BagToTrigger2.GetComponent<BloodBagData>().Cooldown;
        }
    }

    public void TriggerAbility3()
    {
        Debug.Log("Ability 3 Triggered!");
        if (BloodBag3 != null && bloodTimer3 <= 0)
        {
            BagToTrigger3.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
            bloodTimer3 = BagToTrigger3.GetComponent<BloodBagData>().Cooldown;
        }
    }
}
