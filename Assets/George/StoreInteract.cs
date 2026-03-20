using UnityEngine;

public class StoreInteract : MonoBehaviour
{
    public GameObject Store;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
    public void Interact()
    {
        if(Store != null)
        {
            Store.GetComponent<Store>().StartStore();
        }
    }
    public void LeaveStore() { 
        Store.transform.GetChild(0).gameObject.SetActive(false);
        Time.timeScale = 1f;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.CompareTag("Shop"))
        {
            Store = collision.gameObject;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
        {
         
                Store = null;
         
    }
}
