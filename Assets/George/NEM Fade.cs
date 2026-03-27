using System.Threading;
using UnityEngine;
using TMPro;

public class NEMFade : MonoBehaviour
{
    public float fadeDuration = 0f;// Duration of the fade effect in seconds
    private TextMeshProUGUI thisText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        thisText = GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        fadeDuration -= Time.unscaledDeltaTime/2;
        thisText.color = new Color(thisText.color.r, thisText.color.g, thisText.color.b, fadeDuration);
    }
}
