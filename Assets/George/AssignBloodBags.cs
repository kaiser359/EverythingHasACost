using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class AssignBloodBags : MonoBehaviour
{
    public GlobalPlayerInfo gS;
    public Image image;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        image = GetComponentInChildren<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
