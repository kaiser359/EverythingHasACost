using UnityEngine;

public class ExitBank : MonoBehaviour
{
    public GameObject BankUI;
    public AudioClip vault;
    public void Exitbank()
    {
        BankUI.SetActive(false);
        FindAnyObjectByType<AudioSource>().PlayOneShot(vault);
    }
}
