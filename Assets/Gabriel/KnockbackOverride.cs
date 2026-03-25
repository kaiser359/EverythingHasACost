using UnityEngine;

// Ensure this runs after most movement scripts so it can override velocity applied by them.
[DefaultExecutionOrder(1000)]
public class KnockbackOverride : MonoBehaviour
{
    private Rigidbody2D rb;
    private float remaining = 0f;
    private Vector2 velocity = Vector2.zero;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = GetComponentInParent<Rigidbody2D>();
    }

    // Apply a fixed velocity for the given duration (seconds). If an existing override
    // is active, extend it to the max of the two durations and use the latest velocity.
    public void Apply(Vector2 vel, float duration)
    {
        velocity = vel;
        remaining = Mathf.Max(remaining, duration);
    }

    void FixedUpdate()
    {
        if (remaining > 0f && rb != null)
        {
            rb.linearVelocity = velocity;
            remaining -= Time.fixedDeltaTime;
        }
        else if (remaining <= 0f)
        {
            // no longer needed
            Destroy(this);
        }
    }
}
