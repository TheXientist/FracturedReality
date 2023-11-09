using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

public class FadeBlackScreen : MonoBehaviour
{
    [SerializeField]
    public Image m_blackScreen;

    [SerializeField]
    public GameObject m_deathText;

    private void Start()
    {
        if(m_blackScreen == null)
        {
            m_blackScreen = GetComponent<Image>();
        }

        //m_blackScreen.enabled = false;
    }

    public IEnumerator FadeOut(float fadeAmount)
    {
        float tempFade = 0;
        m_blackScreen.enabled = true;
        
        while(m_blackScreen.color.a < 1f)
        {
            tempFade += fadeAmount;
            m_blackScreen.color = new Color(0, 0, 0, tempFade);
            yield return null;
        }
        m_deathText.SetActive(true);
        yield return new WaitForSeconds(3f);

        yield return null;    
    }

    public IEnumerator FadeIn(float fadeAmount)
    {
        m_deathText.SetActive(false);

        float tempFade = 1;

        m_blackScreen.color = new Color(0, 0, 0, 1);

        while (m_blackScreen.color.a > 0f)
        {
            tempFade -= fadeAmount;
            m_blackScreen.color = new Color(0, 0, 0, tempFade);

            yield return null;
        }

        m_blackScreen.enabled = false;

        yield return null;
    }

    public IEnumerator FadeAlpha(float targetAlpha , float fadeDuration)
    {
        Color startColor = m_blackScreen.color;//targetMaterial.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, targetAlpha);

        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            m_blackScreen.color = Color.Lerp(startColor, endColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        m_blackScreen.color = endColor;
    }

}
