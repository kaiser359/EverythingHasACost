using UnityEngine;

namespace Gabriel
{
   
    public class LaserOrbiterPlaceholder : MonoBehaviour
    {
        NuclearBomb parent;
        int index;
        int total;
        float radius;
        float speedDeg;
        float range;
        LayerMask enemyLayer;

        float angleDeg;

        public void Initialize(NuclearBomb parent, int idx, int total, float radius, float speedDeg, float range, LayerMask enemyLayer)
        {
            this.parent = parent;
            this.index = idx;
            this.total = total;
            this.radius = radius;
            this.speedDeg = speedDeg;
            this.range = range;
            this.enemyLayer = enemyLayer;
            angleDeg = (360f / Mathf.Max(1, total)) * idx;
        }

        void Update()
        {
            if (parent == null) return;

            angleDeg += speedDeg * Time.deltaTime;
            float rad = angleDeg * Mathf.Deg2Rad;
            Vector3 pos = parent.transform.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
            transform.position = pos;

            // simple laser: do a short raycast outward from orbiter to parent+direction
            Vector3 dir = (pos - parent.transform.position).normalized;
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, range, enemyLayer);
            if (hit.collider != null)
            {
                var eh = hit.collider.GetComponent<EnemyHealth>();
                if (eh != null) eh.TakeDamage(50); 
            }
        }
    }
}
