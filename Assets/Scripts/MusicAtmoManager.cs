using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicAtmoManager : MonoBehaviour {
    public AudioClip[] music;
    public AudioClip wind;

    AudioSource musicSource;
    AudioSource windSource;

    //---------------------------

    public float delayBetweenTracks = 30.0f;
    public float windVolume = 0.3f;
    public float musicVolume = 0.3f;

    //---------------------------
    
    void Start() {
        // Creates the audio sources
        windSource = gameObject.AddComponent<AudioSource>();
        musicSource = gameObject.AddComponent<AudioSource>();

        // Plays the wind sound, also starts the music coroutine
        windSource.clip = wind;
        windSource.loop = true;
        windSource.volume = windVolume;
        windSource.Play();

        musicSource.volume = musicVolume;
        StartCoroutine(PlayMusic());
    }

    //---------------------------

    IEnumerator PlayMusic() {
        while (true) {
            // Select a random track from the music clips
            AudioClip selectedMusic = music[Random.Range(0, music.Length)];
            float musicLength = selectedMusic.length;

            // Plays the clip
            musicSource.PlayOneShot(selectedMusic);

            yield return new WaitForSeconds(musicLength + delayBetweenTracks);
        }
    }
}
