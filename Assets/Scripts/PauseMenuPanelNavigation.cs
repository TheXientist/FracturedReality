using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuPanelNavigation : MonoBehaviour
{
    [SerializeField] private GameObject m_ControlsPanel;
    [SerializeField] private GameObject m_RepositionPanel;

    public void CloseControlsOpenReposition()
    {
        m_ControlsPanel.SetActive(false);
        m_RepositionPanel.SetActive(true);
    }

    public void CloseRepositionOpenControls()
    {
        m_ControlsPanel.SetActive(true);
        m_RepositionPanel.SetActive(false);
    }


}
