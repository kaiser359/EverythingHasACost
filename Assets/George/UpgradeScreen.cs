using UnityEngine;

public class UpgradeScreen : MonoBehaviour
{
    public GameObject UpgradeScreenUI;
    public GameObject[] Buttons;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleScreen() { 
        UpgradeScreenUI.SetActive(!UpgradeScreenUI.activeSelf);
        foreach (GameObject button in Buttons)
        {
                button.SetActive(true);
        }
    }
}
