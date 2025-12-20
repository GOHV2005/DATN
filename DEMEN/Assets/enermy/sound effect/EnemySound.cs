using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemySound : MonoBehaviour
{
    [Header("Âm thanh")]
    public AudioClip idleSound;   // Ví dụ: ong bay
    public float volume = 1f;

    [Header("Khoảng cách nghe")]
    public float minDistance = 1.5f;   // Gần nhất
    public float maxDistance = 8f;     // Xa nhất

    private AudioSource audioSource;
    private Transform player;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        audioSource.clip = idleSound;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.volume = volume;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    void OnEnable()
    {
        if (idleSound != null)
            audioSource.Play();
    }

    void OnDisable()
    {
        audioSource.Stop();
    }
}
