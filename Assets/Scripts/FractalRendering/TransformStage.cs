using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public abstract class TransformStage : MonoBehaviour
{
    public TransformData data;

    public abstract bool UpdateData();
}
