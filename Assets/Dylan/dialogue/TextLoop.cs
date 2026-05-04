using TMPro;
using UnityEngine;

public class TextLoop : MonoBehaviour
{
    private TMP_Text dialogueText;
    //[SerializeField] private float cooldownTime = 0.1f;
    private Cooldown effectCooldown = new(0.2f, true);

    [SerializeField] private string[] textLoop;
    private int currentIndex = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //effectCooldown = new Cooldown(cooldownTime);
        dialogueText = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (effectCooldown.IsReady())
        {
            //Debug.Log("Looping text: " + textLoop[currentIndex]);
            currentIndex = (currentIndex + 1) % textLoop.Length;
            dialogueText.text = textLoop[currentIndex];

            // update mesh to reflect changes to text
            dialogueText.GetComponent<TextWave>().UpdateText();

            // reset cooldown
            effectCooldown.Use();
        }
    }
}
