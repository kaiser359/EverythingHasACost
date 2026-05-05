using UnityEngine;

public class LevelNum : MonoBehaviour
{
    private TMPro.TextMeshPro text;
    private GlobalPlayerInfo gS;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        text = GetComponent<TMPro.TextMeshPro>();
        gS = FindFirstObjectByType<GlobalPlayerInfo>();
    }

    // Update is called once per frame
    void Update()
    {
        text.text = "Level " + gS.floor;
    }
}
