using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MusicPlayer : MonoBehaviour
{
    private static MusicPlayer instance;
    private AudioSource audioSource;

void Start() {
    Debug.Log("AudioSource exists: " + (GetComponent<AudioSource>() != null));
    Debug.Log("Clip assigned: " + (GetComponent<AudioSource>().clip != null));
    Debug.Log("Volume: " + GetComponent<AudioSource>().volume);
}
    void Awake()
{
    if (instance == null)
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.5f;  // <-- ADD THIS
        audioSource.Play();
        
        Debug.Log("Playing with volume: " + audioSource.volume);
    }
    else
    {
        Destroy(gameObject);
    }
}
}