
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class BossAI : MonoBehaviour, IDamageable
{
    [SerializeField] private float bossMaxHealth;
    private float bossCurrentHealth;

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


    private void SetupReferences()
    {
        SpaceshipController.Instance.SetTarget(transform);
        SpaceshipController.Instance.GetComponent<Player>().m_BossObject = gameObject;
        player = SpaceshipController.Instance.GetComponent<Player>();
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("BossAI.Start");

        m_musicFader = FindAnyObjectByType<MusicFader>();
        
        SetupReferences();
        
        healthDisplay = GameObject.FindWithTag("BossHealthDisplay").GetComponent<TextMeshProUGUI>();
        BossCurrentHealth = bossMaxHealth;

        //to make sure, the last phase lasts till the end of the boss-fight
        phaseList[phaseList.Count-1].percentPhaseCondition = 0;
        
        warningDisplay = GameObject.FindWithTag("RangeDisplay").transform.GetChild(0).gameObject;

        //start the main coroutine
        //StartCoroutine("StartBossScene"); --> start by Spawn animation       
    }

    private void Update()
    {
        CheckDistanceToPlayer();
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
        m_bossCoroutine= BeginFight();

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
                StartCoroutine(m_musicFader.PlayMusic(phase.phasePreMusic, true, false,
                    () => StartCoroutine(m_musicFader.PlayMusic(phase.phaseMusic, false, true))));
            
            else
                StartCoroutine(m_musicFader.PlayMusic(phase.phaseMusic, true, true));
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
        abilityCooldownTime = phseAbilityScriptList[m_currentAbilityNumber].abilityCooldown;

        yield return phseAbilityScriptList[m_currentAbilityNumber].Execute(gameObject, player.gameObject);
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
        if (destroyed) return;
        
        BossCurrentHealth -= damage;
        if (bossCurrentHealth <= 0)
        {
            StopBossFight();
            destroyed = true;
            GetComponent<Animator>().SetTrigger("Death");
            
            // Play death sound, then loop post-fight music
            StartCoroutine(m_musicFader.PlayMusic(deathSound, true, false,
                () => StartCoroutine(m_musicFader.PlayMusic(postFightMusic, false, true)))); // Doesn't work, maybe because this gameobject will be inactive at that point?
        }
        OnDamaged?.Invoke();
    }

    // Called by Death animation
    public void Despawn()
    {
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
