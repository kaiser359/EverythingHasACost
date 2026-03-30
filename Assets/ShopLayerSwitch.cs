using UnityEngine;

public class ShopLayerSwitch : MonoBehaviour
{
    private GameObject player;
    private BoxCollider2D shopCollider;
    private int shopLayerOrder;

    // Start is called before the first frame update
    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        shopCollider = GetComponent<BoxCollider2D>();
        shopLayerOrder = transform.parent.GetComponent<SpriteRenderer>().sortingOrder; // get the sorting order of the shop sprite
    }

    // Update is called once per frame
    void Update()
    {
        // check if the player is outside the horizontal bounds of the shop collider
        if (Mathf.Abs(transform.position.x - player.transform.position.x) > shopCollider.size.x/2)
        {
            return;
        }

        // check if the player is above the shop collider
        if (player.transform.position.y > transform.position.y + shopCollider.offset.y)
        {
            // player is above the shop
            player.GetComponent<SpriteRenderer>().sortingOrder = shopLayerOrder - 1;
        }
        else
        {
            // player is below the shop
            player.GetComponent<SpriteRenderer>().sortingOrder = shopLayerOrder + 1;
        }
    }
}
