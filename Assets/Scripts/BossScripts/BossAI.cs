
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour, IDamageable
{
    public float bossMaxHealth;
    public float bossCurrentHealth;
    private int currentPhase = 0;
    private int abilityCooldownTime = 0;

    [SerializeField]
    private List<BossPhaseScriptableObject> phaseList;

    [SerializeField]
    private GameObject player;


    // Start is called before the first frame update
    private void Start()
    {
        bossCurrentHealth = bossMaxHealth;

        //to make sure, the last phase lasts till the end of the boss-fight
        phaseList[phaseList.Count-1].percentPhaseCondition = 0;

        //start the main coroutine
        StartCoroutine("StartBossScene");        
    }

    private void Update()
    {
        if(bossCurrentHealth <= 0)
        {
            //boss dead
            gameObject.SetActive(false);
            //Destroy(gameObject);
        }
    }

    //Fight coroutine (Main)  possible to add "pre-events"
    private IEnumerator StartBossScene()
    {
        yield return BeginFight();

        yield return null;
    }

    private IEnumerator Phase()
    {
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
        bossCurrentHealth -= damage;
    }


}
