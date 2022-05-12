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

    Transform display;

    Client client;

    void Start()
    {
        display = Instantiate(displayPrefab);
        display.localScale = 0.1f * Vector3.one;
        display.GetComponent<Renderer>().material.color = Color.red;
        client = transform.GetComponent<Client>();
    }

    void Update()
    {
        Vector3 diff = (transform.position - target.position).normalized;
        display.position = target.position + diff;
        float v = Asin(diff.y) / (PI * 0.5f);
        float r = Cos(0.45f * PI * v);
        float u = (Acos(diff.z / r) - PI) / (PI);
        u = transform.position.x <= 0f ? u : -u;
        client.u = -u;
        client.v = -v;
    }
}
