using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class UIManager : MonoBehaviour
    {
        [Header ("Game Over")]
        [SerializeField] private GameObject gameOverScreen;
        [SerializeField] private AudioClip gameOverSound;

        [Header("Pause")]
        [SerializeField] private GameObject pauseScreen;

        private void Awake()
        {
            Time.timeScale = 1;
            gameOverScreen.SetActive(false);
            pauseScreen.SetActive(false);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !gameOverScreen.activeSelf)
            {
                PauseGame(!pauseScreen.activeInHierarchy);
            }
        }

        public void GameOver()
        {
            pauseScreen.SetActive(false);
            gameOverScreen.SetActive(true);
            Time.timeScale = 0;
            SoundManager.instance.PlaySound(gameOverSound);
        }

        public void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void MainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Main Menu");
        }

        public void Quit()
        {
            Time.timeScale = 1;
            Application.Quit();

    #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
    #endif
        }

        public void PauseGame(bool status)
        {
            if (gameOverScreen.activeSelf) return;
            pauseScreen.SetActive(status);

            if (status)
                Time.timeScale = 0;
            else
                Time.timeScale = 1;
        }
        public void SoundVolume()
        {
            SoundManager.instance.ChangeSoundVolume(0.2f);
        }
        public void MusicVolume()
        {
            SoundManager.instance.ChangeMusicVolume(0.2f);
        }

        private void OnDestroy()
        {
            Time.timeScale = 1;
        }
    }
}
