using UnityEngine;

namespace Platformer
{
    [RequireComponent(typeof(PlayerRespawn), typeof(Rigidbody2D))]
    public sealed class FallRespawn : MonoBehaviour
    {
        [SerializeField] private float minimumHeight = -6f;
        private PlayerRespawn respawn;
        private Rigidbody2D body;

        private void Awake()
        {
            respawn = GetComponent<PlayerRespawn>();
            body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            if (body.position.y >= minimumHeight) return;
            body.linearVelocity = Vector2.zero;
            respawn.RespawnCheck();
        }
    }
}
