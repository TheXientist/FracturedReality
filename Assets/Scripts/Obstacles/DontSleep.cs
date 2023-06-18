using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontSleep : MonoBehaviour
{
    private void Awake()
    {
        GetComponent<Rigidbody>().sleepThreshold = 0f;
    }
}
