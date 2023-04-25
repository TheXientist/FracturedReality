using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletModule : MonoBehaviour
{
    [HideInInspector]
    public float speed = 1f;

    [HideInInspector]
    public Vector3 direction;

    [HideInInspector]
    public float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void Update()
    {
        Vector3 movement = direction * speed * Time.deltaTime;
        float rotationAmount = rotationSpeed * Time.deltaTime;

        transform.position += movement;
        transform.Rotate(0, 0, rotationAmount);
    }
}
