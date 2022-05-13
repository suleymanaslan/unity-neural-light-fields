using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{
    [SerializeField]
    Transform cameraTransform;

    void Start()
    {
        
    }

    
    void Update()
    {
        Vector3 lookPoint = transform.position + transform.position - cameraTransform.position;
        transform.position = new Vector3(transform.position.x, cameraTransform.position.y, transform.position.z);
        transform.LookAt(lookPoint);
    }
}
