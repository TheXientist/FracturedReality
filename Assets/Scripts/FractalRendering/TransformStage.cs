using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public abstract class TransformStage : MonoBehaviour
{
    public TransformData data;
    private TransformManager manager;

    public abstract bool UpdateData();

    void OnEnable()
    {
        manager = FindObjectOfType<TransformManager>();
        if (manager)
        {
            if (!manager.transformStages.Contains(this)) manager.transformStages.Add(this);
            manager.Register();
        }
    }

    void OnDisable()
    {
        if (manager)
        {
            if(manager.transformStages.Contains(this)) manager.transformStages.Remove(this);
            manager.Register();
        }
    }
}
