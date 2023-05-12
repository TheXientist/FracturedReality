using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour, IDamageable
{
    public float obstacleMaxHealth = 100;
    public float obstacleCurrentHealth = 0;

    // Start is called before the first frame update
    void Start()
    {
        obstacleCurrentHealth = obstacleMaxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if(obstacleCurrentHealth <= 0)
        {
            DestroySelf();
        }
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(float damage)
    {
        obstacleCurrentHealth -= damage;
    }


}
