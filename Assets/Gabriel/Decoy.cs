using UnityEngine;

public class Decoy : MonoBehaviour
{
    public float lifetime = 5f;
    public float attractRadius = 6f;
    public string enemyTag = "Enemy";

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // optional: show visual or affect enemy AI; here just exist as a target.
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, attractRadius);
    }
}
