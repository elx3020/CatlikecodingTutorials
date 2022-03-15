using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graph : MonoBehaviour
{
    [SerializeField]
    Transform pointPrefab;
    [SerializeField, Range(10, 100)]
    int resolution = 10;

    [SerializeField]
    FunctionLibrary.FunctionName function;

    Transform[] points;


    private void Awake()
    {

        float step = resolution / 2f;

        Vector3 scale = Vector3.one / step;

        points = new Transform[resolution * resolution];

        for (int i = 0; i < points.Length; i++)
        {


            points[i] = Instantiate(pointPrefab);


            points[i].localScale = scale;
            points[i].SetParent(transform, false);
        }

    }


    private void Update()
    {
        FunctionLibrary.Function f = FunctionLibrary.GetFunction(function);
        float time = Time.time;
        float step = resolution / 2f;

        float v = (0.5f / step) - 1f;

        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                v = (((z + 0.5f) / step) - 1);
            }
            float u = (((x + 0.5f) / step) - 1);


            points[i].localPosition = f(u, v, time);
            // points[i].localPosition = new Vector3(u,0f,v);
        }
    }
}
