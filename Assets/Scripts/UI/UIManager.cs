using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private AudioClip gameOverSound;
    [SerializeField] private AudioClip interactSound;
    [SerializeField] private GameObject pauseScreen;
    [SerializeField] private string mainMenuScene = "Main Menu";
    [SerializeField] private string firstLevelScene = "Level 1";

    public bool IsGameOver { get; private set; }
    public bool IsPaused { get; private set; }

    private void Awake()
    {
        IsGameOver = false;
        IsPaused = false;
        Time.timeScale = 1;
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);
        if (pauseScreen != null)
            pauseScreen.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameOver && pauseScreen != null)
            PauseGame(!IsPaused);
    }

    public void GameOver()
    {
        if (IsGameOver) return;

        IsGameOver = true;
        IsPaused = false;
        if (pauseScreen != null)
            pauseScreen.SetActive(false);
        gameOverScreen.SetActive(true);
        SoundManager.instance.PlaySound(gameOverSound);
        Time.timeScale = 0;
    }

    public void PauseGame(bool status)
    {
        if (IsGameOver || pauseScreen == null) return;

        IsPaused = status;
        pauseScreen.SetActive(status);
        Time.timeScale = status ? 0 : 1;
        PlayInteractionSound();
    }

    public void SoundVolume()
    {
        SoundManager.instance.ChangeSoundVolume(0.2f);
        PlayInteractionSound();
    }

    public void MusicVolume()
    {
        SoundManager.instance.ChangeMusicVolume(0.2f);
        PlayInteractionSound();
    }

    public void Restart()
    {
        PlayInteractionSound();
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().path);
    }

    public void MainMenu()
    {
        PlayInteractionSound();
        Time.timeScale = 1;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void StartGame()
    {
        PlayInteractionSound();
        Time.timeScale = 1;
        SceneManager.LoadScene(firstLevelScene);
    }

    public void Quit()
    {
        PlayInteractionSound();
        Time.timeScale = 1;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void PlayInteractionSound()
    {
        if (SoundManager.instance != null && interactSound != null)
            SoundManager.instance.PlaySound(interactSound);
    }

    private void OnDestroy()
    {
        if (IsGameOver || IsPaused)
            Time.timeScale = 1;
    }
}
