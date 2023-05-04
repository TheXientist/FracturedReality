
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossClass : MonoBehaviour
{
    public float bossMaxHealth;
    public float bossCurrentHealth;
    protected int currentPhase = 0;
    protected int abilityCooldownTime = 0;

    [SerializeField]
    protected List<BossPhaseScriptableObject> phaseList;

    [SerializeField]
    protected GameObject m_Player;


    public IEnumerator phase() 
    {
        calculatePhaseAbilityPropabilities();

        while (bossCurrentHealth > bossMaxHealth * phaseList[currentPhase].percentPhaseCondition)
        {
            yield return useRandomPhaseAbility(phaseList[currentPhase].phaseAbilityScripts, phaseList[currentPhase].abilityPropabilityList);
            yield return new WaitForSeconds(abilityCooldownTime);
            print("in phase " +  currentPhase);
        }

        if(phaseList.Count > currentPhase) 
        { 
            currentPhase++; 
        };

        Debug.Log("phase " + currentPhase + " ended");
      
    }

    public IEnumerator beginFight()
    {
        foreach (var phase in phaseList)
        {
            print("begin fight werwerwerwer");
            yield return this.phase();
        }
    }

    protected List<int> calculatePropability(List<AbilityScriptableObject> abilityList) 
    {
        
        List<int> propability = new List<int>();
        for (int j = 0; j < abilityList.Count; j++) 
        {
            
            for (int i = 0; i < abilityList[j].abilityPropability; i++)
            {
                propability.Add(j); ;
            }
            
        }
        return propability;
    }

    protected int getRandomAbility(List<int> abilityPropabilityList)
    {
        return abilityPropabilityList[Random.Range(0, abilityPropabilityList.Count)];
    }

    protected IEnumerator useRandomPhaseAbility(List<AbilityScriptableObject> phseAbilityScriptList, List<int> abilityPropabilityList) 
    {
        int tempAbilityNumber = getRandomAbility(abilityPropabilityList);
        abilityCooldownTime = phseAbilityScriptList[tempAbilityNumber].abilityCooldown;

        yield return phseAbilityScriptList[tempAbilityNumber].Execute(gameObject, m_Player);
    }

    protected void calculatePhaseAbilityPropabilities()
    {
        foreach( var phase in phaseList ) 
        {
            phase.abilityPropabilityList.Clear();
            phase.calculatePropability();
        }
    }


}
