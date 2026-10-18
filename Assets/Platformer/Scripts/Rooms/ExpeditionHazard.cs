using UnityEngine;

namespace Platformer
{
    public class ExpeditionHazard : MonoBehaviour
    {
        private void OnTriggerStay2D(Collider2D other)
        {
            other.GetComponent<ExpeditionPlayer>()?.Hazard();
        }
    }
}
