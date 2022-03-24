using UnityEngine;

using static UnityEngine.Mathf;

public class LFController : MonoBehaviour
{
    [SerializeField]
    Transform target;

    [SerializeField]
    Transform gridPrefab;

    [SerializeField, Range(4, 256)]
    int resolution;

    [SerializeField, Range(15, 165)]
    int degrees;

    float SpherePercentage => degrees / 360f;

    bool paused;

    private void Awake()
    {

    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            transform.position = target.position + target.forward;
            transform.LookAt(target);
            if (paused)
            {
                Time.timeScale = 1;
            }
            else
            {
                Time.timeScale = 0;
            }
            paused = !paused;
        }
        if (Input.GetKeyDown(KeyCode.G))
        {
            CreateGrid();
        }
    }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            foreach (Transform child in transform)
            {
                foreach (Transform camTransform in child)
                {
                    camTransform.GetComponent<CameraCapture>().CamCapture();
                }
            }
        }
    }

    void CreateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        Transform point = Instantiate(gridPrefab);
        float step = 2f / (resolution - 1);
        float u = -1f;
        GameObject uChild = new GameObject("-1");
        uChild.transform.SetParent(transform, false);
        for (int i = 0, x = 0, z = 0; i < resolution * resolution; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z += 1;
                u = z * step - 1f;
                uChild = new GameObject("" + u);
                uChild.transform.SetParent(transform, false);
            }
            float v = (x * step - 1f);
            point.SetParent(uChild.transform, false);
            point.localPosition = Sphere(-u, -v);
            point.gameObject.name = x.ToString("D3") + "-" + z.ToString("D3");
            point.transform.LookAt(target);
            point.GetComponent<CameraCapture>().CamCapture();
        }
        transform.position = target.position;
    }

    public Vector3 Sphere(float u, float v)
    {
        float r = Cos(0.5f * PI * SpherePercentage * v);
        Vector3 p;
        p.x = r * Sin(PI + PI * SpherePercentage * u);
        p.y = Sin(PI * SpherePercentage * 0.5f * v);
        p.z = r * Cos(PI + PI * SpherePercentage * u);
        return p;
    }
}
