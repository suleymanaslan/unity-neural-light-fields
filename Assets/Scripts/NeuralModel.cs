using Unity.Barracuda;
using UnityEngine;

public class NeuralModel : MonoBehaviour
{
    [SerializeField, Range(16, 256)]
    int resolution = 16;

    [SerializeField, Range(-1f, 1f)]
    float u, v;

    [SerializeField, Range(0f, 2f)]
    float interpolationSpeed = 1f;

    [SerializeField]
    bool interpolatePeriodically;

    [SerializeField]
    GameObject display;

    [SerializeField]
    NNModel modelSource;

    IWorker worker;

    float[,] inputCoordinates;

    Tensor inputTensor;

    Tensor outputTensor;

    float scaleFactor = 1f;

    private void OnEnable()
    {
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, ModelLoader.Load(modelSource));
    }

    private void Update()
    {
        ForwardPass();
        display.GetComponent<Renderer>().material.mainTexture = CreateTexture();
    }

    void ForwardPass()
    {
        CreateImageArray();
        inputTensor = new Tensor(resolution * resolution, 4, inputCoordinates);
        worker.Execute(inputTensor);
        outputTensor = worker.PeekOutput();
        inputTensor.Dispose();
    }

    public Texture2D CreateTexture()
    {
        var texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;

        for (int i = 0; i < resolution; i++)
        {
            for (int j = 0; j < resolution; j++)
            {
                texture.SetPixel(j, resolution - 1 - i, GetOutputColor(j * resolution + i));
            }
        }
        texture.Apply();
        return texture;
    }

    void CreateImageArray()
    {
        int size = resolution * resolution;
        inputCoordinates = new float[size, 4];
        float step = 2f / (resolution - 1);
        for (int i = 0, x = 0, z = 0; i < size; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
            }
            if (interpolatePeriodically)
            {
                inputCoordinates[i, 0] = scaleFactor * Mathf.Sin(Mathf.PI * Time.time * interpolationSpeed);
                inputCoordinates[i, 1] = scaleFactor * Mathf.Cos(Mathf.PI * Time.time * interpolationSpeed);
            }
            else
            {
                inputCoordinates[i, 0] = u;
                inputCoordinates[i, 1] = v;
            }
            inputCoordinates[i, 2] = -1f + x * step;
            inputCoordinates[i, 3] = -1f + z * step;
        }
    }

    Color GetOutputColor(int i)
    {
        return new Color((outputTensor[i * 3] + 1) * 0.5f, (outputTensor[i * 3 + 1] + 1f) * 0.5f, (outputTensor[i * 3 + 1] + 1) * 0.5f);
    }

    void PrintCoordinates(int i)
    {
        Debug.Log(inputCoordinates[i, 0] + ", " + inputCoordinates[i, 1] + ", " + inputCoordinates[i, 2] + ", " + inputCoordinates[i, 3]);
    }

    private void OnDisable()
    {
        outputTensor.Dispose();
        inputTensor.Dispose();
        worker.Dispose();
    }

}
