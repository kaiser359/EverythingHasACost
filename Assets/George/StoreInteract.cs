using UnityEngine;
using UnityEngine.InputSystem;

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
    public void Interact(InputAction.CallbackContext ctx)
    {
        // activate only once
        if (!ctx.started)
        {
            return;
        }

        Debug.Log("notice me senpai");
        if(Store != null)
        {
            Store.GetComponent<Store>().StartStore();
        }
    }
    public void LeaveStore(InputAction.CallbackContext ctx)
    {
        // activate only once
        if (!ctx.started)
        {
            return;
        }

        Debug.Log("notice me senpai2");
        if (Store != null)
        {
            Store.GetComponent<Store>().ExitStore();
            //Store.GetComponent<Store>().storePanel.SetActive(false);
            //Time.timeScale = 1f;
        }
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
