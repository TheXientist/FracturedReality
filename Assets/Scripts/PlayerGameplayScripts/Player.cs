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
            string text = "    Ship Health:\n";
            text += new string('|', (int)(playerCurrentHealth / playerMaxHealth * 30));
            healthDisplay.text = text;
        }
    }
    public float playerMaxHealth = 100;
    public float playerRegenSpeed = 50;

    [SerializeField]
    private Transform m_bulletSpawnpointLeft, m_bulletSpawnpointRight;
    private bool lastFiredRight;

    public GameObject m_BossObject;

    [SerializeField]
    private AmmunationModule m_currentBullet;

    private TextMeshProUGUI healthDisplay, heatDisplay;
    private SpaceshipController controller;

    public float fireRate = 1f;

    private float nextFireTime = 0f;

    public FadeBlackScreen m_fadeBlackScreen;

    private bool animatingDamage;

    private float NextFireTime
    {
        get => nextFireTime;
        set => nextFireTime = value;
    }

    private SteamVR_Action_Boolean fireAction, deflectAction;

    [SerializeField, Tooltip("The maximum charge time, after which the shot is overcharged (if chargeTimeWindow.y==1)")]
    private float chargeTime;
    [SerializeField, MinMaxSlider(0f, 1f), Tooltip("Timeframe (relative) for charging a perfect shot")]
    private Vector2 chargeTimeWindow;
    private float minChargeTime => chargeTime * chargeTimeWindow.x;
    private float maxChargeTime => chargeTime * chargeTimeWindow.y;
    private bool buttonDown;
    private float lastBtnDownTime;
    private float lastHeatAddTime;
    private float currentCharge;
    private float lastOverChargeTime;

    [Header("Heat")] [SerializeField] private int maxHeat;
    [SerializeField] private int heatPerShot, heatPerOvercharge;
    [SerializeField, Tooltip("Heat loss per 10ms when cooling down")]
    private int cooldownRate;
    public int cooldownRateOverheated;
    [SerializeField] private float cooldownAfterSeconds;
    [SerializeField] private int currentHeat;

    private int CurrentHeat
    {
        get => currentHeat;
        set
        {
            currentHeat = value;
            if (currentHeat < 0) currentHeat = 0;
            string text = IsOverheated ? "    Overheated!\n" : "    Blaster Heat:\n";
            text += new string('|', (int)(currentHeat / (float)maxHeat * 30));
            heatDisplay.text = text;
        }
    }
    public bool IsOverheated { get; private set; }

    [Header("EMP"), SerializeField] private float empCooldown;
    [SerializeField] private float empRange;
    private bool empReady = true;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Transform explosionSpawnPosition;

    private TextMeshProUGUI empText;
    private Image empDisplay;

    void Start()
    {
        healthDisplay = GameObject.FindWithTag("ShipHealthDisplay").GetComponent<TextMeshProUGUI>();
        heatDisplay = GameObject.FindWithTag("WeaponStateDisplay").GetComponent<TextMeshProUGUI>();
        empText = GameObject.FindWithTag("EMPText").GetComponent<TextMeshProUGUI>();
        empDisplay = GameObject.FindWithTag("EMPBorder").GetComponent<Image>();
        controller = GetComponent<SpaceshipController>();
        PlayerCurrentHealth = playerMaxHealth;
        fireAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "fire");
        fireAction.AddOnChangeListener(OnFireVR, SteamVR_Input_Sources.Any);
        deflectAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "deflect");
        deflectAction.AddOnStateDownListener(OnDeflectVR, SteamVR_Input_Sources.Any);

        InvokeRepeating("LogPlayerHealthAndPosition", 0, 1);
    }

    // Update is called once per frame
    void Update()
    {
        NextFireTime -= Time.deltaTime;

        PlayerCurrentHealth = Mathf.Min(PlayerCurrentHealth + playerRegenSpeed * Time.deltaTime, playerMaxHealth);

        if (!IsOverheated && lastHeatAddTime > 0f && Time.time - lastHeatAddTime > cooldownAfterSeconds)
        {
            StartCoroutine(CooldownRoutine());
        }
        
        UpdateChargeDisplay();

        SteamVR_Fade.Start(new Color(1f, 0f, 0f, 0.2f * (playerMaxHealth - playerCurrentHealth) / playerMaxHealth), Time.deltaTime);
    }

    private void UpdateChargeDisplay()
    {
        float chargePercentage = currentCharge;
        bool overcharged = false;
        
        if (buttonDown)
        {
            float btnDownTime = Time.time - lastBtnDownTime;
            overcharged = btnDownTime > maxChargeTime;
            if (overcharged) lastOverChargeTime = Time.time;
            chargePercentage = btnDownTime / minChargeTime;
        }
        else if (chargePercentage > 0f)
        {
            if (chargePercentage > 1f) chargePercentage = 1f;
            chargePercentage -= 2f * Time.deltaTime;
        }

        currentCharge = chargePercentage;
        CrosshairCharger.Instance.UpdateVisuals(currentCharge, Time.time - lastOverChargeTime < .5f); // half a second buffer for visual feedback of overcharge
    }

    public event Action OnDamaged;

    public void TakeDamage(float damage)
    {
        if (PlayerCurrentHealth <= 0) return;

        // Don't go below zero
        if (damage >= PlayerCurrentHealth)
            damage = PlayerCurrentHealth - 1;
        
        PlayerCurrentHealth -= damage;
        
        OnDamaged?.Invoke();
        
        // *** not used ***
        if (playerCurrentHealth <= 0)
        {
            PlayerCurrentHealth = 0;
            StartCoroutine( DestroySelf());
            OnPlayerDeath?.Invoke();
        }
        // ****************
    }

    public IEnumerator DestroySelf()
    {
        //tell boss the player is dead
        m_BossObject.GetComponent<BossAI>().StopBossFight();
        yield return m_fadeBlackScreen.FadeOut(0.01f);

        SceneManager.LoadScene(0);
    }

    public void ShootBullet(bool charged = false)
    {
        if(nextFireTime <= 0f && m_BossObject != null)
        {
            Vector3 spawnPos = lastFiredRight ? m_bulletSpawnpointLeft.position : m_bulletSpawnpointRight.position;
            lastFiredRight = !lastFiredRight;
            
            Vector3 direction = m_BossObject.activeSelf ? (m_BossObject.transform.position - spawnPos) : transform.forward;
            direction.Normalize();

            AmmunationModule temp = Instantiate(m_currentBullet, spawnPos, Quaternion.LookRotation(direction, Vector3.up));

            temp.direction = direction;

            if (charged)
            {
                // Fire additional bullet
                spawnPos = lastFiredRight ? m_bulletSpawnpointLeft.position : m_bulletSpawnpointRight.position;
                lastFiredRight = !lastFiredRight;
            
                direction = m_BossObject.activeSelf ? (m_BossObject.transform.position - spawnPos) : transform.forward;
                direction.Normalize();
                temp = Instantiate(m_currentBullet, spawnPos, Quaternion.LookRotation(direction, Vector3.up));

                temp.direction = direction;
            } else 
                AddHeat(heatPerShot);

            NextFireTime = 1f / fireRate;
        }


    }

    // Callback func for SteamVR input
    public void OnFireVR(SteamVR_Action_Boolean action, SteamVR_Input_Sources source, bool value) => OnFireInput(value);
    public void OnDeflectVR(SteamVR_Action_Boolean action, SteamVR_Input_Sources source) => OnDeflectInput();

    private void OnFireInput(bool btnDown)
    {
        if (!enabled || PauseMenu.Paused) return;
        
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
        
        if (btnDownTime <= maxChargeTime)
            ShootBullet(btnDownTime >= minChargeTime);
        else
        {
            // Overheat
            AddHeat(heatPerOvercharge);
        }
    }

    private void OnDeflectInput()
    {
        if (!enabled || !empReady || PauseMenu.Paused)
        {
            // TODO: visual / sfx feedback
            return;
        }

        StartCoroutine(Deflect());
    }

    private IEnumerator Deflect()
    {
        // VFX
        var explosionVFX = Instantiate(explosionPrefab).transform;
        explosionVFX.SetParent(explosionSpawnPosition);
        explosionVFX.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        yield return new WaitForSeconds(0.2f);
        
        // Destroy bullets
        Collider[] bullets = Physics.OverlapSphere(transform.position, empRange, LayerMask.GetMask("EnemyBullet"));
        foreach (var bullet in bullets)
        {
            StartCoroutine(bullet.GetComponent<DefaultBullet>().PlayCollisionEffect());
        }
        
        // Cooldown
        empReady = false;
        var originalColor = empDisplay.color;
        empDisplay.color = new Color(originalColor.r, originalColor.g, originalColor.b,
            0.1f);
        empText.text = "EMP on cooldown";
        
        yield return new WaitForSeconds(empCooldown);
        empReady = true;
        empDisplay.color = originalColor;
        empText.text = "EMP ready";
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
        heatDisplay.text = "    Overheated!\n" + new string('|', 30);
        heatDisplay.color = Color.red;
        CrosshairCharger.Instance.SetOverheat(true);
        
        yield return new WaitForSeconds(cooldownAfterSeconds);
        while (currentHeat > 0)
        {
            CurrentHeat -= cooldownRateOverheated;
            yield return new WaitForSeconds(0.1f);
        }
        IsOverheated = false;
        heatDisplay.text = "    Blaster Heat:";
        heatDisplay.color = new Color(1f, .7f, .2f);
        CrosshairCharger.Instance.SetOverheat(false);
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

    public void ResetValues()
    {
        CurrentHeat = 0;
        IsOverheated = false;
        currentCharge = 0f;
        CrosshairCharger.Instance.UpdateVisuals(0f);
    }

    // Callback func for Unity InputSystem (non-VR)
    public void OnFire(InputValue v) => OnFireInput(v.isPressed);
    private void OnDeflect() => OnDeflectInput();
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, empRange);
    }

    private void LogPlayerHealthAndPosition()
    {
        WriteTime.playerHealth = playerCurrentHealth;
        WriteTime.playerPosition = transform.position;
    }
#endif
}
