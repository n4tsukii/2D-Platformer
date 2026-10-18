using UnityEngine;
using UnityEngine.UI;

public class VolumeText : MonoBehaviour
{
    [SerializeField] private string volumeName;
    [SerializeField] private string textIntro;
    private Text txt;

    private void Awake()
    {
        txt = GetComponent<Text>();
    }

    private void OnEnable()
    {
        UpdateVolume();
    }

    private void Update()
    {
        UpdateVolume();
    }

    private void UpdateVolume()
    {
        if (txt == null)
            txt = GetComponent<Text>();
        txt.text = textIntro + Mathf.RoundToInt(PlayerPrefs.GetFloat(volumeName, 1) * 100) + "%";
    }
}
