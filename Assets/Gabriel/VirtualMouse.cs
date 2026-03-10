using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class VirtualMouse : MonoBehaviour
{
    // imma add comments so u understand what this script does. its  pretty big.
    // this script became a "screen" which creates a cursor for most abilities.
    // this now only manages the virtual mouse / reticle. it can be called for power ups, in fact u might find it useful. 

    public Camera playerCamera;

    private Vector2 aimDelta;
    public Vector2 aimPosition;

    public Sprite reticle;

    public float reticleWorldZ = 0f;

    public float reticleScale = 1f;
    private GameObject _reticleInstance;
    private SpriteRenderer _reticleSR; // this is the sprite for the mouse, that is if u want personalized mouse, if not u can just set the reticle to a blank sprite and it will work as a normal mouse.

    public CinemachineConfiner2D confiner; // if we do not use cinemachine, we can just use a collider and set the confiner to it, but this is more flexible for now. if we want to change the confiner at runtime, we can do that easily with this setup.

    private void Awake()
    {// confiner 
        GameObject boundingObject = GameObject.Find("Dark background vr3_0");
        if (boundingObject != null)
        {
            confiner.BoundingShape2D = boundingObject.GetComponent<Collider2D>();
        }

        aimPosition = new Vector2(Screen.width, Screen.height) / 2;

        if (reticle != null)
        {
            _reticleInstance = new GameObject("Reticle");
            _reticleSR = _reticleInstance.AddComponent<SpriteRenderer>();
            _reticleSR.sprite = reticle;
            _reticleSR.sortingOrder = 1000;
            _reticleInstance.transform.localScale = Vector3.one * reticleScale;
        }

        ClampAimToCamera();
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        aimPosition += aimDelta;
        ClampAimToCamera();
        UpdateReticlePosition();
    }

    private void ClampAimToCamera()
    {
        if (playerCamera == null)
        {
            aimPosition.x = Mathf.Clamp(aimPosition.x, 0f, Screen.width);
            aimPosition.y = Mathf.Clamp(aimPosition.y, 0f, Screen.height);
            return;
        }

        Rect pr = playerCamera.pixelRect;
        aimPosition.x = Mathf.Clamp(aimPosition.x, pr.xMin, pr.xMax);
        aimPosition.y = Mathf.Clamp(aimPosition.y, pr.yMin, pr.yMax);
    }

    private void UpdateReticlePosition()
    {
        if (_reticleInstance == null || playerCamera == null)
            return;

        float zDistance = Mathf.Abs(playerCamera.transform.position.z - reticleWorldZ);
        Vector3 screenPoint = new Vector3(aimPosition.x, aimPosition.y, zDistance);
        Vector3 world = playerCamera.ScreenToWorldPoint(screenPoint);
        world.z = reticleWorldZ;
        _reticleInstance.transform.position = world;
    }

    public void Teleport(InputAction.CallbackContext ctx)
    {
        // Intentionally empty, i was thinking one of the power ups to be teleportation into fire, we in hekk so it would be pretty cool. i may or may not to somethign with it later. ur choice george
        return;
    }

    public void Aim(InputAction.CallbackContext ctx)
    {
        Vector2 delta = ctx.ReadValue<Vector2>();
        aimDelta = delta;
    }

    private void OnDestroy()
    {
        if (_reticleInstance != null)
            Destroy(_reticleInstance);
    }
}
