using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField]
    private AudioMixer m_audioMixer;

    float m_stepSize = 2f;

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
        
        PrintVolume(m_master, m_masterVolume);
        PrintVolume(m_music, m_musicVolume);
        PrintVolume(m_SFX, m_SFXVolume);
    }

    private void PrintVolume(TextMeshProUGUI uiElement, float volume)
    {
        int perc = (int)(((volume + 40f) / 60f) * 100);
        uiElement.text = perc.ToString() + '%';
    }
    
    public void ChangeMusicVolume(bool increase)
    {
        if (increase)
            m_musicVolume += m_stepSize;
        else
            m_musicVolume -= m_stepSize;

        m_musicVolume = Mathf.Clamp(m_musicVolume, -40f, 20f);
        m_audioMixer.SetFloat("MusicParam", m_musicVolume);
        PrintVolume(m_music, m_musicVolume);
    }

    public void ChangeSFXVolume(bool increase)
    {
        if (increase)
            m_SFXVolume += m_stepSize;
        else
            m_SFXVolume -= m_stepSize;
        
        m_SFXVolume = Mathf.Clamp(m_SFXVolume, -40f, 20f);
        m_audioMixer.SetFloat("SFXParam", m_SFXVolume);
        PrintVolume(m_SFX, m_SFXVolume);
    }

    public void ChangeMasterVolume(bool increase)
    {
        if (increase)
            m_masterVolume += m_stepSize;
        else
            m_masterVolume -= m_stepSize;

        m_masterVolume = Mathf.Clamp(m_masterVolume, -40f, 20f);
        m_audioMixer.SetFloat("MasterParam", m_masterVolume);
        PrintVolume(m_master, m_masterVolume);
    }

}
