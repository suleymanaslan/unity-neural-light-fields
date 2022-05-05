using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static UnityEngine.Mathf;

public class UVSphere : MonoBehaviour
{
    [SerializeField]
    Transform gridPrefab;

    readonly int resolution = 64;

    readonly int degrees = 360;

    float SpherePercentage => degrees / 360f;

    Transform[] points;

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        points = new Transform[resolution * resolution];
        for (int i = 0; i < points.Length; i++)
        {
            Transform point = points[i] = Instantiate(gridPrefab);
        }
        float step = 2f / resolution;
        float u = -1f + step / 2f;
        GameObject uChild = new GameObject("" + -u);
        uChild.transform.SetParent(transform, false);
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                u = z * step + step / 2f - 1f;
                uChild = new GameObject("" + -u);
                uChild.transform.SetParent(transform, false);
            }
            float v = (x * step - 1f);
            points[i].SetParent(uChild.transform, false);
            points[i].localPosition = Sphere(-u, -v);
            points[i].gameObject.name = (-u) + "," + (-v);
            points[i].transform.LookAt(transform);
        }
    }

    public Vector3 Sphere(float u, float v)
    {
        float r = Cos(0.45f * PI * SpherePercentage * v);
        Vector3 p;
        p.x = r * Sin(PI + PI * SpherePercentage * u);
        p.y = Sin(PI * SpherePercentage * 0.5f * v);
        p.z = r * Cos(PI + PI * SpherePercentage * u);
        return p;
    }
}
