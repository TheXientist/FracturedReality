using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

[ExecuteInEditMode]
public class TransformManager : MonoBehaviour
{
    public List<TransformData> allTransformData = new List<TransformData>();
    public List<TransformStage> transformStages = new List<TransformStage>();

    private ComputeBuffer allTransformsBuffer;

    [SerializeField] private Material raymarchingMaterial;

    public bool showPlanes = true;
    public bool refresh = false;

    #region Shader IDs

    private int BUFFER_ID;
    private int COUNT_ID;

    #endregion

    private void Awake()
    {
        BUFFER_ID = Shader.PropertyToID("_TransformBuffer");
        COUNT_ID = Shader.PropertyToID("_TransformCount");
    }

    private void OnEnable()
    {
        Register();
        RefreshBuffer();
    }

    private void Update()
    {
        foreach (TransformStage stage in transformStages)
        {
            stage.gameObject.GetComponent<MeshRenderer>().enabled = showPlanes;
        }

        for (int i = 0; i < transformStages.Count; ++i)
        {
            bool isDirty = transformStages[transformStages.Count - 1 - i].UpdateData();

            if (isDirty)
            {
                // Replace updated data and then write buffer
                allTransformData[i] = transformStages[transformStages.Count - 1 - i].data;
                RefreshBuffer();
            }
        }

        if (refresh)
        {
            refresh = false;
            Refresh();
        }
    }

    void Refresh()
    {
        allTransformData = new List<TransformData>();
        for (int i = transformStages.Count - 1; i >= 0; --i)
        {
            allTransformData.Add(transformStages[i].data);
            RefreshBuffer();
        }
    }

    public void Register()
    {
        allTransformData = new List<TransformData>();

        for (int i = transformStages.Count - 1; i >= 0; --i)
        {
            transformStages[i].UpdateData();
            allTransformData.Add(transformStages[i].data);
        }
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
        CreateComputeBuffer(ref allTransformsBuffer, allTransformData, TransformData.SizeOf());
        raymarchingMaterial.SetBuffer(BUFFER_ID, allTransformsBuffer);
        raymarchingMaterial.SetInteger(COUNT_ID, allTransformData.Count);
    }

    private void OnDisable()
    {
        allTransformData = new List<TransformData>();
        RefreshBuffer();
    }
}
