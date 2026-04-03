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
    }

    void Update()
    {
        bool shouldSnap = false;
        rotationSpeed = gS.rotationSpeed;
        if (cam == null)
            return;

        if(rotationOffset != gS.rotationOffset)
        {
            Debug.Log("Rotation offset changed, updating rotation immediately.");
            rotationOffset = gS.rotationOffset;
            shouldSnap = true; // Set to true to snap immediately when offset changes
        }
        // Distance from camera to this object (used by ScreenToWorldPoint)
        float z = Mathf.Abs(cam.transform.position.z - transform.position.z);
        Vector3 mouseScreen = Input.mousePosition;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(new Vector3(mouseScreen.x, mouseScreen.y, z));

        Vector3 dir = mouseWorld - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;
        Quaternion target = Quaternion.Euler(0f, 0f, angle);
        gS.aimDir = angle; // Update the aim direction in GlobalPlayerInfo
        if (shouldSnap)
        {
            transform.rotation = target; // Instantly rotate to the new target when offset changes
        }else{
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, rotationSpeed * Time.deltaTime * Mathf.Abs(angle - transform.rotation.eulerAngles.z) * 0.01f);
        }
    }
}
