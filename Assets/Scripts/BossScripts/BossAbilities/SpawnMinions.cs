using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpawnMinions", menuName = "BossAbilities/SpawnMinions")]
public class SpawnMinions : AbilityScriptableObject
{

    [SerializeField] private GameObject m_MinionPrefab;

    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {
        SpawnObjectAtPosition(bossObject.transform.position, m_MinionPrefab);

        yield return null;
    }
}
