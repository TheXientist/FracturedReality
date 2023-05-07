using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AmmunationModule : MonoBehaviour
{
    
    public float speed = 1f;

    [HideInInspector]
    public Vector3 direction;

    
    public float rotationSpeed;

    private const int LIFETIME = 8;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroySelfAfterSeconds(LIFETIME));
    }

    private void Update()
    {
        Vector3 movement = direction * speed * Time.deltaTime;
        float rotationAmount = rotationSpeed * Time.deltaTime;

        transform.position += movement;
        transform.Rotate(0, 0, rotationAmount);
    }

    public IEnumerator DestroySelf()
    {
        Destroy(gameObject);
        yield return null;
    }

    public IEnumerator DestroySelfAfterSeconds(int seconds)
    {
        yield return new WaitForSeconds(seconds);
        Destroy(gameObject);       
    }
}
