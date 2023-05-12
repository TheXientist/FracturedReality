using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Valve.VR;

public class Player : MonoBehaviour
{
    private float playerCurrentHealth = 0;

    public float PlayerCurrentHealth
    {
        get => playerCurrentHealth;
        set
        {
            playerCurrentHealth = value;
            healthDisplay.text = "Ship Health:\n" + playerCurrentHealth;
        }
    }
    public float playerMaxHealth = 100;

    [SerializeField]
    private Transform m_bulletSpawnpoint;

    [SerializeField]
    private GameObject m_BossObject;

    [SerializeField]
    private GameObject m_currentBullet;

    [SerializeField] private TextMeshProUGUI firerateDisplay, healthDisplay;

    public float fireRate = 1f;

    private float nextFireTime = 0f;

    private float NextFireTime
    {
        get => nextFireTime;
        set
        {
            nextFireTime = value;
            switch (nextFireTime)
            {
                case > 0f:
                    firerateDisplay.text = "Blaster cooling\ndown...";
                    firerateDisplay.color = new Color(255, 180, 0);
                    break;
                default:
                    firerateDisplay.text = "Blaster ready";
                    firerateDisplay.color = Color.cyan;
                    break;
            }
        }
    }

    [SerializeField]
    private float projectileThreshold = 1f;

    private SteamVR_Action_Boolean fireAction;

    void Start()
    {
        PlayerCurrentHealth = playerMaxHealth;
        fireAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "fire");
        fireAction.AddOnStateDownListener(OnFire, SteamVR_Input_Sources.Any);
    }

    // Update is called once per frame
    void Update()
    {
        NextFireTime -= Time.deltaTime;
        if(playerCurrentHealth <= 0)
        {
            DestroySelf();
        }
    }

    public void TakeDamage(float damage)
    {
        PlayerCurrentHealth -= damage;
    }

    public void DestroySelf()
    {
        gameObject.SetActive(false);
    }

    public void ShootBullet()
    {
        if(nextFireTime <= 0f && m_BossObject != null)
        {
            Vector3 direction = (m_BossObject.transform.position - transform.position).normalized;

            GameObject temp = Instantiate(m_currentBullet, m_bulletSpawnpoint.position + (m_BossObject.transform.position - m_bulletSpawnpoint.position).normalized * projectileThreshold, Quaternion.LookRotation(direction, Vector3.up));

            temp.GetComponent<AmmunationModule>().direction = direction;

            NextFireTime = 1f / fireRate;
        }


    }

    public void OnFire(SteamVR_Action_Boolean action, SteamVR_Input_Sources source)
    {
        ShootBullet();
    }
}
