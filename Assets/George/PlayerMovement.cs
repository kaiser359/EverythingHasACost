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

    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        moveSpeed = gS.moveSpeed;
        rb = GetComponent<Rigidbody2D>();
    }
    public void Move(InputAction.CallbackContext ctx)
    {
        desiredVelocity = ctx.ReadValue<Vector2>() * moveSpeed;
    }

    private void Update()
    {
        rb.linearVelocity = desiredVelocity;
    }
}
