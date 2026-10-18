using UnityEngine;

namespace Platformer
{
    public class ExpeditionFinish : MonoBehaviour
    {
        public ExpeditionStage stage;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player")) stage.Complete();
        }
    }
}
