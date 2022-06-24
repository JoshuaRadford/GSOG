using UnityEngine;

public class AudioManager : Singleton<AudioManager> {
    // Start is called before the first frame update
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public static void PlayAudioClip(AudioClip clip, AudioSource source) {
        if (clip == null) return;

        source.PlayOneShot(clip);
    }

    public static void PlayAudioClip(AudioClip[] clips, AudioSource source) {
        if (clips == null || clips.Length <= 0) return;

        AudioClip clip = clips[UnityEngine.Random.Range(0, clips.Length)];
        source.PlayOneShot(clip);
    }
}
