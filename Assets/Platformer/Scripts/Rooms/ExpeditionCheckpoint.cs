using UnityEngine;

namespace Platformer
{
    public class ExpeditionCheckpoint : MonoBehaviour
    {
        public Transform spawnPoint;
        public SpriteRenderer beacon;
        private bool activated;

        private void OnTriggerEnter2D(Collider2D other)
        {
            var player = other.GetComponent<ExpeditionPlayer>();
            if (player == null) return;
            player.SetCheckpoint(spawnPoint.position);
            if (activated) return;
            activated = true;
            beacon.color = new Color(0.6f, 1, 0.85f);
            other.GetComponent<Health>().AddHealth(1);
        }
    }
}
