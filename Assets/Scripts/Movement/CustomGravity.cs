using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomGravity
{
    public static Vector3 GetGravity(Vector3 position)
    {
        Vector3 sphericalGravity = position.normalized * Physics.gravity.y;


        return position.normalized * Physics.gravity.y;
    }

    public static Vector3 GetUpAxis(Vector3 position)
    {
        Vector3 up = position.normalized;
        up = Physics.gravity.y < 0 ? -up : up;
        return -Physics.gravity.normalized;
    }

    public static Vector3 GetGravity(Vector3 position, out Vector3 upAxis)
    {
        Vector3 up = position.normalized;
        up = Physics.gravity.y < 0 ? -up : up;
        upAxis = -Physics.gravity.normalized;
        return Physics.gravity;
    }
}
