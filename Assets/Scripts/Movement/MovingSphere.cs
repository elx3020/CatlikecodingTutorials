using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSphere : MonoBehaviour
{

    [SerializeField,Range(0f,10f)]
    float maxSpeed = 10f;

    [SerializeField,Range(0f,100f)]

    float maxAcceleration = 10f;

    [SerializeField]

    // 

    Rect allowedArea =  new Rect(-5f,-5f,10f,10f);

    [SerializeField,Range(0f,1f)]
    float bounciness = 0.5f;

    Vector3 velocity;
    private void Update() {

        Vector2 playerInput;
        playerInput.x = Input.GetAxis("Horizontal");
        playerInput.y = Input.GetAxis("Vertical");
        // Vector2.ClampMagnitude method is static form the Vector2 struct. It takes a vector and a float as the clamp value. it return a copy of the vector clamped to the specified value
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);


        // Input Vector is the new vector3 

        Vector3 desiredVelocity = new Vector3(playerInput.x, 0f,playerInput.y) * maxSpeed;
        //  acceleration is v * delta time
        float maxSpeedChange = maxAcceleration * Time.deltaTime;

        // if(velocity.x < desiredVelocity.x){
        //     velocity.x = Mathf.Min(velocity.x + maxSpeedChange,desiredVelocity.x);
        // }else if(velocity.x > desiredVelocity.x){
        //     velocity.x = Mathf.Min(velocity.x - maxSpeedChange, desiredVelocity.x);
        // }

        // move towards method is to make a change between the current and target value with a float indicating the rate to the target value

        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z , maxSpeedChange);
        
        // position or displacement is equal to velocity * delta time
        Vector3 displacement = velocity * Time.deltaTime;
        
        Vector3 newPosition = transform.localPosition + displacement;

        // make sticky walls because velocity is not set to zero when the collision with the edge happens
        // if(!allowedArea.Contains(new Vector2(newPosition.x,newPosition.z))){
        //     newPosition.x = Mathf.Clamp(newPosition.x, allowedArea.xMin,allowedArea.xMax);
        //     newPosition.z = Mathf.Clamp(newPosition.z, allowedArea.yMin, allowedArea.yMax);
        // }

        // constrain so that the sphere doesn't leave a designed region


        if(newPosition.x + (transform.localScale.x / 2f) > allowedArea.xMax){
            newPosition.x = allowedArea.xMax - transform.localScale.x / 2f ;
            velocity.x = -velocity.x * bounciness;

        }else if(newPosition.x - (transform.localScale.x / 2f) < allowedArea.xMin){
            newPosition.x = allowedArea.xMin + transform.localScale.x / 2f;
            velocity.x = -velocity.x * bounciness;

        }else if(newPosition.z + (transform.localScale.z / 2f) > allowedArea.yMax){
            newPosition.z = allowedArea.yMax - transform.localScale.z / 2f;
            velocity.z = -velocity.z * bounciness;

        }else if(newPosition.z - (transform.localScale.z / 2f) < allowedArea.yMin){
            newPosition.z = allowedArea.yMin + transform.localScale.z / 2f;
            velocity.z = -velocity.z * bounciness;
        }
        transform.localPosition = newPosition; 
    }
}
