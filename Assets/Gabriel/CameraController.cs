using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject target;
    public float speed = 5.0f;
    public Vector3 offset; //= new Vector3(0, 0, -10);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        transform.SetParent(null);
    }

    ////// Update is called once per frame
    //void FixedUpdate()
    //{
    //    transform.position = Vector3.Lerp(transform.position, target.transform.position + offset, speed * Time.deltaTime);
    //}

    [Range(0, 1)]
    public float smoothTime;

    public Transform playerTransform;
    public float amountToZoom = 10f;

    public void FixedUpdate()
    {
        Vector3 pos = GetComponent<Transform>().position;

        pos.x = Mathf.Lerp(pos.x, playerTransform.position.x, smoothTime);
        pos.y = Mathf.Lerp(pos.y, playerTransform.position.y, smoothTime);

        //pos.x = Mathf.Clamp(pos.x, 0 + (orthoSize * 1.775f), worldSize - (orthoSize * 1.775f));

        GetComponent<Transform>().position = pos;
    }
}