using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhaseScriptableObject : ScriptableObject
{
    [SerializeField]
    public List<AbilityScriptableObject> phaseAbilityScripts;

    [SerializeField]
    public List<int> abilityPropabilityList;

    [SerializeField]
    public float percentPhaseCondition = 0f;

    public void calculatePropability()
    {

        List<int> propability = new List<int>();
        for (int j = 0; j < phaseAbilityScripts.Count; j++)
        {

            for (int i = 0; i < phaseAbilityScripts[j].abilityPropability; i++)
            {
                abilityPropabilityList.Add(j); ;
            }

        }
        //return propability;
    }
}
