using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    public GlobalPlayerInfo gS;
    Rigidbody2D rb;
    private float moveSpeed;
    private Vector2 desiredVelocity;

    private Animator animator;

    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        moveSpeed = gS.moveSpeed;
        rb = GetComponent<Rigidbody2D>();

        animator = GetComponent<Animator>();
    }
    public void Move(InputAction.CallbackContext ctx)
    {
        desiredVelocity = ctx.ReadValue<Vector2>() * moveSpeed;

        // flip the sprite based on movement direction
        if (ctx.started || ctx.performed)
        {
            transform.localScale = new Vector3(Mathf.Sign(desiredVelocity.x), 1, 1);
            if (transform.localScale.x < 0)
            {
                gS.rotationOffset = 0f; // Flip rotation when facing left
            }else
            {
                gS.rotationOffset = 180f; // Normal rotation when facing right
            }
        }

        // control animator
        Vector2 animVel = ctx.ReadValue<Vector2>().normalized;
        if (animator != null)
        {
            animator.SetFloat("xVel", Mathf.Abs(animVel.x));
            animator.SetFloat("yVel", animVel.y);
        }
    }

    private void Update()
    {
        rb.linearVelocity = desiredVelocity;
    }
}
