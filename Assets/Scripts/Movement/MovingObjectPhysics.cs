using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObjectPhysics : MonoBehaviour
{
    [SerializeField]
    Transform playerInputSpace = default;
    [SerializeField, Range(0f, 10f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]

    float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 90f)]

    float maxGroundAngle = 25f, maxStairsAngle = 50f;

    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;
    [SerializeField, Range(0f, 1f)]
    float probeDistance = 1f;

    [SerializeField]

    LayerMask probeMask = -1, stairMask = -1;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;


    [SerializeField, Range(0, 5)]

    int maxAirJumps = 1;

    Vector3 contactNormals, steepNormal;

    // keep reference of world gravity vector direction
    Vector3 upAxis;


    int jumpPhase;

    bool desiredJump;

    int groundContactCount, steepContactCount;

    bool OnGround => groundContactCount > 0;

    bool OnSteep => steepContactCount > 0;


    float minGroundDotProduct, minStairsDotProduct;

    int stepsSinceLastGrounded, stepsSinceLastJump;


    Rigidbody body = default;
    Vector3 velocity, desiredVelocity;



    // Stablish the initial condition of the ground steepness whether is counted as ground or not.
    void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
        minStairsDotProduct = Mathf.Cos(maxStairsAngle * Mathf.Deg2Rad);

    }

    // get references of rigidbody and call the on validate method. 
    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    // Check player input and clamp its value using ClampMagnitude.
    // 
    private void Update()
    {
        // player input store in vector 2d

        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        // Vector2.ClampMagnitude method is static form the Vector2 struct. It takes a vector and a float as the clamp value. it return a copy of the vector clamped to the specified value
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);

        // velocity to be added to the rigid body 

        // align input direction with camera view direction

        if (playerInputSpace)
        {
            Vector3 forward = playerInputSpace.forward;
            forward.y = 0f;
            forward.Normalize();
            Vector3 right = playerInputSpace.right;
            right.y = 0f;
            right.Normalize();
            desiredVelocity = (forward * playerInput.y + right * playerInput.x) * maxSpeed;
            // forward direction with the camera forward vector.
            // desiredVelocity = playerInputSpace.TransformDirection(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }
        else
        {
            // assumes forward direction is +z
            desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;
        }




        // boolian from which a jump is executed;
        desiredJump |= Input.GetButtonDown("Jump");


        // Debugging: Color black when in touch with any surface and white if the rigidbody is in the air.
        Color spherecolor = OnGround ? Color.black : Color.white;
        GetComponent<Renderer>().material.SetColor("_BaseColor", spherecolor);


    }

    // physics movement and correction depending on context
    private void FixedUpdate()
    {
        // get the upaxis (opposite direction of gravity vector)
        upAxis = -Physics.gravity.normalized;
        // control the change of state (ground,air)
        UpdateState();
        // adjust velocity in cases of change of slope 
        AdjustVelocity();
        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }
        body.velocity = velocity;
        ClearState();
    }



    void UpdateState()
    {
        velocity = body.velocity;
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;
        if (OnGround || SnapToGround() || CheckSteepContacts())
        {
            stepsSinceLastGrounded = 0;
            if (stepsSinceLastJump > 1)
            {
                jumpPhase = 0;
            }
            if (groundContactCount > 1)
            {
                contactNormals.Normalize();
            }

        }
        else
        {
            contactNormals = upAxis;
        }
    }


    void ClearState()
    {
        groundContactCount = steepContactCount = 0;
        contactNormals = steepNormal = Vector3.zero;

    }

    void Jump()
    {
        Vector3 jumpDirection;
        if (OnGround)
        {
            jumpDirection = contactNormals;
        }
        else if (OnSteep)
        {
            jumpDirection = steepNormal;
            jumpPhase = 0;
        }
        else if (maxAirJumps > 0 && jumpPhase <= maxAirJumps)
        {
            if (jumpPhase == 0)
            {
                jumpPhase = 1;
            }
            jumpDirection = contactNormals;
        }
        else
        {
            return;
        }

        stepsSinceLastJump = 0;
        jumpPhase += 1;
        float jumpSpeed = Mathf.Sqrt(2f * Physics.gravity.magnitude * jumpHeight);
        jumpDirection = (jumpDirection + upAxis).normalized;
        float alignedSpeed = Vector3.Dot(velocity, jumpDirection);
        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += jumpDirection * jumpSpeed;


    }



    void OnCollisionEnter(Collision collision)
    {
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        EvaluateCollision(collision);
    }

    float GetMinDot(int layer)
    {
        return (stairMask & (1 << layer)) == 0 ? minGroundDotProduct : minStairsDotProduct;
    }

    void EvaluateCollision(Collision collision)
    {
        float minDot = GetMinDot(collision.gameObject.layer);
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            float upDot = Vector3.Dot(upAxis, normal);
            if (upDot >= minDot)
            {
                groundContactCount += 1;
                contactNormals += normal;
            }
            else if (upDot > -0.01)
            {
                steepContactCount += 1;
                steepNormal += normal;
            }
        }
    }

    // obtain a vector projected in the plane

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {

        return vector - contactNormals * Vector3.Dot(vector, contactNormals);
    }

    // adjust the desired velocity by calculating vector velocity direction to be parallel to the current surface
    void AdjustVelocity()
    {
        Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
        Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;


        float currentX = Vector3.Dot(velocity, xAxis);
        float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;

        float maxSpeedChange = acceleration * Time.deltaTime;

        float newX = Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
        float newZ = Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);

    }


    // allows to correct micro jumps in steep surfaces
    bool SnapToGround()
    {
        // not snap if time in ground is larger than 1 or if time in air in air is smaller to two
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }

        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }

        if (!Physics.Raycast(body.position, -upAxis, out RaycastHit hit, probeDistance + 1f, probeMask))
        {
            return false;
        }

        float upDot = Vector3.Dot(upAxis, hit.normal);

        if (upDot < GetMinDot(hit.collider.gameObject.layer))
        {
            return false;
        }

        groundContactCount = 1;
        contactNormals = hit.normal;
        float dot = Vector3.Dot(velocity, hit.normal);
        if (dot > 0f)
        {
            velocity = (velocity - hit.normal * dot).normalized * speed;
        }
        return true;

    }

    bool CheckSteepContacts()
    {
        Debug.Log("Check");
        if (steepContactCount > 1)
        {
            steepNormal.Normalize();
            float upDot = Vector3.Dot(upAxis, steepNormal);
            if (upDot >= minGroundDotProduct)
            {
                groundContactCount = 1;
                contactNormals = steepNormal;
                return true;
            }

        }
        return false;
    }


    private void OnDrawGizmos()
    {
        body = GetComponent<Rigidbody>();
        Gizmos.DrawRay(transform.position - upAxis * transform.localScale.x / 2, -upAxis.normalized);

    }








}
