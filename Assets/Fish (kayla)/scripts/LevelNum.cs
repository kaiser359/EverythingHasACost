using TMPro;
using UnityEngine;

public class LevelNum : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;
    private GlobalPlayerInfo gS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        gS = FindFirstObjectByType<GlobalPlayerInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "";
        text.text = "Level " + gS.floor;
    }
}
