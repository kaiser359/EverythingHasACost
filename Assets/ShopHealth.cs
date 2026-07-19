using UnityEngine;

public class ShopHealth : MonoBehaviour
{
    public Money money;
    public TMPro.TextMeshProUGUI healthText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        healthText.text = money.money.ToString();
    }
}
