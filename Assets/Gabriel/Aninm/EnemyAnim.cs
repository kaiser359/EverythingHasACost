using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class EnemyAnim : MonoBehaviour
{
    public Animator anim;

    public float moveSpeed;
    private Rigidbody2D rb;

    private float x;
    private float y;
    private bool Possesed = false;
    private float startposition;
    private Vector2 input;
    private bool Moving;
    private Vector2 oldposition;
    private Vector2 newposition;
    public SpriteRenderer sr;
    void Start()
    {
    }
    private void Update()
    {


        newposition = transform.position;


        GetInput();

        Animate();

        startposition = Vector2.Distance(newposition, oldposition);
        oldposition = transform.position;
    }
    private void FixedUpdate()
    {
       // rb.velocity = input * moveSpeed;
    }

    private void GetInput()
    {
        //Vector2 velocity = newposition - oldposition;

        //input = new Vector2(x, y);
        //input.Normalize();
    }
    private void Animate()
    {
        if (newposition != oldposition) //|| input.magnitude < -0.1f)
        {
            Moving = true;
        }
        else { Moving = false; }

        if (newposition.x > oldposition.x && Mathf.Abs(newposition.x - oldposition.x) > Mathf.Abs(newposition.y - oldposition.y))
        {
            x = 1;
            sr.flipX = false;
        }
        else if (newposition.x < oldposition.x && Mathf.Abs(newposition.x - oldposition.x) > Mathf.Abs(newposition.y - oldposition.y))
        {
            x = -1;
            sr.flipX = true;
        }
        else { x = 0; }

        if(newposition.y > oldposition.y && Mathf.Abs(newposition.y - oldposition.y) > Mathf.Abs(newposition.x - oldposition.x))
        {
            y = 1;
        }
        else if (newposition.y < oldposition.y && Mathf.Abs(newposition.y - oldposition.y) > Mathf.Abs(newposition.x - oldposition.x))
        {
            y = -1;
        }
        else { y = 0; }

        if (Moving)
        {

            anim.SetFloat("X", x);
            anim.SetFloat("Y", y);

        }
        anim.SetBool("Moving", Moving);


    }
}
