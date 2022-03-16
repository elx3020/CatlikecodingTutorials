using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityModifier : MonoBehaviour
{
    [SerializeField]
    Vector3 gravityDirection;

    [SerializeField, Range(0f, 10f)]
    float maxAcceleration;

    void Update()
    {

        Physics.gravity = gravityDirection.normalized * maxAcceleration;

    }



}
