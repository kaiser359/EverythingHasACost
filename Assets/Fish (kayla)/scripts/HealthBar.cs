using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Image = UnityEngine.UI.Image;

public class HealthBar : MonoBehaviour
{
    public UnityEngine.UI.Image healthBar;
    private TMPro.TextMeshProUGUI healthText;
    private Animator coinHeart;
    public Money money;
    private CinemachineCamera cam;
    //private Volume postProcess;
    private VolumeProfile postProcessProfile;
    private float ogVignetteIntensity;
    public float fadeDuration = 1f;
    public GameObject dimmy;
    public Image blackOverlay;

    private void Awake()
    {
        money.money = 10000;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        coinHeart = GetComponentInChildren<Animator>();
        cam = FindAnyObjectByType<CinemachineCamera>();
        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().enabled = false;
        //postProcess = FindObjectOfType<Volume>();
        postProcessProfile = FindAnyObjectByType<Volume>().profile;
        postProcessProfile.TryGet(out Vignette vignette);
        postProcessProfile.TryGet(out MotionBlur mb);
        blackOverlay.enabled = false;
        vignette.intensity.overrideState = false;
        mb.intensity.overrideState = false;
        ogVignetteIntensity = vignette.intensity.value;
    }

    // Update is called once per frame
    void Update()
    {
        money.money = Mathf.Clamp(money.money, 0, 10000);
        healthText.text = money.money.ToString();
        healthBar.fillAmount = money.money / 10000f;
        if (money.money <= 0)
        {
            StartCoroutine(Death());
            
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
        dimmy.GetComponent<SpriteRenderer>().color = Color.red;
        healthText.color = Color.red;
        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().enabled = true;
        postProcessProfile.TryGet(out Vignette vignette);
        postProcessProfile.TryGet(out MotionBlur mb);
        vignette.intensity.overrideState = true;
        mb.intensity.overrideState = true;
        yield return new WaitForSeconds(0.15f);
        dimmy.GetComponent<SpriteRenderer>().color = Color.white;
        healthText.color = Color.black;
        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().enabled = false;
        vignette.intensity.overrideState = false;
        mb.intensity.overrideState = false;
    }

    private IEnumerator Death()
    {
        dimmy.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        postProcessProfile.TryGet(out Vignette vignette);
        vignette.intensity.overrideState = true;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;           
            vignette.intensity.value = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        vignette.intensity.overrideState = false;
        vignette.intensity.value = ogVignetteIntensity;
        blackOverlay.enabled = true;
        yield return new WaitForSeconds(0.75f);
        SceneManager.LoadScene("death");
    }
}
