    using UnityEngine;

    public class Audio : MonoBehaviour
    {
        public AudioClip myAudioClip; // Assign your AudioClip in the Inspector

       void Start()
{
    var source = GetComponent<AudioSource>();
    Debug.Log("Audio Clip: " + source.clip);  // چک کن ببینی clip هست یا نه
    source.Play();
}
    }