using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class ExpeditionLaunch : MonoBehaviour
    {
        public string sceneName;
        public void Launch() { Time.timeScale = 1; SceneManager.LoadScene(sceneName); }
    }
}
