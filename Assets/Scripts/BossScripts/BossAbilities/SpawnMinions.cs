using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SpawnMinions", menuName = "BossAbilities/SpawnMinions")]
public class SpawnMinions : AbilityScriptableObject
{

    [SerializeField] private GameObject m_MinionPrefab;

    public override IEnumerator Execute(Transform spawn, Vector3 targetPosition)
    {
        SpawnObjectAtPosition(spawn.position, m_MinionPrefab);

        yield return null;
    }
}
