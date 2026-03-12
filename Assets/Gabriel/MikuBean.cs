using System;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent(typeof(PlayerStats))]
public class MikuBean : MonoBehaviour
{
    [Header("References")]
    public Transform lazerFirePoint;
    public GameObject lazer; // visual laser object that will rotate
    public GameObject beanPrefab;

    [Header("Projectile")]
    public float throwForce = 6f;
    public float throwInterval = 2f;
    public float throwSpreadDegrees = 15f; // random spread from exact aim

    [Header("Detection & Movement")]
    public float detectionRange = 10f;
    public float chaseRange = 15f;
    public float chaseSpeed = 2.5f;
    public float wanderRadius = 2f;
    public float wanderSpeed = 0.8f;
    public float idleMin = 0.5f;
    public float idleMax = 2f;
    public float aimSpeed = 3f; // how quickly the laser aims toward the player when detected

    private Transform playerTransform;
    private Vector3 originPosition;
    private Vector3 wanderTarget;
    private float wanderIdleTimer = 0f;
    private bool isIdling = false;

    private Vector2 _currentAimDir = Vector2.right;
    private float shootTimer = 0f;

    private void Awake()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) playerTransform = p.transform;

        originPosition = transform.position;
        PickNewWanderTarget();
    }

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        float dist = playerTransform != null ? Vector2.Distance(transform.position, playerTransform.position) : Mathf.Infinity;

        // Aim target direction (if player exists aim towards them, otherwise keep current aim)
        if (playerTransform != null)
        {
            Vector2 targetDir = (playerTransform.position - transform.position);
            if (targetDir.sqrMagnitude > 0.0001f)
            {
                // when player is within detectionRange the lazer will slowly turn towards player
                if (dist <= detectionRange)
                    _currentAimDir = Vector2.Lerp(_currentAimDir, targetDir.normalized, aimSpeed * Time.deltaTime);
                else
                    _currentAimDir = Vector2.Lerp(_currentAimDir, targetDir.normalized, (aimSpeed * 0.4f) * Time.deltaTime);
            }
        }

        // rotate lazer visual if present
        if (lazer != null)
        {
            float angle = Mathf.Atan2(_currentAimDir.y, _currentAimDir.x) * Mathf.Rad2Deg;
            lazer.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Behavior: stop and shoot if in detectionRange, else chase if within chaseRange, else wander
        if (dist <= detectionRange)
        {
            // Shoot (stationary)
            if (shootTimer <= 0f)
            {
                ThrowBean();
                shootTimer = throwInterval;
            }
        }
        else if (playerTransform != null && dist <= chaseRange)
        {
            // Chase player
            Vector3 dir = (playerTransform.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, transform.position + dir, chaseSpeed * Time.deltaTime);
        }
        else
        {
            // Wander
            if (isIdling)
            {
                wanderIdleTimer -= Time.deltaTime;
                if (wanderIdleTimer <= 0f)
                {
                    isIdling = false;
                    PickNewWanderTarget();
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, wanderTarget, wanderSpeed * Time.deltaTime);
                if (Vector2.Distance(transform.position, wanderTarget) < 0.1f)
                {
                    isIdling = true;
                    wanderIdleTimer = UnityEngine.Random.Range(idleMin, idleMax);
                }
            }
        }
    }

    void PickNewWanderTarget()
    {
        Vector2 offset = UnityEngine.Random.insideUnitCircle * wanderRadius;
        wanderTarget = originPosition + (Vector3)offset;
    }

    void ThrowBean()
    {
        if (beanPrefab == null) return;

        Vector3 spawnPos = lazerFirePoint != null ? lazerFirePoint.position : transform.position;

        // base aim direction is current aim dir
        Vector2 aim = _currentAimDir.normalized;

        // apply random spread so it doesn't hit player exactly
        float halfSpread = throwSpreadDegrees * 0.5f;
        float randomAngle = UnityEngine.Random.Range(-halfSpread, halfSpread);
        float rad = randomAngle * Mathf.Deg2Rad;
        Vector2 spreadDir = new Vector2(
            aim.x * Mathf.Cos(rad) - aim.y * Mathf.Sin(rad),
            aim.x * Mathf.Sin(rad) + aim.y * Mathf.Cos(rad)
        ).normalized;

        GameObject b = Instantiate(beanPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = b.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = spreadDir * throwForce;
        }
    }
}
