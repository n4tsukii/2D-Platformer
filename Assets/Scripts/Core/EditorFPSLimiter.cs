 using UnityEngine;

public class EditorFPSLimiter : MonoBehaviour
{
    [SerializeField] private int targetFPS = 60;

    void Awake()
    {
        // Enforce the limitation strictly when running inside the Unity Editor
        #if UNITY_EDITOR
        QualitySettings.vSyncCount = 0; // VSync must be turned off for targetFrameRate to work
        Application.targetFrameRate = targetFPS;
        #endif
    }
}