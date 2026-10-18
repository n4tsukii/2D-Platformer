using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class LevelExit : MonoBehaviour
    {
        [SerializeField] private string nextScene;
        private bool loading;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Enter(collision);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            Enter(collision.collider);
        }

        private void Enter(Collider2D collision)
        {
            if (loading || !collision.CompareTag("Player")) return;

            loading = true;
            Time.timeScale = 1;
            SceneManager.LoadScene(nextScene);
        }
    }
}
