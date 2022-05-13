using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using static UnityEngine.Mathf;

public class UVProjection : MonoBehaviour
{
    [SerializeField]
    Transform displayPrefab;

    [SerializeField]
    Transform target;

    [SerializeField]
    bool displayProjection;

    [SerializeField]
    float displayScale;

    Transform display;

    Client client;

    void Start()
    {
        if (displayProjection)
        {
            display = Instantiate(displayPrefab);
            display.localScale = displayScale * Vector3.one;
            display.GetComponent<Renderer>().material.color = Color.red;
        }
        client = transform.GetComponent<Client>();
    }

    void Update()
    {
        Vector3 diff = (transform.position - target.position).normalized;
        if (displayProjection)
        {
            display.position = target.position + diff;
        }
        float v = Asin(diff.y) / (PI * 0.5f);
        float r = Cos(0.5f * PI * v);
        float u = (Acos(diff.z / r) - PI) / (PI);
        u = transform.position.x <= 0f ? -u : u;
        if (client != null && !float.IsNaN(u) && !float.IsNaN(v))
        {
            client.u = u;
            client.v = v;
        }
    }
}
