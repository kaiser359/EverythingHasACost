using UnityEngine;

public class AbilityTrigger : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private GameObject BloodBag1; // Assign your ability effect prefab in the Inspector
    private GameObject BloodBag2; // Assign your ability effect prefab in the Inspector
    private GameObject BloodBag3; // Assign your ability effect prefab in the Inspector
    private GameObject BagToTrigger1;
    private GameObject BagToTrigger2;
    private GameObject BagToTrigger3;
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
    }
    // Update is called once per frame
    public void TriggerAbility1()
    {
        Debug.Log("Ability 1 Triggered!");
        if (BloodBag1 != null)
        {
            
            BagToTrigger1.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
        }
    }
    public void TriggerAbility2()
    {
        Debug.Log("Ability 2 Triggered!");
        if (BloodBag2 != null)
        {
            BagToTrigger2.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
        }
    }

    public void TriggerAbility3()
    {
        Debug.Log("Ability 3 Triggered!");
        if (BloodBag3 != null)
        {
            BagToTrigger3.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
        }
    }
}
