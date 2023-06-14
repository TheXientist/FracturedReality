using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;


[CreateAssetMenu(fileName = "Explosion", menuName = "BossAbilities/Explosion")]
public class Explosion : AbilityScriptableObject
{
    [SerializeField]
    private float m_explosionSpeed = 1.01f;

    /// <summary>
    /// This prefab will be used as a bullet.
    /// </summary>
    [SerializeField]
    private GameObject m_bulletPrefab;

    /// <summary>
    /// Will call the explode method.
    /// </summary>
    /// <param name="bossPosition"></param>
    /// <param name="playerPosition"></param>
    /// <returns></returns>
    public override IEnumerator Execute(GameObject bossObject, GameObject playerObject)
    {  

        yield return Explode(bossObject.transform.position);
        

        yield return null;
    }

    /// <summary>
    /// Will spawn a bullet and increase its size over time.
    /// </summary>
    /// <param name="bossPosition"></param>Position to spawn the bullet.
    /// <returns></returns>
    public IEnumerator Explode(Vector3 bossPosition)
    {
        GameObject projectile = SpawnObjectAtPosition(bossPosition, m_bulletPrefab);

        while(projectile.transform.localScale.magnitude <= 100)
        {
            projectile.transform.localScale *= m_explosionSpeed;
            
            yield return null;
        }

        Destroy(projectile);
    }
}
