using UnityEngine;

public class ExitBank : MonoBehaviour
{
    public GameObject BankUI;
    public void Exitbank()
    {
        BankUI.SetActive(false);
    }
}
