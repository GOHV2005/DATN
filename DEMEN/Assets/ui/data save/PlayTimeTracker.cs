using UnityEngine;

public class PlayTimeTracker : MonoBehaviour
{
    public static PlayTimeTracker Instance;
    private float playTime;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        playTime += Time.deltaTime;
    }

    public float GetPlayTime() => playTime;
    public void ResetTime() => playTime = 0f;
}
