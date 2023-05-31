using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FractalTransform : TransformStage
{
    public override bool UpdateData()
    {
        if (data.data == transform.worldToLocalMatrix) return false;

        data = new TransformData(0, transform.worldToLocalMatrix);
        return true;
    }
}
