using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Fractal : MonoBehaviour
{
    enum FractalType
    {
        Mandelbox = 0, Mandelbulb = 1
    }
    // Must be identical to the struct in the shader
    struct FractalData
    {
        public int type;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public FractalData(int t, Vector3 p, Vector3 r, Vector3 s)
        {
            type = t;
            position = p;
            rotation = r;
            scale = s;
        }

        public static int SizeOf()
        {
            return sizeof(int) + 3 * 3 * sizeof(float);
        }
    }

    private static List<FractalData> allFractalData = new List<FractalData>();
    private static ComputeBuffer allFractalsBuffer;
    private static bool bufferInitialized;

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
        RegisterSelf();
    }

    private void Start()
    {
        if (bufferInitialized) return;
        
        CreateComputeBuffer(ref allFractalsBuffer, allFractalData, FractalData.SizeOf());
        raymarchingMaterial.SetBuffer(BUFFER_ID, allFractalsBuffer);
        raymarchingMaterial.SetInteger(COUNT_ID, allFractalData.Count);
        bufferInitialized = true;
    }

    private void Update()
    {
        bool isDirty = UpdateData();

        if (isDirty)
        {
            // Replace updated data and then write buffer
            allFractalData[id] = myData;
            allFractalsBuffer.SetData(allFractalData);
            raymarchingMaterial.SetBuffer(BUFFER_ID, allFractalsBuffer);
        }
    }

    /// <summary>
    /// Update all fields of the data struct except for type
    /// </summary>
    /// <returns>true if any field had to be updated</returns>
    private bool UpdateData()
    {
        bool dirty = false;
        if (myData.position != transform.position)
        {
            myData.position = transform.position;
            dirty = true;
        }
        if (myData.rotation != transform.rotation.eulerAngles)
        {
            myData.rotation = transform.rotation.eulerAngles;
            dirty = true;
        }
        if (myData.scale != transform.localScale)
        {
            myData.scale = transform.localScale;
            dirty = true;
        }

        return dirty;
    }

    private void RegisterSelf()
    {
        id = allFractalData.Count;
        
        myData = new FractalData((int)type,
            transform.position,
            transform.rotation.eulerAngles,
            transform.localScale);

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

    private void OnDisable()
    {
        if (allFractalsBuffer != null)
            allFractalsBuffer.Release();
    }
}
