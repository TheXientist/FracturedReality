using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuPanelNavigation : MonoBehaviour
{
    [SerializeField] private GameObject m_ControlsPanel;
    [SerializeField] private GameObject m_RepositionPanel;
    [SerializeField] private GameObject m_SoundPanel;

    public void CloseControlsOpenReposition()
    {
        if (!PauseMenu.Paused) return;
        m_ControlsPanel.SetActive(false);
        m_RepositionPanel.SetActive(true);
    }

    public void CloseRepositionOpenControls()
    {
        if (!PauseMenu.Paused) return;
        m_ControlsPanel.SetActive(true);
        m_RepositionPanel.SetActive(false);
    }

    public void CloseControlsOpenSound()
    {
        if (!PauseMenu.Paused) return;
        m_ControlsPanel.SetActive(false);
        m_SoundPanel.SetActive(true);
    }

    public void CloseSoundOpenControls()
    {
        if (!PauseMenu.Paused) return;
        m_ControlsPanel.SetActive(true);
        m_SoundPanel.SetActive(false);
    }


}
