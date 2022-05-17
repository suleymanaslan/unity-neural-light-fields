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
            //transform.localEulerAngles = new Vector3(180 * Mathf.Sin(Time.time), transform.localEulerAngles.y, transform.localEulerAngles.z);
            transform.localRotation = Quaternion.Euler(80 * Mathf.Sin(0.5f * Time.time), transform.localEulerAngles.y - speed * Time.deltaTime, transform.localEulerAngles.z);
            //transform.Rotate(0, -speed * Time.deltaTime, 0);
        }
    }
}
