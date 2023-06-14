using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIReloader : MonoBehaviour
{
    private PauseMenu pauseMenu;
    private FadeBlackScreen fadeBlackScreen;


    void Start()
    {
        ReloadBlackScreen();
        ReloadPauseScreen();  
    }



    public void ReloadBlackScreen()
    {
        FadeBlackScreen fadeBlackScreen = FindObjectOfType<FadeBlackScreen>();
        SpaceshipController.Instance.GetComponent<Player>().m_fadeBlackScreen = fadeBlackScreen;
        StartCoroutine(fadeBlackScreen.FadeIn(0.01f));
    }

    public void ReloadPauseScreen()
    {
        pauseMenu = FindObjectOfType<PauseMenu>();
        StartCoroutine(pauseMenu.PauseOnReload(1f)); 
    }
}
