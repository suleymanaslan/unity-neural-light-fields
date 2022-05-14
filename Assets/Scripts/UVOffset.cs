using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVOffset : MonoBehaviour
{
    public Client client;

    float uOffset, vOffset;

    void Start()
    {
        uOffset = 0f;
        vOffset = 0f;
    }

    void Update()
    {
        var angle = transform.eulerAngles.y;
        if (angle > 180) angle -= 360;
        if (angle < -180) angle += 360;
        uOffset = angle / 180f;
        if (client != null)
        {
            client.uOffset = uOffset;
            client.vOffset = vOffset;
        }
    }
}
