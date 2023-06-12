using System;
using System.Collections;
using System.Collections.Generic;
using GD.MinMaxSlider;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;

public class Player : MonoBehaviour, IDamageable
{
    private float playerCurrentHealth = 0;

    public static event Action OnPlayerDeath;

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

    public GameObject m_BossObject;

    [SerializeField]
    private AmmunationModule m_currentBullet;

    private TextMeshProUGUI healthDisplay, heatText;
    private Image heatDisplay, chargeDisplay, chargeMark;

    public float fireRate = 1f;

    private float nextFireTime = 0f;

    public FadeBlackScreen m_fadeBlackScreen;

    private bool animatingDamage;

    private float NextFireTime
    {
        get => nextFireTime;
        set => nextFireTime = value;
    }

    [SerializeField]
    private float projectileThreshold = 1f;

    private SteamVR_Action_Boolean fireAction;

    [SerializeField, MinMaxSlider(0f, 5f)] private Vector2 chargeTime;
    private float minChargeTime => chargeTime.x;
    private float maxChargeTime => chargeTime.y;
    private bool buttonDown;
    private float lastBtnDownTime;
    private float lastHeatAddTime;

    [Header("Heat")] [SerializeField] private int maxHeat;
    [SerializeField] private int heatPerShot, heatPerOvercharge;
    [SerializeField, Tooltip("Heat loss per 10ms when cooling down")]
    private int cooldownRate;
    [SerializeField] private float cooldownAfterSeconds;
    private int currentHeat;

    private int CurrentHeat
    {
        get => currentHeat;
        set
        {
            currentHeat = value;
            heatDisplay.fillAmount = (float)currentHeat / maxHeat;
        }
    }
    public bool IsOverheated { get; private set; }

    void Start()
    {
        healthDisplay = GameObject.FindWithTag("ShipHealthDisplay").GetComponent<TextMeshProUGUI>();
        heatText = GameObject.FindWithTag("WeaponStateDisplay").GetComponent<TextMeshProUGUI>();
        heatDisplay = GameObject.FindWithTag("HeatDisplay").GetComponent<Image>();
        chargeDisplay = GameObject.FindWithTag("ChargeDisplay").GetComponent<Image>();
        chargeMark = GameObject.FindWithTag("ChargeMark").GetComponent<Image>();
        PlayerCurrentHealth = playerMaxHealth;
        fireAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "fire");
        fireAction.AddOnStateDownListener(OnFireVR, SteamVR_Input_Sources.Any);

        chargeMark.fillAmount = 1f - (minChargeTime / maxChargeTime);
    }

    // Update is called once per frame
    void Update()
    {
        NextFireTime -= Time.deltaTime;

        if (lastHeatAddTime > 0f && Time.time - lastHeatAddTime > cooldownAfterSeconds)
        {
            StartCoroutine(CooldownRoutine());
        }
        
        UpdateChargeDisplay();
    }

    private void UpdateChargeDisplay()
    {
        float chargePercentage = chargeDisplay.fillAmount;
        if (buttonDown)
        {
            float btnDownTime = Time.time - lastBtnDownTime;
            chargePercentage = btnDownTime / maxChargeTime;
            if (chargePercentage > 1f)
                chargeDisplay.color = Color.red;
            else if (btnDownTime >= minChargeTime)
                chargeDisplay.color = Color.green;
        }
        else if (chargePercentage > 0f)
        {
            chargePercentage -= 2f * Time.deltaTime;
            chargeDisplay.color = Color.white;
        }
        
        chargeDisplay.fillAmount = chargePercentage;
    }

    public event Action OnDamaged;

    public void TakeDamage(float damage)
    {
        PlayerCurrentHealth -= damage;
        
        if (!animatingDamage)
            StartCoroutine(FlashRed());
        
        if (playerCurrentHealth <= 0)
        {
            PlayerCurrentHealth = 0;
            StartCoroutine( DestroySelf());
            OnPlayerDeath?.Invoke();
        }
        OnDamaged?.Invoke();
    }

    private IEnumerator FlashRed()
    {
        animatingDamage = true;

        if (SpaceshipController.VR)
        {
            SteamVR_Fade.Start(new Color(1f, 0f, 0f, 0.1f), 0.25f);
            yield return new WaitForSeconds(0.25f);
            SteamVR_Fade.Start(Color.clear, 0.5f);
        }
        else
        {
            // TODO: non-VR alternative
        }
        
        animatingDamage = false;
    }

    public IEnumerator DestroySelf()
    {
        //tell boss the player is dead
        m_BossObject.GetComponent<BossAI>().StopBossFight();
        yield return m_fadeBlackScreen.FadeOut(0.01f);

        SceneManager.LoadScene(0);
    }

    public void ShootBullet()
    {
        if(nextFireTime <= 0f && m_BossObject != null)
        {
            Vector3 direction = (m_BossObject.transform.position - transform.position).normalized;

            AmmunationModule temp = Instantiate(m_currentBullet, m_bulletSpawnpoint.position + (m_BossObject.transform.position - m_bulletSpawnpoint.position).normalized * projectileThreshold, Quaternion.LookRotation(direction, Vector3.up));

            temp.direction = direction;

            NextFireTime = 1f / fireRate;
        }


    }

    // Callback func for SteamVR input
    public void OnFireVR(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) => OnFireInput(action.stateDown);

    private void OnFireInput(bool btnDown)
    {
        if (IsOverheated)
        {
            // TODO: SFX & visual feedback
            lastBtnDownTime = -1f; // mark invalid stateDown
            return;
        }
        
        buttonDown = btnDown;
        
        // Button down
        if (btnDown || lastBtnDownTime < 0f)
        {
            lastBtnDownTime = Time.time;
            return;
        }
        
        // Button release
        float btnDownTime = Time.time - lastBtnDownTime;
        if (btnDownTime >= minChargeTime && btnDownTime <= maxChargeTime)
        {
            // Perfectly charged attack (2 bullets, no heat)
            ShootBullet();
            ShootBullet();
            // TODO: SFX, UI display
        } else if (btnDownTime < minChargeTime)
        {
            // Regular attack
            ShootBullet();
            AddHeat(heatPerShot);
        }
        else
        {
            // Overheat
            AddHeat(heatPerOvercharge);
        }
    }

    private void AddHeat(int amount)
    {
        CurrentHeat += amount;
        if (currentHeat >= maxHeat)
        {
            CurrentHeat = maxHeat;
            StartCoroutine(OverheatRoutine());
        }
        lastHeatAddTime = Time.time;
    }

    private IEnumerator OverheatRoutine()
    {
        // TODO: SFX
        IsOverheated = true;
        heatDisplay.color = Color.red;
        heatText.text = "Overheated!";
        heatText.color = Color.red;
        
        yield return new WaitForSeconds(3f);
        while (currentHeat > 0)
        {
            CurrentHeat -= cooldownRate;
            yield return new WaitForSeconds(0.1f);
        }
        IsOverheated = false;
        heatDisplay.color = new Color(1f, .7f, .2f);
        heatText.text = "Blaster Heat";
        heatText.color = new Color(1f, .7f, .2f);
    }

    private IEnumerator CooldownRoutine()
    {
        lastHeatAddTime = -1f;
        while (currentHeat > 0 && lastHeatAddTime < 0f)
        {
            CurrentHeat -= cooldownRate;
            yield return new WaitForSeconds(0.1f);
        }
    }

    // Callback func for Unity InputSystem (non-VR)
    public void OnFire(InputValue v) => OnFireInput(v.isPressed);
}
