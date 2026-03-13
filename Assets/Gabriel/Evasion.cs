//using System.Collections;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class Evasion : MonoBehaviour
//{

//    public KeyCode dashKey = KeyCode.Space;
   
//    public float dashDistance = 5f;
  
//    public float dashDuration = 0.15f;
  
//    public float dashCooldown = 0.5f;

//    private Rigidbody2D _rb;
//    private Collider2D _col;
//    private bool _isDashing;
//    private float _lastDashTime = -10f;
//    private RigidbodyConstraints2D _originalConstraints;

    
//    void Start()
//    {
      
//        _rb = GetComponent<Rigidbody2D>();
//        _col = GetComponent<Collider2D>();
//        if (_rb != null) _originalConstraints = _rb.constraints;
//    }

//    public void Dash(InputAction.CallbackContext ctx)
//    {
//        if (ctx.canceled)
//            return;

//        if (!_isDashing && Time.time >= _lastDashTime + dashCooldown)
//        {
//            StartCoroutine(DashRoutine());
//        }
//    }

//    private IEnumerator DashRoutine()
//    {
//        _isDashing = true;
//        _lastDashTime = Time.time;

//        if (_rb != null)
//        {
           
//            _rb.constraints |= RigidbodyConstraints2D.FreezeRotation;
//        }

//        if (_col != null)
//        {
          
//            _col.enabled = false;
//        }


//        Vector2 inputDir = GetComponent<PlayerMovement>().lastMoveDirection;
//        Vector2 dashDir = inputDir.sqrMagnitude > 0.0001f ? inputDir.normalized : (Vector2)transform.up;
//        Vector2 startPos = _rb != null ? _rb.position : (Vector2)transform.position;
//        Vector2 targetPos = startPos + dashDir * dashDistance;

//        float elapsed = 0f;

//        while (elapsed < dashDuration)
//        {
//            elapsed += Time.fixedDeltaTime;
//            float t = Mathf.Clamp01(elapsed / dashDuration);
//            Vector2 newPos = Vector2.Lerp(startPos, targetPos, t);

//            if (_rb != null)
//                _rb.MovePosition(newPos);
//            else
//                transform.position = newPos;

//            yield return new WaitForFixedUpdate();
//        }


//        if (_rb != null)
//            _rb.MovePosition(targetPos);
//        else
//            transform.position = targetPos;


//        if (_col != null) _col.enabled = true;
//        if (_rb != null) _rb.constraints = _originalConstraints;

//        _isDashing = false;
//    }

  
//    public bool IsDashing => _isDashing;
//}

