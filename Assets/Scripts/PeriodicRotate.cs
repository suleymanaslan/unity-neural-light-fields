using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PeriodicRotate : MonoBehaviour
{
    [SerializeField]
    bool reverseDirection;

    [SerializeField]
    float speed;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (reverseDirection)
        {
            transform.Rotate(0, speed * Time.deltaTime, 0);
        }
        else
        {
            transform.Rotate(0, -speed * Time.deltaTime, 0);
        }
    }
}
