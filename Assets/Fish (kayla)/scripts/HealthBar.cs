using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    public int health;
    private UnityEngine.UI.Image healthBar;
    private TMPro.TextMeshProUGUI healthText;
    private Animator coinHeart;
    public SpriteRenderer dimmySR;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = 100;
        healthBar = GetComponentInChildren<UnityEngine.UI.Image>();
        healthText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        coinHeart = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, 100);
        healthText.text = health.ToString();
        healthBar.fillAmount = health / 100f;
    }
    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;
        healthBar.rectTransform.anchoredPosition = new Vector2(healthBar.rectTransform.anchoredPosition.x - damageAmount * 7, healthBar.rectTransform.anchoredPosition.y); // Move the health bar slightly to the right
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {     
        coinHeart.SetTrigger("hurt");
        dimmySR.color = Color.red;
        healthText.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        dimmySR.color = Color.white;
        healthText.color = Color.black;
    }
}
