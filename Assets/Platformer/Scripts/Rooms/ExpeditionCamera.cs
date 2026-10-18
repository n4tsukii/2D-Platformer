using UnityEngine;

namespace Platformer
{
    public class ExpeditionCamera : MonoBehaviour
    {
        public Transform target;
        public float left = 0;
        public float right = 64;
        private Vector3 velocity;

        private void LateUpdate()
        {
            if (target == null) return;
            float halfWidth = GetComponent<Camera>().orthographicSize * GetComponent<Camera>().aspect;
            float x = Mathf.Clamp(target.position.x + Mathf.Sign(target.localScale.x) * 1.2f,
                left + halfWidth, right - halfWidth);
            transform.position = Vector3.SmoothDamp(transform.position,
                new Vector3(x, 0, -10), ref velocity, 0.18f);
        }
    }
}
