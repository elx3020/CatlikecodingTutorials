using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUGraph : MonoBehaviour
{



    ComputeBuffer positionBuffer;

    [SerializeField]
    ComputeShader computeShader;


    [SerializeField]
    Material material;

    [SerializeField]
    Mesh mesh;

    static readonly int
        positionsId = Shader.PropertyToID("_Positions"),
        resolutionId = Shader.PropertyToID("_Resolution"),
        stepId = Shader.PropertyToID("_Step"),
        timeId = Shader.PropertyToID("_Time");


    [SerializeField, Range(10, 1000)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    Bounds bounds;

    void OnEnable()
    {
        positionBuffer = new ComputeBuffer(resolution * resolution, 3 * 4);

    }


    void UpdateFunctionOnGPU()
    {
        float step = 2f / resolution;
        computeShader.SetInt(resolutionId, resolution);
        computeShader.SetFloat(stepId, step);
        computeShader.SetFloat(timeId, Time.time);

        computeShader.SetBuffer(0, positionsId, positionBuffer);
        int groups = Mathf.CeilToInt(resolution / 8f);
        computeShader.Dispatch(0, groups, groups, 1);

        // Draw mesh

        material.SetBuffer(positionsId, positionBuffer);
        material.SetFloat(stepId, step);

        bounds = new Bounds(Vector3.zero, Vector3.one * (2f + 2f / resolution));

        Graphics.DrawMeshInstancedProcedural(
            mesh, 0, material, bounds, positionBuffer.count
        );


    }





    private void Update()
    {
        UpdateFunctionOnGPU();

    }


    // transition function 



    // release buffer in case object gets disable or destroyed

    void OnDisable()
    {
        positionBuffer.Release();
        positionBuffer = null;
    }





}
