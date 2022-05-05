using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Mathf;

public class UVProjection : MonoBehaviour
{
    [SerializeField]
    Transform tempPrefab;

    [SerializeField]
    Transform target;

    Transform display;

    void Start()
    {
        display = Instantiate(tempPrefab);
        display.localScale = 0.1f * Vector3.one;
        display.GetComponent<Renderer>().material.color = Color.red;
    }

    void Update()
    {
        Vector3 diff = (transform.position - target.position).normalized;
        display.position = target.position + diff;
        float v = Asin(diff.y) / (PI * 0.5f);
        float r = Cos(0.45f * PI * v);
        float u = (Acos(diff.z / r) - PI) / (PI);
        u = transform.position.x <= 0f ? u : -u;
        Debug.Log(u + "," + v);
    }
}
