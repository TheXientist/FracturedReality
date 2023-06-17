using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossPhaseScriptableObject : ScriptableObject
{
    [SerializeField]
    public List<AbilityScriptableObject> phaseAbilityScripts;
    
    [HideInInspector]
    public List<int> abilityPropabilityList;

    [SerializeField]
    public float percentPhaseCondition = 0f;

    [SerializeField]
    public AudioClip phaseMusic;

    [SerializeField]
    public AudioClip phasePreMusic;

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
