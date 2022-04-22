using Unity.Barracuda;
using UnityEngine;

public class NeuralModel : MonoBehaviour
{
    [SerializeField, Range(4, 256)]
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

    [SerializeField, Range(1, 2)]
    int resolutionFactor;

    IWorker worker;

    float[,] spatialCoordinates;

    float[,] inputCoordinates;

    Tensor inputTensor;

    Tensor outputTensor;

    readonly float scaleFactor = 1f;

    int InputResolution => resolution / resolutionFactor;
    int InputSize => InputResolution * InputResolution;

    int[] spatialOffset;
    int[,] textureOffset;

    int offsetIndex = 0;

    Texture2D texture;

    private void OnEnable()
    {
        spatialOffset = new int[] { 0, 1, resolution, resolution + 1 };
        textureOffset = new int[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } };
        inputCoordinates = new float[InputSize, 4];
        texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        display.GetComponent<Renderer>().material.mainTexture = texture;

        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.ComputePrecompiled, ModelLoader.Load(modelSource));
        InitializeSpatialCoordinates();
        CreateInputCoordinates();
        ForwardPass();
        CreateTexture();
    }

    private void Update()
    {
        if (resolutionFactor == 2)
        {
            offsetIndex++;
            if (offsetIndex == 4)
            {
                offsetIndex = 0;
            }
        }
        CreateInputCoordinates();
        ForwardPass();
        CreateTexture();
    }

    void ForwardPass()
    {
        inputTensor = new Tensor(resolution * resolution / (resolutionFactor * resolutionFactor), 4, inputCoordinates);
        worker.Execute(inputTensor);
        outputTensor = worker.PeekOutput();
        inputTensor.Dispose();
    }

    Tensor ExecuteInParts(IWorker worker, Tensor I, int syncEveryNthLayer = 32)
    {
        var executor = worker.StartManualSchedule(I);
        var it = 0;
        bool hasMoreWork;

        do
        {
            hasMoreWork = executor.MoveNext();
            if (++it % syncEveryNthLayer == 0)
                worker.FlushSchedule();

        } while (hasMoreWork);

        return worker.PeekOutput();
    }

    public void CreateTexture()
    {
        for (int i = 0; i < InputResolution; i++)
        {
            for (int j = 0; j < InputResolution; j++)
            {
                texture.SetPixel(j * resolutionFactor + textureOffset[offsetIndex, 0], resolution - 1 - (i * resolutionFactor + textureOffset[offsetIndex, 1]), GetOutputColor(j * InputResolution + i));
            }
        }
        texture.Apply();
    }

    void InitializeSpatialCoordinates()
    {
        int size = resolution * resolution;
        spatialCoordinates = new float[size, 2];
        float step = 2f / (resolution - 1);
        for (int i = 0, x = 0, z = 0; i < size; i++, x++)
        {
            if (x == resolution)
            {
                x = 0;
                z++;
            }
            spatialCoordinates[i, 0] = -1f + x * step;
            spatialCoordinates[i, 1] = -1f + z * step;
        }

    }

    void CreateInputCoordinates()
    {
        float curU = interpolatePeriodically ? scaleFactor * Mathf.Sin(Mathf.PI * Time.time * interpolationSpeed) : scaleFactor * u;
        float curV = interpolatePeriodically ? scaleFactor * Mathf.Cos(Mathf.PI * Time.time * interpolationSpeed) : scaleFactor * v;
        for (int i = 0, x = 0, si = spatialOffset[offsetIndex]; i < InputSize; i++, x++, si += resolutionFactor)
        {
            if (x == InputResolution)
            {
                x = 0;
                if (resolutionFactor == 2)
                {
                    si += resolution;
                }
            }
            inputCoordinates[i, 0] = curU;
            inputCoordinates[i, 1] = curV;
            inputCoordinates[i, 2] = spatialCoordinates[si, 0];
            inputCoordinates[i, 3] = spatialCoordinates[si, 1];
        }
    }

    Color GetOutputColor(int i)
    {
        return new Color((outputTensor[i * 3] + 1) * 0.5f, (outputTensor[i * 3 + 1] + 1f) * 0.5f, (outputTensor[i * 3 + 1] + 1) * 0.5f);
    }

    void PrintSpatialCoordinates(int i)
    {
        Debug.Log(spatialCoordinates[i, 0] + ", " + spatialCoordinates[i, 1]);
    }

    void PrintCoordinates(int i)
    {
        Debug.Log(inputCoordinates[i, 0] + ", " + inputCoordinates[i, 1] + ", " + inputCoordinates[i, 2] + ", " + inputCoordinates[i, 3]);
    }

    private void OnDisable()
    {
        outputTensor?.Dispose();
        inputTensor?.Dispose();
        worker?.Dispose();
    }

}
