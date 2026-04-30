using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnimateDialogue : MonoBehaviour
{
    private float originalY;
    private float originalScale;

    private float yOffset = 0;
    private float maxYOffset = -30f;
    private float alpha = 1;

    private void Awake()
    {
        originalY = transform.localPosition.y;
        originalScale = transform.localScale.x;
    }

    public void StartListening()
    {
        StopCoroutine(CListening());
        StartCoroutine(CListening());
    }

    public void StartTalking()
    {
        StopCoroutine(CTalking());
        StartCoroutine(CTalking());
    }

    public void Jump()
    {
        StopCoroutine(CJump());
        StartCoroutine(CJump());
    }

    IEnumerator CListening()
    {
        float initialTime = Time.unscaledTime;
        float transitionTime = 0.5f;

        //float originalScale = currentChar.GetComponent<RectTransform>().localScale.x;

        while (Time.unscaledTime < initialTime + transitionTime)
        {
            float t = (Time.unscaledTime - initialTime) / transitionTime;

            yOffset = Mathf.Lerp(0f, maxYOffset, -t * (t - 2));
            alpha = Mathf.Lerp(1, 0.5f, -t * (t - 2));

            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(gameObject.GetComponent<RectTransform>().anchoredPosition.x, originalY + yOffset);
            gameObject.transform.localScale = Vector3.one * (Mathf.Lerp(originalScale, 0.9f, -t * (t - 2)));

            gameObject.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    IEnumerator CTalking()
    {
        float initialTime = Time.unscaledTime;
        float transitionTime = 0.5f;

        float currentOffset = yOffset;
        float currentAlpha = alpha;

        //float originalScale = currentChar.GetComponent<RectTransform>().localScale.x;

        while (Time.unscaledTime < initialTime + transitionTime)
        {
            float t = (Time.unscaledTime - initialTime) / transitionTime;

            yOffset = Mathf.Lerp(currentOffset, 0f, -t * (t - 2) );
            alpha = Mathf.Lerp(currentAlpha, 1, -t * (t - 2) );

            //gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(gameObject.GetComponent<RectTransform>().anchoredPosition.x, Mathf.Lerp(transform.localPosition.y, originalY, -t * (t - 2) ));
            gameObject.transform.localScale = Vector3.one * (Mathf.Lerp(originalScale, 1, -t * (t - 2)));

            gameObject.GetComponent<Image>().color = new Color(1, 1, 1, alpha);
            yield return null;
        }
    }

    IEnumerator CJump()
    {
        float initialTime = Time.unscaledTime;
        float jumpTime = 0.5f;
        float jumpHeight = 10f;

        //float originalY = currentChar.GetComponent<RectTransform>().anchoredPosition.y;

        while (Time.unscaledTime < initialTime + jumpTime)
        {
            float t = (Time.unscaledTime - initialTime) / jumpTime;

            float y = -4 * t * (t - 1) * jumpHeight; // Parabolic jump formula

            Debug.Log("Jumping: " + gameObject.name + " at time: " + Time.unscaledTime + " with y: " + y);

            //currentChar.GetComponent<RectTransform>().localScale = Vector3.one * (1 + 0.1f * Mathf.Sin(t * Mathf.PI)); // Scale up and down for a bouncing effect
            gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector3(gameObject.GetComponent<RectTransform>().anchoredPosition.x, originalY + y + yOffset);
            yield return null;
        }
    }
}
