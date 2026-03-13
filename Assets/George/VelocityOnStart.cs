using UnityEngine;

public class VelocityOnStart : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GlobalPlayerInfo gS;

    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = -transform.right * gS.bulletSpeed; // Adjust the speed as needed
        }        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
