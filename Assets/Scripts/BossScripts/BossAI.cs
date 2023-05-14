
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossAI : MonoBehaviour, IDamageable
{
    [SerializeField] private float bossMaxHealth;
    private float bossCurrentHealth;

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
    private GameObject player;

    private TextMeshProUGUI healthDisplay;


    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("BossAI.Start");
        
        healthDisplay = GameObject.FindWithTag("BossHealthDisplay").GetComponent<TextMeshProUGUI>();
        BossCurrentHealth = bossMaxHealth;

        //to make sure, the last phase lasts till the end of the boss-fight
        phaseList[phaseList.Count-1].percentPhaseCondition = 0;

        //start the main coroutine
        //StartCoroutine("StartBossScene"); --> start by Spawn animation       
    }

    //Fight coroutine (Main)  possible to add "pre-events"
    public void StartBossScene()
    {
        StartCoroutine(BeginFight());
    }

    private IEnumerator Phase()
    {
        if (destroyed)
            yield break;
        
        CalculatePhaseAbilityPropabilities();

        while (bossCurrentHealth > bossMaxHealth * phaseList[currentPhase].percentPhaseCondition)
        {
            yield return UseRandomPhaseAbility(phaseList[currentPhase].phaseAbilityScripts, phaseList[currentPhase].abilityPropabilityList);
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
        int tempAbilityNumber = GetRandomAbility(abilityPropabilityList);
        abilityCooldownTime = phseAbilityScriptList[tempAbilityNumber].abilityCooldown;

        yield return phseAbilityScriptList[tempAbilityNumber].Execute(gameObject, player);
    }

    private void CalculatePhaseAbilityPropabilities()
    {
        foreach (var phase in phaseList)
        {
            phase.abilityPropabilityList.Clear();
            phase.calculatePropability();
        }
    }

    public void TakeDamage(float damage)
    {
        if (destroyed) return;
        
        BossCurrentHealth -= damage;
        if (bossCurrentHealth <= 0)
        {
            destroyed = true;
            GetComponent<Animator>().SetTrigger("Death");
        }
    }

    // Called by Death animation
    public void Despawn()
    {
        gameObject.SetActive(false);
        // Unlock player targeting
        FindObjectOfType<SpaceshipController>().RemoveTarget();
    }
}
