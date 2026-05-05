using UnityEngine;

public class BGM : MonoBehaviour
{
    public AudioClip bgmClip;
    private AudioSource audioSource;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = bgmClip;
        audioSource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
