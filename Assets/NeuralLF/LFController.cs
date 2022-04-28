using UnityEngine;
using UnityEngine.InputSystem;

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

    [SerializeField, Range(1.0f, 2.5f)]
    float scale;

    float SpherePercentage => degrees / 360f;

    bool paused;

    bool animate;

    Transform animatedPoint;

    Transform[] points;

    private void Awake()
    {
        transform.localScale = scale * Vector3.one;
    }

    void Start()
    {

    }

    void Update()
    {
        
        if (Keyboard.current.pKey.wasReleasedThisFrame)
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
        if (Keyboard.current.iKey.wasReleasedThisFrame)
        if (Input.GetKeyDown(KeyCode.I))
        {
            IterateGrid();
        }
        if (Keyboard.current.gKey.wasReleasedThisFrame)
        if (Input.GetKeyDown(KeyCode.G))
        {
            CreateGrid();
        }
        {
            animate = true;
            animatedPoint = Instantiate(gridPrefab);
            animatedPoint.SetParent(transform, false);
        }
        if (animate)
        {
            AnimateGrid();
        }
    }

    private void LateUpdate()
    {
        if (Keyboard.current.cKey.wasReleasedThisFrame)
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

    void AnimateGrid()
    {
        animatedPoint.localPosition = TimeSphere();
        animatedPoint.transform.LookAt(target);
    }

    void IterateGrid()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
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
            Transform point = Instantiate(gridPrefab);
            float v = (x * step - 1f);
            point.SetParent(uChild.transform, false);
            point.localPosition = Sphere(-u, -v);
            point.gameObject.name = x.ToString("D3") + "-" + z.ToString("D3");
            point.transform.LookAt(target);
            point.GetComponent<CameraCapture>().CamCapture();
            Destroy(point.gameObject);
        }
        transform.position = target.position;
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
        float step = 2f / (resolution - 1);
        float u = -1f;
        GameObject uChild = new GameObject("-1");
        uChild.transform.SetParent(transform, false);
        for (int i = 0, x = 0, z = 0; i < points.Length; i++, x++)
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
            points[i].SetParent(uChild.transform, false);
            points[i].localPosition = Sphere(-u, -v);
            points[i].gameObject.name = "(" + u + "," + v + ")";
            points[i].transform.LookAt(target);
        }
        transform.position = target.position;
    }

    public Vector3 TimeSphere()
    {
        float t = Sin(PI * Time.unscaledTime * 0.25f);
        float t2 = t * 4;
        float r = Cos(0.5f * PI * SpherePercentage * t);
        Vector3 p;
        p.x = r * Sin(PI + PI * SpherePercentage * t2);
        p.y = Sin(PI * SpherePercentage * 0.5f * t);
        p.z = r * Cos(PI + PI * SpherePercentage * t2);
        return p;
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
