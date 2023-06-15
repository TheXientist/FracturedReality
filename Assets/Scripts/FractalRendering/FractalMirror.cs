using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FractalMirror : TransformStage
{
    public override bool UpdateData()
    {
        Matrix4x4 m = TransformData.VectorToMatrix(transform.position, transform.up);
        if (data.data == m) return false;

        data = new TransformData(1, m);
        return true;
    }
}
