using UnityEngine;
using System.Collections.Generic;

namespace Gabriel
{
    public class LaserBeam : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private List<Vector2> pathPoints;
        private Vector2 direction;
        private float laserSpeed;
        private float maxDistance;
        private int maxBounces;
        private LayerMask obstacleLayer;
        private float currentExtension;
        private float activeTimer;
        private float activeDuration = 5f;
        private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();
        private const int damageAmount = 5;

        void Start()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
                lineRenderer.startWidth = 0.1f;
                lineRenderer.endWidth = 0.1f;
            }

            CalculateLaserPath();
            activeTimer = activeDuration;
        }

        void Update()
        {
            activeTimer -= Time.deltaTime;

            if (activeTimer <= 0f)
            {
                Destroy(gameObject);
                return;
            }

            // Extend laser over time based on laserSpeed
            currentExtension += laserSpeed * Time.deltaTime;
            UpdateLineRenderer();
        }

        public void Initialize(Vector2 shootDirection, float speed, float maxDist, int bounces, LayerMask obstacles)
        {
            direction = shootDirection.normalized;
            laserSpeed = speed;
            maxDistance = maxDist;
            maxBounces = bounces;
            obstacleLayer = obstacles;
            currentExtension = 0f;
        }

        private void CalculateLaserPath()
        {
            pathPoints = new List<Vector2>();
            Vector2 currentPos = (Vector2)transform.position;
            Vector2 currentDir = direction;
            float distanceRemaining = maxDistance;
            int bouncesLeft = maxBounces;

            pathPoints.Add(currentPos);

            while (distanceRemaining > 0f && bouncesLeft > 0)
            {
                RaycastHit2D hit = Physics2D.Raycast(currentPos, currentDir, distanceRemaining, obstacleLayer);

                if (hit.collider != null)
                {
                    pathPoints.Add(hit.point);
                    distanceRemaining -= hit.distance;
                    bouncesLeft--;

                    // Calculate bounce direction
                    currentDir = Vector2.Reflect(currentDir, hit.normal);
                    currentPos = hit.point + currentDir * 0.01f; // Offset to prevent re-hitting

                    // Check for damage-able objects
                    var healthComponent = hit.collider.GetComponent<HealthBar>();
                    if (healthComponent != null && !hitObjects.Contains(hit.collider))
                    {
                        healthComponent.TakeDamage(damageAmount);
                        hitObjects.Add(hit.collider);
                    }
                }
                else
                {
                    // No more obstacles, extend to max distance
                    pathPoints.Add(currentPos + currentDir * distanceRemaining);
                    break;
                }
            }
        }

        private void UpdateLineRenderer()
        {
            if (pathPoints.Count < 2)
                return;

            List<Vector3> displayPoints = new List<Vector3>();
            float distanceCovered = 0f;

            for (int i = 0; i < pathPoints.Count - 1; i++)
            {
                Vector2 segmentStart = pathPoints[i];
                Vector2 segmentEnd = pathPoints[i + 1];
                float segmentLength = Vector2.Distance(segmentStart, segmentEnd);

                displayPoints.Add(segmentStart);

                if (distanceCovered + segmentLength <= currentExtension)
                {
                    distanceCovered += segmentLength;
                    displayPoints.Add(segmentEnd);
                }
                else
                {
                    // Partially draw this segment
                    float remainingDistance = currentExtension - distanceCovered;
                    Vector2 partialEnd = Vector2.Lerp(segmentStart, segmentEnd, remainingDistance / segmentLength);
                    displayPoints.Add(partialEnd);
                    break;
                }
            }

            lineRenderer.positionCount = displayPoints.Count;
            for (int i = 0; i < displayPoints.Count; i++)
            {
                lineRenderer.SetPosition(i, displayPoints[i]);
            }
        }

        public void SetActiveDuration(float duration)
        {
            activeDuration = duration;
        }
    }
}