using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;
    private AudioSource audioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Sahne deðiþse bile müzik devam etsin
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }

    public void FadeOut(float duration)
    {
        StartCoroutine(FadeMusic(0f, duration));
    }

    public void FadeIn(float targetVolume, float duration)
    {
        StartCoroutine(FadeMusic(targetVolume, duration));
    }

    private System.Collections.IEnumerator FadeMusic(float targetVolume, float duration)
    {
        float start = audioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            audioSource.volume = Mathf.Lerp(start, targetVolume, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
