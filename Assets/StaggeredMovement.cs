using UnityEngine;

public class StaggeredMovement : MonoBehaviour
{
    public float gridSize = 1f;
    private Camera mainCamera;

    private float NearestMutliple(float value, float multiple)
    {
        return Mathf.RoundToInt(value / multiple) * multiple;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(NearestMutliple(mainCamera.transform.position.x, gridSize),
                                         NearestMutliple(mainCamera.transform.position.y, gridSize),
                                         -10);
    }
}
