using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [Tooltip("Camera used to convert screen -> world. If null, Camera.main is used.")]
    public Camera cam;

    [Tooltip("Degrees per second used for smoothing. Set to a very large value for effectively instant rotation.")]
    public float rotationSpeed = 720f;

    [Tooltip("Add an offset (degrees) to the computed angle.")]
    public float rotationOffset = 0f;

    [Tooltip("When true, rotation will be smoothed; when false, it will snap instantly.")]
    public bool smooth = true;

    void Start()
    {
        if (cam == null)
            cam = Camera.main;
    }

    void Update()
    {
        if (cam == null)
            return;

        // Distance from camera to this object (used by ScreenToWorldPoint)
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, z));

        Vector3 dir = mouseWorld - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;
        Quaternion target = Quaternion.Euler(0f, 0f, angle);

        if (smooth)
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);
        else
            transform.rotation = target;
    }
}
