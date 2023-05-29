using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class Fractal : MonoBehaviour
{
    enum FractalType
    {
        Mandelbox = 0, Mandelbulb = 1, Mirror = 2
    }
    // Must be identical to the struct in the shader
    struct FractalData
    {
        public int type;
        public Matrix4x4 worldToLocal;

        public FractalData(int t, Matrix4x4 m)
        {
            type = t;
            worldToLocal = m;
        }

        public static int SizeOf()
        {
            return sizeof(int) + 4 * 4 * sizeof(float);
        }
    }

    private static List<FractalData> allFractalData = new List<FractalData>();
    private static ComputeBuffer allFractalsBuffer;

    [SerializeField] private FractalType type;

    [SerializeField] private Material raymarchingMaterial;

    private int id;
    private FractalData myData;

    #region Shader IDs

    private int BUFFER_ID;
    private int COUNT_ID;
    
    #endregion

    private void Awake()
    {
        BUFFER_ID = Shader.PropertyToID("_FractalBuffer");
        COUNT_ID = Shader.PropertyToID("_FractalCount");
    }

    private void OnEnable()
    {
        RegisterSelf();
        RefreshBuffer();
    }

    private void Update()
    {
        bool isDirty = UpdateData();

        if (isDirty)
        {
            // Replace updated data and then write buffer
            allFractalData[id] = myData;
            RefreshBuffer();
        }
    }

    /// <summary>
    /// Update the transform matrix
    /// </summary>
    /// <returns>true if it had to be updated</returns>
    private bool UpdateData()
    {
        if (myData.worldToLocal == transform.worldToLocalMatrix) return false;
        
        myData.worldToLocal = transform.worldToLocalMatrix;
        return true;
    }

    private void RegisterSelf()
    {
        id = allFractalData.Count;
        
        myData = new FractalData((int)type,
            transform.worldToLocalMatrix);

        allFractalData.Add(myData);
    }
    
    // Create or update the compute buffers
    private void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
        where T : struct
    {
        if (buffer != null)
        {
            // Release outdated buffers
            if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
            {
                buffer.Release();
                buffer = null;
            }
        }

        if (data.Count != 0)
        {
            // Create buffer if needed, initial fill
            if (buffer == null)
            {
                buffer = new ComputeBuffer(data.Count, stride);
            }
            buffer.SetData(data);
        }
    }

    private void RefreshBuffer()
    {
        CreateComputeBuffer(ref allFractalsBuffer, allFractalData, FractalData.SizeOf());
        raymarchingMaterial.SetBuffer(BUFFER_ID, allFractalsBuffer);
        raymarchingMaterial.SetInteger(COUNT_ID, allFractalData.Count);
    }

    private void OnDisable()
    {
        allFractalData.RemoveAt(id);
        RefreshBuffer();
    }
}
