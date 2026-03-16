using UnityEngine;

public class StoreInteract : MonoBehaviour
{
    public GlobalPlayerInfo gS;
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
        gS.Store.SetActive(true);
    }
}
