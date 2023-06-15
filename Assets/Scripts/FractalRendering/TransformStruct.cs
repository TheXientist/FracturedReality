using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TransformData //read [0,0-2] and [1,0-2] for mirror vectors
{
    public int mirror;
    public Matrix4x4 data;

    public TransformData(int t, Matrix4x4 m)
    {
        mirror = t;
        data = m;
    }

    public static int SizeOf()
    {
        return sizeof(int) + 4 * 4 * sizeof(float);
    }

    public static Matrix4x4 VectorToMatrix(Vector3 pos, Vector3 n)
    {
        Matrix4x4 matrix = Matrix4x4.zero;
        matrix.m00 = pos.x;
        matrix.m01 = pos.y;
        matrix.m02 = pos.z;
        matrix.m10 = n.x;
        matrix.m11 = n.y;
        matrix.m12 = n.z;
        return matrix;
    }
}