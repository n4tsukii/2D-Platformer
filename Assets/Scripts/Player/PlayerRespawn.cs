using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [SerializeField] private AudioClip checkpoint;
    private Transform currentCheckpoint;
    private Health playerHealth;
    private Rigidbody2D body;
    private UIManager uiManager;

    private void Awake()
    {
        playerHealth = GetComponent<Health>();
        body = GetComponent<Rigidbody2D>();
        uiManager = FindFirstObjectByType<UIManager>();
    }

    public void Respawn()
    {
        if (currentCheckpoint == null)
        {
            uiManager.GameOver();
            return;
        }

        transform.position = currentCheckpoint.position;
        body.linearVelocity = Vector2.zero;
        body.angularVelocity = 0;
        body.gravityScale = 7;
        playerHealth.Respawn();

        Transform room = currentCheckpoint.parent;
        if (room != null)
        {
            Camera.main.GetComponent<CameraController>().MoveToNewRoom(room);
            room.GetComponent<Room>().ActivateRoom(true);
        }
    }

    private void RespawnAfterDeath()
    {
        Respawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Checkpoint")) return;

        currentCheckpoint = collision.transform;
        SoundManager.instance.PlaySound(checkpoint);
        collision.enabled = false;
        collision.GetComponent<Animator>().SetTrigger("activate");
    }
}
