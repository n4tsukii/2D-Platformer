using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer
{
    public class ExpeditionStage : MonoBehaviour
    {
        public string title;
        public string subtitle;
        public Color accent = Color.cyan;
        public Transform player;
        private bool complete;
        private float started;
        private GUIStyle heading, detail, button;

        private void Start() { started = Time.unscaledTime; }

        public void Complete()
        {
            complete = true;
            Time.timeScale = 0;
        }

        private void OnGUI()
        {
            if (heading == null)
            {
                heading = new GUIStyle(GUI.skin.label) { fontSize = 26, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
                detail = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                button = new GUIStyle(GUI.skin.button) { fontSize = 18 };
            }
            float w = Mathf.Min(600, Screen.width - 40);
            float x = (Screen.width - w) / 2;
            if (complete)
            {
                GUI.Box(new Rect(x, Screen.height / 2 - 120, w, 250), GUIContent.none);
                GUI.color = accent;
                GUI.Label(new Rect(x, Screen.height / 2 - 100, w, 45), "THE FORTRESS IS BEHIND YOU", heading);
                GUI.color = Color.white;
                GUI.Label(new Rect(x + 20, Screen.height / 2 - 45, w - 40, 55), "All three expeditions complete.\nYou made it out.", detail);
                if (GUI.Button(new Rect(x + 60, Screen.height / 2 + 35, w - 120, 45), "RETURN TO MENU", button))
                { Time.timeScale = 1; SceneManager.LoadScene("Main Menu"); }
            }
            else if (Time.unscaledTime - started < 6)
            {
                GUI.color = accent;
                GUI.Label(new Rect(x, 32, w, 40), title, heading);
                GUI.color = Color.white;
                GUI.Label(new Rect(x, 75, w, 45), subtitle, detail);
            }
        }

        private void OnDestroy() { if (complete) Time.timeScale = 1; }
    }
}
