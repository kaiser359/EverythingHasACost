using UnityEngine;

public class AbilityTrigger : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    private GameObject BloodBag1; // Assign your ability effect prefab in the Inspector
    private GameObject BloodBag2; // Assign your ability effect prefab in the Inspector
    private GameObject BloodBag3; // Assign your ability effect prefab in the Inspector
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
    }
    void Update()
    {
        BloodBag1 = gS.BloodBag1;
        BloodBag2 = gS.BloodBag2;
        BloodBag3 = gS.BloodBag3;
    }
    // Update is called once per frame
    public void TriggerAbility1()
    {
        Debug.Log("Ability 1 Triggered!");
        if (BloodBag1 != null)
        {
            BloodBag1.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
        }
    }
    public void TriggerAbility2()
    {
        Debug.Log("Ability 2 Triggered!");
        if (BloodBag2 != null)
        {
            BloodBag2.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
        }
    }
    public void TriggerAbility3()
    {
        Debug.Log("Ability 3 Triggered!");
        if (BloodBag3 != null)
        {
            BloodBag3.SendMessage("ActivateAbility", SendMessageOptions.DontRequireReceiver);
        }
    }
}
