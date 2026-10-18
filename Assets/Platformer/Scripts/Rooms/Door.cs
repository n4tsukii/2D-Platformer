using UnityEngine;

namespace Platformer
{
    public class Door : MonoBehaviour
    {
        [SerializeField] private Transform previousRoom;
        [SerializeField] private Transform nextRoom;
        [SerializeField] private CameraController cam;

        private void Awake()
        {
            // A room passage must never become a solid wall when an old exit is reused.
            GetComponent<BoxCollider2D>().isTrigger = true;
            cam = Camera.main.GetComponent<CameraController>();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (previousRoom == null || nextRoom == null) return;

            if (collision.tag == "Player")
            {
                if (collision.transform.position.x < transform.position.x)
                {
                    cam.MoveToNewRoom(nextRoom);
                    nextRoom.GetComponent<Room>().ActivateRoom(true);
                    previousRoom.GetComponent<Room>().ActivateRoom(false);
                }
                else
                {
                    cam.MoveToNewRoom(previousRoom);
                    previousRoom.GetComponent<Room>().ActivateRoom(true);
                    nextRoom.GetComponent<Room>().ActivateRoom(false);
                }
            }
        }
    }
}
