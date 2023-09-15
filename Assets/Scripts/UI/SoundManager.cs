using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer m_audioMixer;

    float m_stepSize = 1f;

    float m_masterVolume = 0f;
    float m_musicVolume = 0f;
    float m_SFXVolume = 0f;

    public TextMeshProUGUI m_music;
    public TextMeshProUGUI m_SFX;
    public TextMeshProUGUI m_master;

    // Start is called before the first frame update
    void Start()   
    {
        m_audioMixer.GetFloat("MusicParam", out m_musicVolume);
        m_audioMixer.GetFloat("SFXParam", out m_SFXVolume);
        m_audioMixer.GetFloat("MasterParam", out m_SFXVolume);
    }

    public void ChangeMusicVolume(bool increase)
    {
        if (increase)
        {
            m_musicVolume += m_stepSize;

            m_audioMixer.SetFloat("MusicParam", m_musicVolume);
        }
        else
        {
            m_musicVolume -= m_stepSize;
            m_audioMixer.SetFloat("MusicParam", m_musicVolume);
        }

        m_music.text = m_musicVolume.ToString();

    }

    public void ChangeSFXVolume(bool increase)
    {
        if (increase)
        {
            m_SFXVolume += m_stepSize;

            m_audioMixer.SetFloat("SFXParam", m_SFXVolume);
        }
        else
        {
            m_SFXVolume -= m_stepSize;
            m_audioMixer.SetFloat("SFXParam", m_SFXVolume);
        }

        m_SFX.text = m_SFXVolume.ToString();

    }

    public void ChangeMasterVolume(bool increase)
    {
        if (increase)
        {
            m_masterVolume += m_stepSize;

            m_audioMixer.SetFloat("MasterParam", m_masterVolume);
        }
        else
        {
            m_masterVolume -= m_stepSize;
            m_audioMixer.SetFloat("MasterParam", m_masterVolume);
        }

        m_master.text = m_masterVolume.ToString();

    }

}
