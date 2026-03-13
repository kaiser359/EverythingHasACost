using TMPro;
using UnityEngine;

public class MoneyBar : MonoBehaviour
{
    public Money money;
    public TextMeshProUGUI text; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Money: "+money.money.ToString();
    }
}
