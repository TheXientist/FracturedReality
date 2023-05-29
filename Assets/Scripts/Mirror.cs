using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Mirror : MonoBehaviour
{
    // Must be identical to the struct in the shader
    struct MirrorData
    {
        public Vector3 position;
        public Vector3 normal;

        public MirrorData(Vector3 pos, Vector3 n)
        {
            position = pos;
            normal = n;
        }

        public static int SizeOf()
        {
            return 2 * 3 * sizeof(float);
        }
    }

    private static List<MirrorData> allMirrorData = new List<MirrorData>();
    private static ComputeBuffer allMirrorsBuffer;

    [SerializeField] private Material raymarchingMaterial;

    [SerializeField] private int id;
    private MirrorData myData;

    #region Shader IDs

    private int BUFFER_ID;
    private int COUNT_ID;

    #endregion

    private void Awake()
    {
        BUFFER_ID = Shader.PropertyToID("_MirrorBuffer");
        COUNT_ID = Shader.PropertyToID("_MirrorCount");
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
            allMirrorData[id] = myData;
            RefreshBuffer();
        }
    }

    /// <summary>
    /// Update the transform matrix
    /// </summary>
    /// <returns>true if it had to be updated</returns>
    private bool UpdateData()
    {
        if (myData.position == transform.position && myData.normal == transform.up) return false;

        myData.position = transform.position;
        myData.normal = transform.up;
        return true;
    }

    private void RegisterSelf()
    {
        id = allMirrorData.Count;

        myData = new MirrorData(transform.position, transform.up);

        allMirrorData.Add(myData);
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
        CreateComputeBuffer(ref allMirrorsBuffer, allMirrorData, MirrorData.SizeOf());
        raymarchingMaterial.SetBuffer(BUFFER_ID, allMirrorsBuffer);
        raymarchingMaterial.SetInteger(COUNT_ID, allMirrorData.Count);
    }

    private void OnDisable()
    {
        allMirrorData.RemoveAt(id);
        RefreshBuffer();
    }
}
