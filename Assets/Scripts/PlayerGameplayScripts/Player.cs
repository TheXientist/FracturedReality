using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float playerCurrentHealth = 0;
    public float playerMaxHealth = 100;

    [SerializeField]
    private Transform m_bulletSpawnpoint;

    [SerializeField]
    private GameObject m_BossObject;

    [SerializeField]
    private GameObject m_currentBullet;

    public float fireRate = 1f;

    private float nextFireTime = 0f;

    void Start()
    {
        playerCurrentHealth = playerMaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerCurrentHealth <= 0)
        {
            DestroySelf();
        }
    }

    public void TakeDamage(float damage)
    {
        playerCurrentHealth -= damage;
    }

    public void DestroySelf()
    {
        gameObject.SetActive(false);
    }

    public void ShootBullet()
    {
        if(Time.time >= nextFireTime && m_BossObject != null)
        {
            Vector3 direction = (m_BossObject.transform.position - transform.position).normalized;

            GameObject temp = Instantiate(m_currentBullet, m_bulletSpawnpoint.position + (m_BossObject.transform.position - m_bulletSpawnpoint.position).normalized, Quaternion.LookRotation(direction, Vector3.up));

            temp.GetComponent<AmmunationModule>().direction = direction;

            nextFireTime = Time.time + 1f / fireRate;
        }


    }
}
