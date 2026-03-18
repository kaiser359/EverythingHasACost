using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class HealthBar : MonoBehaviour
{
    private UnityEngine.UI.Image healthBar;
    private TMPro.TextMeshProUGUI healthText;
    private Animator coinHeart;
    public SpriteRenderer dimmySR;
    public Money money;
    //public float adjust;

    private void Awake()
    {
        money.money = 10000;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthBar = GetComponentInChildren<UnityEngine.UI.Image>();
        healthText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        coinHeart = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        money.money = Mathf.Clamp(money.money, 0, 10000);
        healthText.text = money.money.ToString();
        healthBar.fillAmount = money.money / 10000f;
        if (money.money <= 0)
        {
            SceneManager.LoadScene("death");
        }
    }
    public void TakeDamage(int damageAmount)
    {
        money.money -= damageAmount;
        //healthBar.rectTransform.anchoredPosition = new Vector2(healthBar.rectTransform.anchoredPosition.x - damageAmount * adjust, healthBar.rectTransform.anchoredPosition.y); // Move the health bar slightly to the right
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
