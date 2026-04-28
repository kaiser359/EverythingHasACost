using System.Collections;
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
    public RotateToMouse rotateScript;

    public AudioClip footstepSound;

    // when true external systems (like dashes) should prevent this script from setting velocity
    public bool ignoreMovement = false;

    private Animator animatorBody;
    private GameObject head;
    private Animator animatorHead;

    private int moveSideBody;
    private bool isMoving = false;

    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        moveSpeed = gS.moveSpeed;
        rb = GetComponent<Rigidbody2D>();

        animatorBody = GetComponent<Animator>();
        head = transform.GetChild(0).gameObject;
        animatorHead = head.GetComponent<Animator>();

        moveSideBody = Animator.StringToHash("moveSide");
    }
    public void Move(InputAction.CallbackContext ctx)
    {
        if (!isMoving)
        {
            StartCoroutine(PlayFootsteps(ctx));
        }
        isMoving = true;
        desiredVelocity = ctx.ReadValue<Vector2>() * moveSpeed;

        // flip the sprite based on movement direction
        if (ctx.started || ctx.performed)
        {
            GetComponent<SpriteRenderer>().flipX = desiredVelocity.x < 0;
        }

        // control animator
        Vector2 animVel = ctx.ReadValue<Vector2>().normalized;
        if (animatorBody != null)
        {
            animatorBody.SetFloat("xVel", Mathf.Abs(animVel.x));
            animatorBody.SetFloat("yVel", animVel.y); // only enable gun flipping when not moving horizontally
        }
        if(ctx.canceled)
        {
            isMoving = false;
        }

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

        gunScript.GetComponent<GunFlip>().placement = -Mathf.Sin(gS.aimDir * Mathf.Deg2Rad); // pass vertical movement to gun script for layering

        // control head animator
        if (animatorHead != null)
        {
            animatorHead.SetFloat("Angle", Mathf.Sin(gS.aimDir * Mathf.Deg2Rad));
            animatorHead.SetBool("isMoving", desiredVelocity != Vector2.zero);
        }

        // move head down when moving sideways to prevent decapitating dimmy :sob:
        if (animatorBody.GetCurrentAnimatorStateInfo(0).shortNameHash == moveSideBody)
        {
            Debug.Log("moving sideways");
            head.transform.localPosition = new Vector3(0, -0.3f, 0);
        }
        else
        {
            head.transform.localPosition = Vector3.zero;
        }

        transform.GetChild(0).GetComponent<SpriteRenderer>().flipX = Mathf.Cos(gS.aimDir * Mathf.Deg2Rad) > 0; // flip head based on aim direction
    }

    private IEnumerator PlayFootsteps(InputAction.CallbackContext ctx)
    {
        while (ctx.ReadValue<Vector2>() != Vector2.zero)
        {
            GetComponent<AudioSource>().PlayOneShot(footstepSound);
            yield return new WaitForSeconds(0.2f);
            /*if(ctx.ReadValue<Vector2>() == Vector2.zero)
            {
                GetComponent<AudioSource>().Stop();
                yield break;
            }*/
        }
    }
}
