using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CustomGravityRigidBody : MonoBehaviour
{
    Rigidbody body;
    float floatDelay;

    // staying awake
    [SerializeField]
    bool floatToSleep = false;


    void Awake()
    {
        body = GetComponent<Rigidbody>();
        body.useGravity = false;
    }

    void FixedUpdate()
    {
        Color cubeColor = body.IsSleeping() ? Color.gray : floatDelay >= 1f ? Color.yellow : Color.red;

        if (floatToSleep)
        {
            GetComponent<Renderer>().material.SetColor("_BaseColor", cubeColor);
            if (body.IsSleeping())
            {
                floatDelay = 0f;
                return;
            }

            if (body.velocity.sqrMagnitude < 0.0001f)
            {
                floatDelay += Time.deltaTime;
                if (floatDelay >= 1f)
                {
                    return;
                }
            }
            else
            {
                floatDelay = 0f;
            }

        }


        body.AddForce(CustomGravity.GetGravity(body.position), ForceMode.Acceleration);
    }

}
