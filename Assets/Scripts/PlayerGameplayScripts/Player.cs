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
    private Transform m_bulletSpawnpointLeft, m_bulletSpawnpointRight;
    private bool lastFiredRight;

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

    private SteamVR_Action_Boolean fireAction, deflectAction;

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

    [Header("EMP"), SerializeField] private float empCooldown;
    [SerializeField] private float empRange;
    private bool empReady = true;
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private Transform explosionSpawnPosition;

    private TextMeshProUGUI empText;
    private GameObject empDisplay;

    void Start()
    {
        healthDisplay = GameObject.FindWithTag("ShipHealthDisplay").GetComponent<TextMeshProUGUI>();
        heatText = GameObject.FindWithTag("WeaponStateDisplay").GetComponent<TextMeshProUGUI>();
        heatDisplay = GameObject.FindWithTag("HeatDisplay").GetComponent<Image>();
        chargeDisplay = GameObject.FindWithTag("ChargeDisplay").GetComponent<Image>();
        chargeMark = GameObject.FindWithTag("ChargeMark").GetComponent<Image>();
        empText = GameObject.FindWithTag("EMPText").GetComponent<TextMeshProUGUI>();
        empDisplay = GameObject.FindWithTag("EMPBorder");
        PlayerCurrentHealth = playerMaxHealth;
        fireAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "fire");
        fireAction.AddOnChangeListener(OnFireVR, SteamVR_Input_Sources.Any);
        deflectAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("spaceship", "deflect");
        deflectAction.AddOnStateDownListener(OnDeflectVR, SteamVR_Input_Sources.Any);

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
        if (PlayerCurrentHealth <= 0) return;
        
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
        // TODO: separate layer for bullets
        Collider[] bullets = Physics.OverlapSphere(transform.position, empRange, LayerMask.GetMask("Enemy"));
        foreach (var bullet in bullets)
        {
            if (bullet.tag.Equals("Boss")) continue;

            StartCoroutine(bullet.GetComponent<DefaultBullet>().PlayCollisionEffect());
        }
        
        // Cooldown
        empReady = false;
        empDisplay.gameObject.SetActive(false);
        empText.text = "EMP on cooldown";
        
        yield return new WaitForSeconds(empCooldown);
        empReady = true;
        empDisplay.gameObject.SetActive(true);
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
    private void OnDeflect() => OnDeflectInput();
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, empRange);
    }
#endif
}
