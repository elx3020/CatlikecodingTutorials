using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphReview : MonoBehaviour
{

    [SerializeField]

    Transform pointPrefab;

    [SerializeField,Range(10,100)]

    int resolution = 10;
    Transform[] points;


    private void Awake() {

        int step = resolution / 2;
        
        points = new Transform[resolution];

        Vector3 position = Vector3.zero;
        Vector3 scale = Vector3.one / step;


        for(int i = 0;i < points.Length;i++){
            Transform point = points[i] = Instantiate(pointPrefab);

            position.x = (i+0.5f) * (1f/step) - 1;

            position.y = Mathf.Sin(Mathf.PI * position.x);

            point.localPosition = position;
            point.localScale = scale;

            point.SetParent(transform,false);


        }

        
    }




   
}
