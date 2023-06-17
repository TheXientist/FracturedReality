using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicFader : MonoBehaviour
{
    public AudioSource audisource;
    private float fadeTimer;
    private float currentvolume;
    public float m_startVolume = 0.5f;
    public float fadeDuration = 1f;

    private bool m_bossIsAlive = true;

    private AudioClip loopableAudioClip;

    public AudioClip postFightMusic;

    private void Start()
    {
        audisource = GetComponent<AudioSource>();

        fadeTimer = fadeDuration;
    }

    private void Update()
    {
        if (!audisource.isPlaying && m_bossIsAlive)
        {
            audisource.loop = true;
            audisource.clip = loopableAudioClip;
            audisource.Play();
        }

        if (!audisource.isPlaying && !m_bossIsAlive)
        {
            audisource.loop = true;
            audisource.clip = postFightMusic;
            audisource.Play();
        }
    }

    public IEnumerator FadeMusic(AudioClip musicClip, bool loopable)
    {
        yield return FadeDown();

        audisource.loop = loopable;
        audisource.clip = musicClip;
        audisource.Play();

        yield return FadeUp(m_startVolume);

        yield return null;
    }

    public IEnumerator FadeMusic(AudioClip musicClip, AudioClip preMusicClip)
    {
        yield return FadeDown();

        if(preMusicClip != null )
        {
            audisource.loop = false;
            audisource.clip = preMusicClip;
            audisource.Play();
            loopableAudioClip = musicClip;
        }
        else
        {
            audisource.loop = true;
            audisource.clip = musicClip;
            audisource.Play();
        }

        yield return FadeUp(m_startVolume);

        yield return null;
    }



    public IEnumerator FadeDown()
    {
        currentvolume = audisource.volume;
        fadeTimer = fadeDuration;
        while (audisource.volume > 0)
        {
            if (fadeTimer > 0)
            {

                fadeTimer -= Time.deltaTime;

                float newVolume = Mathf.Lerp(0, currentvolume, fadeTimer / fadeDuration);

                audisource.volume = newVolume;
            }

            yield return null;
        }
        yield return null;
    }

    public IEnumerator FadeUp(float targetVolume)
    {
        currentvolume = audisource.volume;
        fadeTimer = fadeDuration;
        while (audisource.volume < targetVolume)
        {
            if (fadeTimer > 0)
            {
                fadeTimer -= Time.deltaTime;

                float newVolume = Mathf.Lerp(targetVolume, currentvolume, fadeTimer / fadeDuration);

                audisource.volume = newVolume;
            }

            yield return null;
        }
        yield return null;
    }

    public IEnumerator PlayDeathSound(AudioClip deathSound)
    {
        m_bossIsAlive = false;
        yield return FadeDown();

        audisource.loop = false;
        audisource.clip = deathSound;
        audisource.Play();

        yield return FadeUp(m_startVolume);

        yield return null;
    }

}
