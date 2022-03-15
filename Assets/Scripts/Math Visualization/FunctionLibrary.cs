
using UnityEngine;
using static UnityEngine.Mathf;

public static class FunctionLibrary
{

    public delegate Vector3 Function(float u, float v, float t);

    static Function[] functions = { Grid, Wave, MultiWave, Ripple, Sphere };

    public enum FunctionName { Grid, Wave, MultiWave, Ripple, Sphere };

    public static Function GetFunction(FunctionName name)
    {
        return functions[(int)name];

    }


    public static Vector3 Grid(float u, float v, float t)
    {
        Vector3 outV;
        outV.x = u;
        outV.y = 0;
        outV.z = v;
        return outV;
    }




    public static Vector3 Wave(float u, float v, float t)
    {
        Vector3 outV;
        outV.x = u;
        outV.y = Sin(PI * (u + v + t));
        outV.z = v;
        return outV;
    }

    public static Vector3 MultiWave(float u, float v, float t)
    {
        Vector3 outV;

        outV.x = u;
        outV.y = Sin(PI * (u + t));
        outV.y += Sin(2f * PI * (v + t)) * (1f / 2f);
        outV.y += Sin(PI * (u + v + 0.25f + t)) * (1f / 2.5f);
        outV.z = v;
        return outV;
    }

    public static Vector3 Ripple(float u, float v, float t)
    {
        float d = Sqrt(u * u + v * v);
        Vector3 outV;
        outV.x = u;
        outV.y = Sin(PI * (2f * d) - t) / (1f + 10f * d);
        outV.z = v;
        return outV;
    }

    public static Vector3 Sphere(float u, float v, float t)
    {
        float r = Cos(0.5f * PI * v);
        Vector3 outV;
        outV.x = r * Sin(PI * u);
        outV.y = Sin(PI * 0.5f * v);
        outV.z = r * Cos(PI * u);
        return outV;

    }


}
