using UnityEngine;
using UnityEngine.Rendering.Universal;
public class lightController : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Get the Light component attached to this GameObject
            Light2D light = GetComponent<Light2D>();
            if (light != null)
            {
                // Toggle the light on
                light.enabled = true;
            }
            else{  
                Debug.LogWarning("No Light component found on this GameObject."); 
            }
        }
    }
}
