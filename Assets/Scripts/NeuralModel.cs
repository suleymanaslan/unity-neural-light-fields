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
    bool predictSampling;

    [SerializeField]
    GameObject display;

    [SerializeField]
    NNModel modelSource;

    [SerializeField, Range(1, 2)]
    int resolutionFactor;

    IWorker worker;

    float[,] spatialCoordinates;

    float[,] inputCoordinates;

    bool[,] inputMask;
    bool[,] newMask;

    int maskSum;

    Tensor inputTensor;

    Tensor outputTensor;

    readonly float scaleFactor = 1f;

    int InputResolution => resolution / resolutionFactor;
    int InputSize => InputResolution * InputResolution;

    int[] spatialOffset;
    int[,] textureOffset;

    int offsetIndex = 0;

    Texture2D texture;

    Texture2D prevTexture;

    int maskAnchorPeriod = 64;
    int maskFilterSize = 3;

    System.Func<Color, float> meanRGB = c => (c.r + c.g + c.b) / 3f;

    private void OnEnable()
    {
        spatialOffset = new int[] { 0, 1, resolution, resolution + 1 };
        textureOffset = new int[,] { { 0, 0 }, { 0, 1 }, { 1, 0 }, { 1, 1 } };
        inputMask = new bool[InputResolution, InputResolution];
        maskSum = 0;
        for (int maskI = 0; maskI < InputResolution; maskI++)
        {
            for (int maskJ = 0; maskJ < InputResolution; maskJ++)
            {
                inputMask[maskI, maskJ] = true;
                maskSum++;
            }
        }
        inputCoordinates = new float[maskSum, 4];
        texture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Point
        };
        prevTexture = new Texture2D(resolution, resolution, TextureFormat.ARGB32, false)
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
        inputTensor = new Tensor(maskSum, 4, inputCoordinates);
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
        int outputI = 0;
        if (predictSampling)
        {
            newMask = new bool[InputResolution, InputResolution];
            maskSum = 0;
        }
        for (int maskI = 0; maskI < InputResolution; maskI++)
        {
            for (int maskJ = 0; maskJ < InputResolution; maskJ++)
            {
                if (inputMask[maskI, maskJ])
                {
                    Color outputColor = GetOutputColor(outputI);
                    texture.SetPixel(maskJ * resolutionFactor + textureOffset[offsetIndex, 0], resolution - 1 - (maskI * resolutionFactor + textureOffset[offsetIndex, 1]), outputColor);
                    outputI++;
                    if (predictSampling)
                    {
                        Color prevColor = prevTexture.GetPixel(maskJ * resolutionFactor + textureOffset[offsetIndex, 0], resolution - 1 - (maskI * resolutionFactor + textureOffset[offsetIndex, 1]));
                        if (Mathf.Abs(meanRGB(outputColor) - meanRGB(prevColor)) > 0.01f || (maskJ + maskAnchorPeriod / 2) % maskAnchorPeriod == 0 || (maskI + maskAnchorPeriod / 2) % maskAnchorPeriod == 0)
                        {
                            for (int offsetI = 0; offsetI < maskFilterSize; offsetI++)
                            {
                                for (int offsetJ = 0; offsetJ < maskFilterSize; offsetJ++)
                                {
                                    int windowI = maskI + offsetI - (maskFilterSize - 1) / 2;
                                    int windowJ = maskJ + offsetJ - (maskFilterSize - 1) / 2;
                                    windowI = windowI < 0 ? 0 : windowI;
                                    windowI = windowI >= InputResolution ? InputResolution - 1 : windowI;
                                    windowJ = windowJ < 0 ? 0 : windowJ;
                                    windowJ = windowJ >= InputResolution ? InputResolution - 1 : windowJ;
                                    if (!newMask[windowI, windowJ])
                                    {
                                        maskSum++;
                                    }
                                    newMask[windowI, windowJ] = true;
                                }
                            }
                        }
                    }
                }
            }
        }
        texture.Apply();
        if (predictSampling)
        {
            Graphics.CopyTexture(texture, prevTexture);
            for (int maskI = 0; maskI < InputResolution; maskI++)
            {
                for (int maskJ = 0; maskJ < InputResolution; maskJ++)
                {
                    inputMask[maskI, maskJ] = newMask[maskI, maskJ];
                }
            }
        }
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
        int inputI = 0;
        int si = spatialOffset[offsetIndex];
        for (int maskI = 0; maskI < InputResolution; maskI++)
        {
            for (int maskJ = 0; maskJ < InputResolution; maskJ++)
            {
                if (inputMask[maskI, maskJ])
                {
                    inputCoordinates[inputI, 0] = curU;
                    inputCoordinates[inputI, 1] = curV;
                    inputCoordinates[inputI, 2] = spatialCoordinates[si, 0];
                    inputCoordinates[inputI, 3] = spatialCoordinates[si, 1];
                    inputI++;
                }
                si += resolutionFactor;
            }
            if (resolutionFactor == 2)
            {
                si += resolution;
            }
        }
    }

    Color GetOutputColor(int i)
    {
        float r = (outputTensor[i * 3] + 1) * 0.5f;
        float g = (outputTensor[i * 3 + 1] + 1f) * 0.5f;
        float b = (outputTensor[i * 3 + 1] + 1) * 0.5f;
        r = r > 0.01 ? r : 0f;
        g = g > 0.01 ? g : 0f;
        b = b > 0.01 ? b : 0f;
        r = r < 0.99 ? r : 1f;
        g = g < 0.99 ? g : 1f;
        b = b < 0.99 ? b : 1f;
        return new Color(r, g, b);
    }

    float GetOutputFloat(Tensor tensor, int i)
    {
        float r = (tensor[i * 3] + 1) * 0.5f;
        float g = (tensor[i * 3 + 1] + 1f) * 0.5f;
        float b = (tensor[i * 3 + 1] + 1) * 0.5f;
        r = r > 0.01 ? r : 0f;
        g = g > 0.01 ? g : 0f;
        b = b > 0.01 ? b : 0f;
        r = r < 0.99 ? r : 1f;
        g = g < 0.99 ? g : 1f;
        b = b < 0.99 ? b : 1f;
        return r + g + b;
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
