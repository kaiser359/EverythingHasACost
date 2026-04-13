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
    public GameObject gunScript;

    // when true external systems (like dashes) should prevent this script from setting velocity
    public bool ignoreMovement = false;

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
            GetComponent<SpriteRenderer>().flipX = desiredVelocity.x < 0;
        }

        // control animator
        Vector2 animVel = ctx.ReadValue<Vector2>().normalized;
        if (animator != null)
        {
            animator.SetFloat("xVel", Mathf.Abs(animVel.x));
            animator.SetFloat("yVel", animVel.y); // only enable gun flipping when not moving horizontally
        }
        gunScript.GetComponent<GunFlip>().placement = ctx.ReadValue<Vector2>().y; // pass vertical movement to gun script for layering
    }

    private void Update()
    {

        if (!ignoreMovement){
            rb.linearVelocity = desiredVelocity;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }

    }
}
