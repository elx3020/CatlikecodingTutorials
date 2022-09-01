using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public static class GravitityController
{
    [SerializeField]


    public delegate Vector3 GetGravity(Vector3 x,out Vector3 upAxis);
    public static GetGravity[] functions = { sphericalGravity, downGravity };
    public enum GravityType { sphericalGravity, downGravity };



    public static GetGravity GetFunction(GravityType name){
        return functions[(int)name];
    }


    public static Vector3 sphericalGravity(Vector3 x, out Vector3 upAxis){
        upAxis = new Vector3();

        return CustomGravity.GetGravity(x, out upAxis);

    }


    public static Vector3 downGravity(Vector3 x, out Vector3 upAxis){
        upAxis = new Vector3(0,1,0);
        return Physics.gravity;
    }







}
