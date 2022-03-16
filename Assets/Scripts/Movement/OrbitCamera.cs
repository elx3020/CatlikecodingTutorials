using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{
    [SerializeField]
    Transform focus = default;

    [SerializeField, Range(0f, 50f)]
    float distance = 5f;
    [SerializeField, Min(0f)]
    float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)]
    float focusCentering = 0.5f;

    Vector3 focusPoint, previousFocusPoint;

    Vector2 orbitAngles = new Vector2(35f, -45f);

    [SerializeField, Range(0f, 360f)]

    float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)]
    float minVerticalAngle = -30f, maxVerticalAngle = 60f;


    //fields for automatic rotation 
    float lastTimeManualChange;
    [SerializeField, Min(0f)]
    float alignDelay = 5f;

    [SerializeField, Range(0, 90f)]
    float alignSmoothRange = 45f;

    Camera regularCamera;

    [SerializeField]
    LayerMask obstructionMask = -1;


    Vector3 cameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;
            halfExtends.y = regularCamera.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * regularCamera.fieldOfView);
            halfExtends.x = halfExtends.y * regularCamera.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }


    // for relative upAxis
    Quaternion gravityAlignment = Quaternion.identity;
    Quaternion orbitRotation;



    void Onvalidate()
    {
        if (maxVerticalAngle < minVerticalAngle)
        {
            maxVerticalAngle = minVerticalAngle;
        }
    }

    void Awake()
    {
        regularCamera = GetComponent<Camera>();
        focusPoint = focus.position;
        transform.localRotation = orbitRotation = Quaternion.Euler(orbitAngles);

    }

    void LateUpdate()
    {

        // to align camera rotation with upaxis relative to gravity

        gravityAlignment = Quaternion.FromToRotation(gravityAlignment * Vector3.up, CustomGravity.GetUpAxis(focusPoint)) * gravityAlignment;


        UpdateFocusPoint();
        ManualRotation();
        // get the direction of the z vector
        // Vector3 lookDirection = transform.forward;

        // || AutomaticRotation()

        if (ManualRotation() || AutomaticRotation())
        {
            ConstrainAngles();
            orbitRotation = Quaternion.Euler(orbitAngles);
        }

        Quaternion lookRotation = gravityAlignment * orbitRotation;
        Vector3 lookDirection = lookRotation * Vector3.forward;
        Vector3 lookPosition = focusPoint - lookDirection * distance;
        AutomaticDistance(lookDirection, ref lookPosition, lookRotation);
        transform.SetPositionAndRotation(lookPosition, lookRotation);
    }

    void UpdateFocusPoint()
    {
        previousFocusPoint = focusPoint;
        Vector3 targetPoint = focus.position;
        if (focusRadius > 0)
        {
            float distance = Vector3.Distance(targetPoint, focusPoint);
            float t = 1f;
            if (distance > 0.01 && focusCentering > 0f)
                t = Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime);

            if (distance > focusRadius)
            {
                t = Mathf.Min(focusRadius / distance, t);
            }

            focusPoint = Vector3.Lerp(targetPoint, focusPoint, t);
        }
        else
        {
            focusPoint = targetPoint;
        }
    }

    bool ManualRotation()
    {
        Vector2 input = new Vector2(Input.GetAxis("Vertical Camera"), Input.GetAxis("Horizontal Camera"));
        const float e = 0.001f;
        if (input.x < -e || input.x > e || input.y < -e || input.y > e)
        {
            orbitAngles += rotationSpeed * Time.unscaledDeltaTime * input;
            lastTimeManualChange = Time.unscaledTime;
            return true;
        }
        return false;

    }

    void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);
        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }

    }

    bool AutomaticRotation()
    {
        if (Time.unscaledTime - lastTimeManualChange < alignDelay)
        {
            return false;
        }

        // automatic aligment custom gravity
        Vector3 alignedDelta =
            Quaternion.Inverse(gravityAlignment) *
            (focusPoint - previousFocusPoint);
        Vector2 movement = new Vector2(alignedDelta.x, alignedDelta.z);

        // Vector2 movement = new Vector2(focusPoint.x - previousFocusPoint.x, focusPoint.z - previousFocusPoint.z);

        float deltaMovementSqrMagnitude = movement.magnitude;

        if (deltaMovementSqrMagnitude < 0.000001f)
        {
            return false;
        }


        float headingAngle = GetAngle(movement.normalized);
        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));
        float rotationChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime, deltaMovementSqrMagnitude);
        if (deltaAbs < alignSmoothRange)
        {
            rotationChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotationChange *= (180f - deltaAbs) / alignSmoothRange;
        }

        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y, headingAngle, rotationChange);

        return true;

    }
    static float GetAngle(Vector2 direction)
    {
        float angle = Mathf.Acos(direction.y) * Mathf.Rad2Deg;
        return direction.x < 0f ? 360f - angle : angle;
    }

    void AutomaticDistance(Vector3 lookDirection, ref Vector3 lookPosition, Quaternion lookRotation)
    {
        Vector3 rectOffset = lookDirection * regularCamera.nearClipPlane;
        Vector3 rectPosition = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPosition - castFrom;
        float castDistance = castLine.magnitude;
        Vector3 castDirection = castLine / castDistance;

        if (Physics.BoxCast(
            castFrom, cameraHalfExtends, castDirection, out RaycastHit hit, lookRotation, castDistance, obstructionMask))
        {
            rectPosition = castFrom + castDirection * hit.distance;
            lookPosition = rectPosition - rectOffset;
        }



    }





}
