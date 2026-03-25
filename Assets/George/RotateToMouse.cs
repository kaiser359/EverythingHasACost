using UnityEngine;

public class RotateToMouse : MonoBehaviour
{

    public GlobalPlayerInfo gS;
    private Camera cam;
    private float rotationSpeed;
    private float rotationOffset;

    void Start()
    {
        gS = FindAnyObjectByType<GlobalPlayerInfo>();
        cam = Camera.main;
        rotationOffset = gS.rotationOffset;
        rotationSpeed = gS.rotationSpeed;
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

        transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime);

    }
}
