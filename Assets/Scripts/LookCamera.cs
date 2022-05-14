using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{
    [SerializeField]
    Transform cameraTransform;

    [SerializeField]
    bool reverseDirection;

    void Start()
    {
        
    }

    
    void Update()
    {
        Vector3 lookPoint;
        if (reverseDirection)
        {
            lookPoint = transform.position - (transform.position - cameraTransform.position);
        }
        else
        {
            lookPoint = transform.position + (transform.position - cameraTransform.position);
        }
        transform.position = new Vector3(transform.position.x, cameraTransform.position.y, transform.position.z);
        transform.LookAt(lookPoint);
    }
}
