using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IDamageable
{
    public float obstacleMaxHealth = 100;
    public float obstacleCurrentHealth = 0;

    public Material m_obstacleMaterial;

    public float colorTransitionDuration = 2f;
    private float m_currentTransitionTime = 0f;

    private bool m_isWhite = true;

    public int m_DamageValue;

    // Start is called before the first frame update
    void Start()
    {
        obstacleCurrentHealth = obstacleMaxHealth;
        m_obstacleMaterial = new Material(GetComponent<Renderer>().material);
        GetComponent<Renderer>().material = m_obstacleMaterial;
    }

    // Update is called once per frame
    void Update()
    {
        if(obstacleCurrentHealth <= 0)
        {
            DestroySelf();
        }

        if (!m_isWhite)
        {
            m_isWhite = true;
            StartCoroutine(FadeColorBack());
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public event Action OnDamaged;

    public void TakeDamage(float damage)
    {
        obstacleCurrentHealth -= damage;
        ObstacleShaderDamage();
        OnDamaged?.Invoke();
    }

    public IEnumerator FadeColor()
    {
        m_isWhite = false;
        print("fade is called");
        while(m_currentTransitionTime < colorTransitionDuration)
        {
            m_currentTransitionTime += Time.deltaTime;
            float normalizedTime = m_currentTransitionTime / colorTransitionDuration;
            m_obstacleMaterial.color = Color.Lerp(Color.white, Color.cyan, normalizedTime);
            yield return null;
        }


        m_currentTransitionTime = 0;
        yield return null;
    }

    public IEnumerator FadeColorBack()
    {
        yield return new WaitForSeconds(5f);

        while (m_currentTransitionTime < colorTransitionDuration)
        {
            m_currentTransitionTime += Time.deltaTime;
            float normalizedTime = m_currentTransitionTime / colorTransitionDuration;
            m_obstacleMaterial.color = Color.Lerp(Color.cyan, Color.white, normalizedTime);
            yield return null;
        }

        m_currentTransitionTime = 0;
        yield return null;
    }

    public void ObstacleShaderDamage()
    {
        m_obstacleMaterial.SetFloat("_BumpScale", 1 + ((obstacleMaxHealth-obstacleCurrentHealth)/obstacleMaxHealth) * 4f);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player")) other.gameObject.GetComponent<Player>().TakeDamage(m_DamageValue * other.gameObject.GetComponent<Rigidbody>().velocity.magnitude);
    }
}
