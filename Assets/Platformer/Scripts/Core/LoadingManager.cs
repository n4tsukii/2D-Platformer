using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class LoadingManager : MonoBehaviour
    {
        public static LoadingManager instance { get; private set; }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;

            }
            else if (instance != null && instance != this)
                Destroy(gameObject);
        }

        public void LoadCurrentLevel()
        {
            int currentLevel = PlayerPrefs.GetInt("currentLevel", 1);
            SceneManager.LoadScene(currentLevel);
        }
        public void Restart()
        {
        }
    }
}
