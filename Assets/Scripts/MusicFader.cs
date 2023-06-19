using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class MusicFader : MonoBehaviour
{
    [SerializeField] private AudioSource trackA, trackB, atmo;
    [SerializeField]
    private float targetVolume = 0.5f;
    public float fadeDuration = 1f;

    private void Start()
    {
        trackA.volume = targetVolume;
        trackB.volume = targetVolume;
    }

    public void PlayMusic(AudioClip clip, bool fade, bool loop, Action callback = null) =>
        StartCoroutine(PlayMusicCoroutine(clip, fade, loop, callback));
    
    public IEnumerator PlayMusicCoroutine(AudioClip clip, bool fade, bool loop, Action callback = null)
    {
        if (trackA.clip.Equals(clip)) yield break; // Clip is already playing
        
        // Current track is playing in A,
        // new clip will fade in in B
        trackB.clip = clip;
        trackB.loop = loop;
        trackB.volume = 0f;
        trackA.volume = targetVolume;
        trackB.Play();

        while (fade && trackB.volume <= targetVolume)
        {
            trackB.volume += targetVolume / (10f * fadeDuration);
            trackA.volume -= targetVolume / (10f * fadeDuration);
            yield return new WaitForSeconds(1/10f);
        }
        
        // Set current track to A again
        trackA.clip = trackB.clip;
        trackA.volume = targetVolume;
        trackA.time = trackB.time;
        trackA.loop = loop;
        trackA.Play();
        trackB.Stop();
        
        // If it's a non-looping track, inform caller that it has ended
        if (loop) yield break;
        
        yield return new WaitForSeconds(clip.length - fadeDuration);
        callback?.Invoke();
    }

    public IEnumerator FadeOut(float duration)
    {
        while (trackA.volume > 0f)
        {
            trackA.volume -= targetVolume / (10f * duration);
            yield return new WaitForSeconds(1/10f);
        }
        trackA.Stop();
    }

}
