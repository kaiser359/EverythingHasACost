using UnityEngine;

public class GunFlip : MonoBehaviour
{
    public GameObject dimmy;
    public GameObject Rot;
    private SpriteRenderer childSprite;
    private bool isFlipped;
    public float placement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dimmy = GameObject.FindWithTag("Player");
        if (transform.childCount > 0)
            childSprite = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        float rotZ = Rot.transform.eulerAngles.z;
        bool shouldFlip = rotZ > 90f && rotZ < 270f;
        
        // Set sprite layer based on placement
        if (placement > 0)
        {
            childSprite.sortingOrder = 1;
        }
        else if (placement < 0)
        {
            childSprite.sortingOrder = 7;
        }
        
        if (shouldFlip)
        {
            if (!isFlipped)
            {
                isFlipped = true;
                transform.rotation = Rot.transform.rotation * Quaternion.Euler(0f, 0f, 180f);
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            }
        }
        else
        {
            if (isFlipped)
            {
                isFlipped = false;
                transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                transform.rotation = Rot.transform.rotation;
            }
        }
    }
}
