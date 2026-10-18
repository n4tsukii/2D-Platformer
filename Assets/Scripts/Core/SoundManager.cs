using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance {get; private set;}
    private AudioSource soundSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        soundSource = GetComponent<AudioSource>();
        musicSource = transform.GetChild(0).GetComponent<AudioSource>();
        ChangeSoundVolume(0);
        ChangeMusicVolume(0);
    }

    public void PlaySound(AudioClip _sound)
    {
        if (_sound != null)
            soundSource.PlayOneShot(_sound);
    }

    public void ChangeSoundVolume(float change)
    {
        ChangeSourceVolume(1, "soundVolume", change, soundSource);
    }

    public void ChangeMusicVolume(float change)
    {
        ChangeSourceVolume(0.3f, "musicVolume", change, musicSource);
    }

    private void ChangeSourceVolume(float baseVolume, string volumeName, float change, AudioSource source)
    {
        float currentVolume = Mathf.Round((PlayerPrefs.GetFloat(volumeName, 1) + change) * 100) / 100;
        if (currentVolume > 1)
            currentVolume = 0;
        else if (currentVolume < 0)
            currentVolume = 1;

        source.volume = currentVolume * baseVolume;
        PlayerPrefs.SetFloat(volumeName, currentVolume);
        if (change != 0)
            PlayerPrefs.Save();
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
