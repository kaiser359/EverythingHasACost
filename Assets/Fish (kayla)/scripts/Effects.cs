using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Effects : MonoBehaviour
{
    public bool flicker; //random bloom
    public bool glitch; //random chrome abberation
    public Volume gv;
    private Bloom bloom;
    private ChromaticAberration ca;
    [Range(0.2f, 50f)]
    public float maxBloom = 50f;
    [Range(0.1f, 0.7f)]
    public float maxCA = 0.7f;
    [Range(0f, 0.7f)]
    public float minCA = 0.3f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(randomEffect());
    }

    private IEnumerator randomEffect()
    {
        while (true)
        {
            if (gv.profile.TryGet<Bloom>(out bloom) && flicker)
            {
                float elapsedTime = 0f;
                float fade = Random.Range(0.1f, 1f);
                float fadeIntensity = Random.Range(0.1f, maxBloom);
                float startIntensity = bloom.intensity.value;
                while (elapsedTime < fade)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / fade;
                    bloom.intensity.value = Mathf.Lerp(startIntensity, fadeIntensity, t);
                    yield return null;
                }
            }
            if (gv.profile.TryGet<ChromaticAberration>(out ca) && glitch)
            {
                float elapsedTime = 0f;
                float fade = Random.Range(0.1f, 1f);
                float fadeIntensity = Random.Range(minCA, maxCA);
                float startIntensity = ca.intensity.value;
                while (elapsedTime < fade)
                {
                    elapsedTime += Time.deltaTime;
                    float t = elapsedTime / fade;
                    ca.intensity.value = Mathf.Lerp(startIntensity, fadeIntensity, t);
                    yield return null;
                }
            }
            //yield return new WaitForSeconds(Random.Range(0.1f, 1f));
        }
    }
}
