using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDamageable
{
    public event Action OnDamaged; 
    public void TakeDamage(float damage);
}
