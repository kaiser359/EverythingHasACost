using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class HealthBar : MonoBehaviour
{
    private UnityEngine.UI.Image healthBar;
    private TMPro.TextMeshProUGUI healthText;
    private Animator coinHeart;
    public SpriteRenderer dimmySR;
    public Money money;
    private CinemachineCamera cam;
    //private Volume postProcess;
    private VolumeProfile postProcessProfile;

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
        cam = FindObjectOfType<CinemachineCamera>();
        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().enabled = false;
        //postProcess = FindObjectOfType<Volume>();
        postProcessProfile = FindObjectOfType<Volume>().profile;
        postProcessProfile.TryGet(out Vignette vignette);
        postProcessProfile.TryGet(out MotionBlur mb);
        vignette.intensity.overrideState = false;
        mb.intensity.overrideState = false;

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
        StartCoroutine(FlashRed());
    }

    private IEnumerator FlashRed()
    {     
        coinHeart.SetTrigger("hurt");
        dimmySR.color = Color.red;
        healthText.color = Color.red;
        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().enabled = true;
        postProcessProfile.TryGet(out Vignette vignette);
        postProcessProfile.TryGet(out MotionBlur mb);
        vignette.intensity.overrideState = true;
        mb.intensity.overrideState = true;
        yield return new WaitForSeconds(0.1f);
        dimmySR.color = Color.white;
        healthText.color = Color.black;
        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().enabled = false;
        vignette.intensity.overrideState = false;
        mb.intensity.overrideState = false;
    }
}
