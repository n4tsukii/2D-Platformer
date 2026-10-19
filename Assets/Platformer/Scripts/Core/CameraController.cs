using UnityEngine;

namespace Platformer
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private float speed;
        private float currentPosX;
        private Vector3 velocity = Vector3.zero;

        [SerializeField] private Transform player;
        [SerializeField] private float aheadDistance;
        [SerializeField] private float cameraSpeed;
        private float lookAhead;

        [Header("Vertical Follow")]
        [SerializeField] private float verticalSmoothTime = 0.12f;
        private float minY, maxY;
        private bool followVertical;
        private float velocityY;

        private void Start()
        {
            // Frame whichever room the player starts in.
            Room closest = null;
            foreach (Room room in FindObjectsByType<Room>(FindObjectsSortMode.None))
                if (closest == null || Mathf.Abs(room.transform.position.x - player.position.x) <
                    Mathf.Abs(closest.transform.position.x - player.position.x))
                    closest = room;
            if (closest != null)
            {
                MoveToNewRoom(closest.transform);
                transform.position = new Vector3(currentPosX, TargetY(), transform.position.z);
            }
        }

        private void Update()
        {
            float x = Mathf.SmoothDamp(transform.position.x, currentPosX, ref velocity.x, speed);
            float y = followVertical
                ? Mathf.SmoothDamp(transform.position.y, TargetY(), ref velocityY, verticalSmoothTime)
                : transform.position.y;
            transform.position = new Vector3(x, y, transform.position.z);
        }

        private float TargetY()
        {
            return followVertical ? Mathf.Clamp(player.position.y, minY, maxY) : transform.position.y;
        }

        public void MoveToNewRoom(Transform _newRoom)
        {
            currentPosX = _newRoom.position.x;

            Room room = _newRoom.GetComponent<Room>();
            followVertical = room != null && player != null && room.CameraMaxY > room.CameraMinY;
            if (followVertical)
            {
                minY = _newRoom.position.y + room.CameraMinY;
                maxY = _newRoom.position.y + room.CameraMaxY;
            }
        }
    }
}
