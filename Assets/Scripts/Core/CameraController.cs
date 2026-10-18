using UnityEngine;
using UnityEngine.Serialization;

public class CameraController : MonoBehaviour
{
    // Camera follow room
    [SerializeField] private float speed;
    [SerializeField] Transform initialRoom;
    private Vector3 velocity = Vector3.zero;
    private float curposX;

    // Camera follow player
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float cameraSpeed;

    private float lookahead;

    void Awake()
    {
        curposX = initialRoom.position.x;
        transform.position = new Vector3(
            initialRoom.position.x,
            initialRoom.position.y,
            transform.position.z
    );
    }

    void Update()
    {
        // Camera follow room
        transform.position = Vector3.SmoothDamp(transform.position, new Vector3(curposX, transform.position.y, transform.position.z), ref velocity, speed);

        // Camera follow player
        // transform.position = new Vector3(player.position.x + lookahead, transform.position.y, transform.position.z);
        // lookahead = Mathf.Lerp(lookahead, (aheadDistance * player.localScale.x), Time.deltaTime *cameraSpeed);
    }


    public void MoveToNewRoom(Transform _newRoom)
    {
        curposX = _newRoom.position.x;
    }
}
