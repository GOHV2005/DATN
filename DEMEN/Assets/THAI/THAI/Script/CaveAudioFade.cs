using UnityEngine;
using System.Collections;

public class CaveAudioFade : MonoBehaviour
{
    public AudioSource[] caveSounds;
    public float fadeDuration = 2f;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (AudioSource source in caveSounds)
                StartCoroutine(FadeIn(source));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            foreach (AudioSource source in caveSounds)
                StartCoroutine(FadeOut(source));
        }
    }

    IEnumerator FadeIn(AudioSource source)
    {
        float startVolume = source.volume;
        float time = 0;
        while (time < fadeDuration)
        {
            source.volume = Mathf.Lerp(startVolume, 1f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = 1f;
    }

    IEnumerator FadeOut(AudioSource source)
    {
        float startVolume = source.volume;
        float time = 0;
        while (time < fadeDuration)
        {
            source.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            time += Time.deltaTime;
            yield return null;
        }
        source.volume = 0f;
    }
}
