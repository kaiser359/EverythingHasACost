using UnityEngine;

public class ElevatorProximity : MonoBehaviour
{
    private GameObject player;
    private Animator animator;
    public float range = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // testing
        //Debug.Log(((Vector2)transform.position - (Vector2)player.transform.position).magnitude);

        if (((Vector2)transform.position - (Vector2)player.transform.position).magnitude < range)
        {
            animator.SetBool("isOpen", true);
        }
        else
        {
            animator.SetBool("isOpen", false);
        }
    }
}
