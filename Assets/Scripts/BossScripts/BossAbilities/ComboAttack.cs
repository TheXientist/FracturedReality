using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New ComboAttack", menuName = "BossAbilities/ComboAttack")]
public class ComboAttack : AbilityScriptableObject
{
    [SerializeField] private AbilityScriptableObject[] subAbilities;
    [SerializeField]
    private bool useIndividualCooldowns, useIndividualWindups;

    private AbilityScriptableObject currentExecutingAbility;

    private bool interrupted;
    
    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        for (int i = 0; i < subAbilities.Length; i++)
        {
            if (interrupted) break;
            
            AbilityScriptableObject currentAbility = subAbilities[i];
            
            if (useIndividualWindups)
                yield return new WaitForSeconds(currentAbility.abilityWindup);

            if (interrupted) break;

            currentExecutingAbility = currentAbility;
            yield return currentAbility.Execute(bossObject, playerObject);
            currentExecutingAbility = null;

            if (interrupted) break;
            
            if (useIndividualCooldowns)
                yield return new WaitForSeconds(currentAbility.abilityCooldown);
        }
        yield return null;
    }
    
    public override void InterruptCurrentAbility()
    {
        interrupted = true;
        if (currentExecutingAbility != null)
            currentExecutingAbility.InterruptCurrentAbility();
    }
}
