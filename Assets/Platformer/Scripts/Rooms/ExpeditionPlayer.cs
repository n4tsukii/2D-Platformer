using UnityEngine;

namespace Platformer
{
    // Separate from the original room-based respawn so the original stages retain their behaviour.
    public class ExpeditionPlayer : MonoBehaviour
    {
        private Vector3 safePosition;
        private Health health;
        private Rigidbody2D body;
        private float hazardCooldown;

        private void Awake()
        {
            safePosition = transform.position;
            health = GetComponent<Health>();
            body = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            if (transform.position.y < -7 && Time.time >= hazardCooldown) Hazard();
        }

        public void SetCheckpoint(Vector3 position)
        {
            safePosition = position;
        }

        public void Hazard()
        {
            if (Time.time < hazardCooldown || health.currentHealth <= 0) return;
            hazardCooldown = Time.time + 1.25f;
            health.TakeDamage(1);
            if (health.currentHealth > 0) ReturnToSafety();
        }

        // Called by the existing player death animation.
        public void RespawnCheck()
        {
            health.Respawn();
            ReturnToSafety();
        }

        private void ReturnToSafety()
        {
            transform.position = safePosition;
            body.linearVelocity = Vector2.zero;
        }
    }
}
