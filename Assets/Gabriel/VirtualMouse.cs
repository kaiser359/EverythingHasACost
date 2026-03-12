using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VirtualMouse : MonoBehaviour
{
    public Camera playerCamera;
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public Sprite reticle;
    public float reticleWorldZ = 0f;
    public float reticleScale = 1f;
    public CinemachineConfiner2D confiner;

    private Vector2 aimDelta;
    public Vector2 aimPosition;
    private GameObject _reticle;
    private GameObject _hover;

    private void Awake()
    {
        var bound = GameObject.Find("Dark background vr3_0");
        if (bound != null) confiner.BoundingShape2D = bound.GetComponent<Collider2D>();

        eventSystem ??= EventSystem.current;
        if (uiRaycaster == null)
        {
            var c = FindAnyObjectByType<Canvas>();
            if (c != null)
            {
                uiRaycaster = c.GetComponent<GraphicRaycaster>();
                if (c.renderMode == RenderMode.WorldSpace && c.worldCamera == null)
                    c.worldCamera = playerCamera;
            }
        }

        aimPosition = new Vector2(Screen.width, Screen.height) / 2;
        if (reticle != null)
        {
            _reticle = new GameObject("Reticle");
            var sr = _reticle.AddComponent<SpriteRenderer>();
            sr.sprite = reticle;
            sr.sortingOrder = 1000;
            _reticle.transform.localScale = Vector3.one * reticleScale;
        }

        ClampAim();
    }
    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
       // Mouse.current.WarpCursorPosition(aimPosition); //if u wanna make everything simpler, just use this. but it will be harder to move the cursor outside the game. but it will allow multiple canvas to work
        
        aimPosition += aimDelta;
        ClampAim();
        UpdateReticle();
        UpdateHover();
    }

    private void ClampAim()
    {
        if (playerCamera == null)
        {
            aimPosition.x = Mathf.Clamp(aimPosition.x, 0, Screen.width);
            aimPosition.y = Mathf.Clamp(aimPosition.y, 0, Screen.height);
            return;
        }
        var r = playerCamera.pixelRect;
        aimPosition.x = Mathf.Clamp(aimPosition.x, r.xMin, r.xMax);
        aimPosition.y = Mathf.Clamp(aimPosition.y, r.yMin, r.yMax);
    }

    private void UpdateReticle()
    {
        if (_reticle == null || playerCamera == null) return;
        float z = Mathf.Abs(playerCamera.transform.position.z - reticleWorldZ);
        var world = playerCamera.ScreenToWorldPoint(new Vector3(aimPosition.x, aimPosition.y, z));
        world.z = reticleWorldZ;
        _reticle.transform.position = world;
    }

    // returns topmost UI GameObject under aimPosition (or null)
    private GameObject RaycastUI(out RaycastResult topResult)
    {
        topResult = new RaycastResult();
        if (uiRaycaster == null || (eventSystem ?? EventSystem.current) == null) return null;
        var ped = new PointerEventData(eventSystem ?? EventSystem.current) { position = aimPosition };
        var list = new List<RaycastResult>();
        uiRaycaster.Raycast(ped, list);
        if (list.Count == 0) return null;
        topResult = list[0];
        return topResult.gameObject;
    }

    private void UpdateHover()
    {
        var es = eventSystem ?? EventSystem.current;
        var top = RaycastUI(out _);
        if (top == _hover) return;

        if (_hover != null)
            ExecuteEvents.ExecuteHierarchy(_hover, new PointerEventData(es) { position = aimPosition }, ExecuteEvents.pointerExitHandler);

        if (top != null)
        {
            ExecuteEvents.ExecuteHierarchy(top, new PointerEventData(es) { position = aimPosition }, ExecuteEvents.pointerEnterHandler);
            var s = top.GetComponentInParent<Selectable>();
            if (s != null && es != null) es.SetSelectedGameObject(s.gameObject);
        }
        else if (es != null) es.SetSelectedGameObject(null);

        _hover = top;
    }

    public void Click(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        var es = eventSystem ?? EventSystem.current;

        // UI click
        var top = RaycastUI(out var r);
        if (top != null && es != null)
        {
            // prefer Button.onClick on parent
            var btn = top.GetComponentInParent<Button>();
            if (btn != null) { btn.onClick.Invoke(); return; }

            var ped = new PointerEventData(es)
            {
                position = aimPosition,
                pressPosition = aimPosition,
                clickCount = 1,
                button = PointerEventData.InputButton.Left,
                pointerId = -1,
                pointerCurrentRaycast = r,
                pointerPressRaycast = r
            };
            ExecuteEvents.ExecuteHierarchy(top, ped, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.ExecuteHierarchy(top, ped, ExecuteEvents.pointerUpHandler);
            ExecuteEvents.ExecuteHierarchy(top, ped, ExecuteEvents.pointerClickHandler);
            return;
        }

        // world-space 2D fallback
        if (playerCamera != null)
        {
            float z = Mathf.Abs(playerCamera.transform.position.z - reticleWorldZ);
            var world = playerCamera.ScreenToWorldPoint(new Vector3(aimPosition.x, aimPosition.y, z));
            var col = Physics2D.OverlapPoint((Vector2)world);
            if (col != null)
            {
                var go = col.gameObject;
                var b = go.GetComponentInParent<Button>();
                if (b != null) { b.onClick.Invoke(); return; }
                if (es != null)
                {
                    var ped = new PointerEventData(es) { position = aimPosition, pointerId = -1 };
                    ExecuteEvents.ExecuteHierarchy(go, ped, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.ExecuteHierarchy(go, ped, ExecuteEvents.pointerUpHandler);
                    ExecuteEvents.ExecuteHierarchy(go, ped, ExecuteEvents.pointerClickHandler);
                    return;
                }
                go.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void Aim(InputAction.CallbackContext ctx) => aimDelta = ctx.ReadValue<Vector2>();

    private void OnDestroy() { if (_reticle != null) Destroy(_reticle); }
}
