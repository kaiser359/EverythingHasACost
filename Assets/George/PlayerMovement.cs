using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]

    public GlobalPlayerInfo gS;
    Rigidbody2D rb;
    Vector2 desiredVelocity;
    private float moveSpeed;
    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        moveSpeed = gS.moveSpeed;
        rb = GetComponent<Rigidbody2D>();
    }
    // Called by the Input System. Bind your action to `Move` with a Vector2 control (e.g., WASD or left stick).
    public void Move(InputAction.CallbackContext ctx)
    {
        desiredVelocity = ctx.ReadValue<Vector2>() * moveSpeed;
    }

    private void Update()
    {
        rb.linearVelocity = desiredVelocity;
    }
}
