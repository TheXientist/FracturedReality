
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossAI : MonoBehaviour, IDamageable
{
    public static BossAI Instance { get; private set; }
    
    [SerializeField] private float bossMaxHealth;
    private float bossCurrentHealth;

    [SerializeField]
    private bool invincible;
    
    private MusicFader m_musicFader;

    [SerializeField] private AudioClip deathSound, postFightMusic;

    public float BossCurrentHealth
    {
        private set
        {
            bossCurrentHealth = value;
            healthDisplay.text = (value <= 0f) ? "You won!" : "Boss Health:\n" + bossCurrentHealth;
        }
        get => bossCurrentHealth;
    }

    private int currentPhase = 0;
    private int abilityCooldownTime = 0;
    private bool destroyed = false;

    [SerializeField]
    private List<BossPhaseScriptableObject> phaseList;

    [SerializeField]
    private Player player;

    [SerializeField, Tooltip("Range the player has to stay within to not get damaged")] private float maxPlayerRange;
    [SerializeField] private int damagePerSecondWhenOutOfRange;
    private float lastDamageTickTime;

    private TextMeshProUGUI healthDisplay;

    private int m_currentAbilityNumber;

    private IEnumerator m_bossCoroutine;

    private GameObject warningDisplay;

    [SerializeField] private Animator animator;

    [SerializeField] private Transform bulletSpawnPoint;


    private void SetupReferences()
    {
        SpaceshipController.Instance.SetTarget(transform);
        SpaceshipController.Instance.GetComponent<Player>().m_BossObject = gameObject;
        player = SpaceshipController.Instance.GetComponent<Player>();
    }

    private void Awake()
    {
        Instance = this;
        m_musicFader = FindAnyObjectByType<MusicFader>();
        healthDisplay = GameObject.FindWithTag("BossHealthDisplay").GetComponent<TextMeshProUGUI>();
        warningDisplay = GameObject.FindWithTag("RangeDisplay").transform.GetChild(0).gameObject;
        gameObject.SetActive(false);
    }

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("BossAI.Start");
        
        SetupReferences();

        BossCurrentHealth = bossMaxHealth;

        //to make sure, the last phase lasts till the end of the boss-fight
        phaseList[phaseList.Count-1].percentPhaseCondition = 0;
        
        
        if (phaseList[0].phasePreMusic != null)
            m_musicFader.PlayMusic(phaseList[0].phasePreMusic, true, false, () => m_musicFader.PlayMusic(phaseList[0].phaseMusic, false, true));
        else
            m_musicFader.PlayMusic(phaseList[0].phaseMusic, true, true);

        // Make boss invincible during spawn animation
        invincible = true;

        //start the main coroutine
        //StartCoroutine("StartBossScene"); --> start by Spawn animation       
    }

    private void Update()
    {
        CheckDistanceToPlayer();
        FacePlayer();
    }

    private void FacePlayer()
    {
        transform.forward = (player.transform.position - transform.position).normalized;
    }

    private void CheckDistanceToPlayer()
    {
        if (Vector3.Distance(transform.position, player.transform.position) <= maxPlayerRange)
        {
            warningDisplay.SetActive(false);
            return;
        }
        warningDisplay.SetActive(true);
        
        if (Time.time - lastDamageTickTime > 1f)
        {
            lastDamageTickTime = Time.time;
            player.TakeDamage(damagePerSecondWhenOutOfRange);
        }
    }

    //Fight coroutine (Main)  possible to add "pre-events"
    public void StartBossScene()
    {
        invincible = false;
        m_bossCoroutine = BeginFight();

        StartCoroutine(m_bossCoroutine);

    }

    private IEnumerator Phase()
    {
        if (destroyed)
            yield break;
        
        CalculatePhaseAbilityPropabilities();

        var phase = phaseList[currentPhase];
        
        if(phase.phaseMusic != null)
        {
            if (phase.phasePreMusic != null)
                // Fade premusic, then fade looping music
                StartCoroutine(m_musicFader.PlayMusicCoroutine(phase.phasePreMusic, true, false,
                    () => StartCoroutine(m_musicFader.PlayMusicCoroutine(phase.phaseMusic, false, true))));
            
            else
                StartCoroutine(m_musicFader.PlayMusicCoroutine(phase.phaseMusic, true, true));
        }

        while (bossCurrentHealth > bossMaxHealth * phase.percentPhaseCondition )
        {
            yield return UseRandomPhaseAbility(phase.phaseAbilityScripts, phase.abilityPropabilityList);
            yield return new WaitForSeconds(abilityCooldownTime);
        }

        if (phaseList.Count > currentPhase)
        {
            currentPhase++;
        };

        Debug.Log("Phase " + currentPhase + " ended");
    }

    private IEnumerator BeginFight()
    {
        foreach (var phase in phaseList)
        {
            yield return this.Phase();
        }
    }

    private int GetRandomAbility(List<int> abilityPropabilityList)
    {
        return abilityPropabilityList[Random.Range(0, abilityPropabilityList.Count)];
    }

    private IEnumerator UseRandomPhaseAbility(List<AbilityScriptableObject> phseAbilityScriptList, List<int> abilityPropabilityList)
    {
        m_currentAbilityNumber = GetRandomAbility(abilityPropabilityList);
        var currentAbility = phseAbilityScriptList[m_currentAbilityNumber];
        abilityCooldownTime = currentAbility.abilityCooldown;

        animator.SetTrigger(currentAbility.animationTriggerName);
        
        yield return new WaitForSeconds(currentAbility.abilityWindup);
        
        if (!destroyed) // Check if boss got killed during windup
            yield return currentAbility.Execute(bulletSpawnPoint, player.transform.position);
    }

    private void CalculatePhaseAbilityPropabilities()
    {
        foreach (var phase in phaseList)
        {
            phase.abilityPropabilityList.Clear();
            phase.calculatePropability();
        }
    }

    public event Action OnDamaged;

    public void TakeDamage(float damage)
    {
        if (destroyed || invincible) return;
        
        BossCurrentHealth -= damage;
        if (bossCurrentHealth <= 0)
        {
            StopBossFight();
            destroyed = true;
            animator.SetTrigger("Death");
            
            // Play death sound, then loop post-fight music
            StartCoroutine(m_musicFader.PlayMusicCoroutine(deathSound, true, false));
        }
        OnDamaged?.Invoke();
    }

    // Called by Death animation
    public void Despawn()
    {
        m_musicFader.PlayMusic(postFightMusic, true, true);
        gameObject.SetActive(false);
        // Unlock player targeting
        FindObjectOfType<SpaceshipController>().RemoveTarget();
    }

    public void StopBossFight()
    {
        phaseList[currentPhase].phaseAbilityScripts[m_currentAbilityNumber].InterruptCurrentAbility();
        StopCoroutine(m_bossCoroutine);
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxPlayerRange);
    }
#endif
}
